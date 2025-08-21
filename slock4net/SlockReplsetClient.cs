using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace slock4net
{
    public class SlockReplsetClient : ISlockClient
    {
        protected string[] hosts;
        private ushort defaultTimeoutFlag;
        private ushort defaultExpriedFlag;
        protected LinkedList<SlockClient> clients;
        protected LinkedList<SlockClient> livedClients;
        private volatile SlockClient livedLeaderClient;
        protected ConcurrentDictionary<byte[], Command> requests;
        private LinkedList<Command> pendingRequests;
        protected bool closed;
        protected SlockDatabase[] databases;

        public SlockReplsetClient(string[] hosts)
        {
            this.hosts = hosts;
            this.clients = new LinkedList<SlockClient>();
            this.livedClients = new LinkedList<SlockClient>();
            this.requests = new ConcurrentDictionary<byte[], Command>(new SlockClient.ByteArrayComparer());
            this.pendingRequests = new LinkedList<Command>();
            this.closed = false;
            this.databases = new SlockDatabase[256];
        }

        public ConcurrentDictionary<byte[], Command> Requests => requests;
        
        public bool HasLivedClient => livedClients.Count > 0;

        public virtual void SetDefaultTimeoutFlag(ushort defaultTimeoutFlag)
        {
            this.defaultTimeoutFlag = defaultTimeoutFlag;
            foreach (SlockDatabase database in databases)
            {
                if (database != null)
                {
                    database.DefaultTimeoutFlag = defaultTimeoutFlag;
                }
            }
        }

        public virtual void SetDefaultExpriedFlag(ushort defaultExpriedFlag)
        {
            this.defaultExpriedFlag = defaultExpriedFlag;
            foreach (SlockDatabase database in databases)
            {
                if (database != null)
                {
                    database.DefaultExpriedFlag = defaultExpriedFlag;
                }
            }
        }

        public void Open()
        {
            foreach (string host in this.hosts)
            {
                string[] hostInfo = host.StartsWith("[") && host.Contains("]:") ? host.Substring(1).Split("]:") : host.Split(":");
                if (hostInfo.Length != 2)
                {
                    continue;
                }

                SlockClient client = new SlockClient(hostInfo[0], Convert.ToInt32(hostInfo[1]), this, databases);
                this.clients.AddLast(client);
                client.TryOpen();
            }

            if (clients.Count <= 0)
            {
                throw new ClientUnconnectException("clients not connected");
            }
        }

        public Task OpenAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    this.Open();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                    return;
                }
                taskCompletionSource.SetResult(true);
            }, TaskCreationOptions.LongRunning);
            return taskCompletionSource.Task;
        }

        public ISlockClient TryOpen()
        {
            try
            {
                this.Open();
            } catch (Exception)
            {
                return null;
            }
            return this;
        }

        public Task<ISlockClient> TryOpenAsync()
        {
            TaskCompletionSource<ISlockClient> taskCompletionSource = new TaskCompletionSource<ISlockClient>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    taskCompletionSource.SetResult(this.TryOpen());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            }, TaskCreationOptions.LongRunning);
            return taskCompletionSource.Task;
        }

        public void Close()
        {
            this.closed = true;
            ClosePendingRequestCommands();
            foreach (byte[] requestId in this.requests.Keys.ToArray())
            {
                if (this.requests.TryRemove(requestId, out Command command) && command != null)
                {
                    command.CommandResult = null;
                    if (command.RetryType == 2)
                    {
                        RemovePendingRequestCommand(command);
                    }
                    if (!command.WakeupWaiter())
                    {
                        command.WakeupTask();
                    }
                }
            }
            
            foreach (SlockClient client in this.clients)
            {
                
                for (int i = 0; i < databases.Length; i++)
                {
                    if (databases[i] != null)
                    {
                        databases[i].Close();
                    }

                    databases[i] = null;
                }
                
                try
                {
                    client.Close();
                } catch { }
            }
            this.clients.Clear();
        }

        public Task CloseAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    this.Close();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                    return;
                }
                taskCompletionSource.SetResult(true);
            }, TaskCreationOptions.LongRunning);
            return taskCompletionSource.Task;
        }


        public void AddLivedClient(SlockClient client, bool isLeader)
        {
            lock (this)
            {
                this.livedClients.AddLast(client);
                if (isLeader)
                {
                    this.livedLeaderClient = client;
                }
            }
        }

        public void RemoveLivedClient(SlockClient client)
        {
            lock (this)
            {
                this.livedClients.Remove(client);
                if (client == this.livedLeaderClient)
                {
                    this.livedLeaderClient = null;
                }
            }
        }
        
        public void AddLivedLeaderClient(SlockClient client) {
            lock (this) {
                this.livedLeaderClient = client;
            }
            this.WakeupPendingRequestCommands(client);
        }

        public void RemoveLivedLeaderClient(SlockClient client) {
            lock (this) {
                if (client.Equals(this.livedLeaderClient)) {
                    this.livedLeaderClient = null;
                }
            }
        }
        
        public bool DoPendingRequestCommand(SlockClient client, Command command) {
            if (closed) return false;
            if (livedLeaderClient == null && livedClients.Count <= 0) return false;

            lock (this.pendingRequests)
            {
                if (command.RetryType < 1)
                {
                    try
                    {
                        SlockClient currentClient = livedLeaderClient;
                        if (currentClient == null)
                        {
                            currentClient = livedClients.First.Value;
                        }

                        if (currentClient != client)
                        {
                            currentClient.WriteCommand(command);
                            command.RetryType = 1;
                            return true;
                        }
                    }
                    catch (Exception ignored)
                    {
                    }
                }

                this.pendingRequests.AddLast(command);
                command.RetryType = 2;
                return true;
            }
        }

        public void RemovePendingRequestCommand(Command command) {
            lock (this.pendingRequests)
            {
                this.pendingRequests.Remove(command);
            }
        }

        public void WakeupPendingRequestCommands(SlockClient client) {
            lock (this.pendingRequests)
            {
                while (this.pendingRequests.First != null)
                {
                    Command command = this.pendingRequests.First.Value;
                    this.pendingRequests.RemoveFirst();
                    if (command == null) break;

                    command.RetryType = 3;
                    try
                    {
                        client.WriteCommand(command);
                    }
                    catch (Exception e)
                    {
                        command.exception = e;
                        command.WakeupWaiter();
                    }
                }
            }
        }
        
        public void ClosePendingRequestCommands() {
            lock (this.pendingRequests)
            {
                while (this.pendingRequests.First != null)
                {
                    Command command = this.pendingRequests.First.Value;
                    this.pendingRequests.RemoveFirst();
                    if (command == null) break;

                    command.RetryType = 3;
                    command.exception = new ClientClosedException("client closed");
                    command.WakeupWaiter();
                }
            }
        }

        public CommandResult SendCommand(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException("client has been closed");
            }

            SlockClient livedLeaderClient = this.livedLeaderClient;
            if (livedLeaderClient != null)
            {
                return livedLeaderClient.SendCommand(command);
            }

            LinkedListNode<SlockClient> firstNode = livedClients.First;
            if(firstNode == null || firstNode.Value == null)
            {
                throw new ClientUnconnectException("clients not connected");
            }
            return firstNode.Value.SendCommand(command);
        }

        public async Task<CommandResult> SendCommandAsync(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException("client has been closed");
            }

            SlockClient livedLeaderClient = this.livedLeaderClient;
            if (livedLeaderClient != null)
            {
                return await livedLeaderClient.SendCommandAsync(command);
            }

            LinkedListNode<SlockClient> firstNode = livedClients.First;
            if (firstNode == null || firstNode.Value == null)
            {
                throw new ClientUnconnectException("clients not connected");
            }
            return await firstNode.Value.SendCommandAsync(command);
        }

        public void WriteCommand(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException("client has been closed");
            }

            SlockClient livedLeaderClient = this.livedLeaderClient;
            if (livedLeaderClient != null)
            {
                livedLeaderClient.WriteCommand(command);
                return;
            }
            
            LinkedListNode<SlockClient> firstNode = livedClients.First;
            if (firstNode == null || firstNode.Value == null)
            {
                throw new ClientUnconnectException("clients not connected");
            }
            firstNode.Value.WriteCommand(command);
        }

        public bool Ping()
        {
            PingCommand pingCommand = new PingCommand();
            PingCommandResult pingCommandResult = (PingCommandResult)this.SendCommand(pingCommand);
            if (pingCommandResult != null && pingCommandResult.Result == ICommand.COMMAND_RESULT_SUCCED)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> PingAsync()
        {
            PingCommand pingCommand = new PingCommand();
            PingCommandResult pingCommandResult = (PingCommandResult) await this.SendCommandAsync(pingCommand);
            if (pingCommandResult != null && pingCommandResult.Result == ICommand.COMMAND_RESULT_SUCCED)
            {
                return true;
            }
            return false;
        }

        public SlockDatabase SelectDatabase(byte databaseId)
        {
            if (this.databases[databaseId] == null)
            {
                lock (this)
                {
                    if (this.databases[databaseId] == null)
                    {
                        this.databases[databaseId] = new SlockDatabase(this, databaseId, defaultTimeoutFlag, defaultExpriedFlag);
                    }
                }
            }
            return this.databases[databaseId];
        }

        public Lock NewLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewLock(lockKey, timeout, expried);
        }

        public Lock NewLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewLock(lockKey, timeout, expried);
        }

        public Event NewEvent(byte[] eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return this.SelectDatabase(0).NewEvent(eventKey, timeout, expried, defaultSeted);
        }

        public Event NewEvent(string eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return this.SelectDatabase(0).NewEvent(eventKey, timeout, expried, defaultSeted);
        }

        public ReentrantLock NewReentrantLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReentrantLock(lockKey, timeout, expried);
        }

        public ReentrantLock NewReentrantLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReentrantLock(lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReadWriteLock(lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewReadWriteLock(lockKey, timeout, expried);
        }

        public Semaphore NewSemaphore(byte[] semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewSemaphore(semaphoreKey, count, timeout, expried);
        }

        public Semaphore NewSemaphore(string semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewSemaphore(semaphoreKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewMaxConcurrentFlow(flowKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, ushort count, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewMaxConcurrentFlow(flowKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried, byte priority)
        {
            return this.SelectDatabase(0).NewMaxConcurrentFlow(flowKey, count, timeout, expried, priority);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, ushort count, uint timeout, uint expried, byte priority)
        {
            return this.SelectDatabase(0).NewMaxConcurrentFlow(flowKey, count, timeout, expried, priority);
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period);
        }

        public TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period);
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period, byte priority)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period, priority);
        }

        public TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period, byte priority)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period, priority);
        }

        public GroupEvent NewGroupEvent(byte[] groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewGroupEvent(groupKey, clientId, versionId, timeout, expried);
        }

        public GroupEvent NewGroupEvent(string groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewGroupEvent(groupKey, clientId, versionId, timeout, expried);
        }

        public TreeLock NewTreeLock(byte[] parentKey, byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(parentKey, lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(string parentKey, string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(parentKey, lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(lockKey, timeout, expried);
        }

        public PriorityLock NewPriorityLock(byte[] lockKey, byte priority, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewPriorityLock(lockKey, priority, timeout, expried);
        }

        public PriorityLock NewPriorityLock(string lockKey, byte priority, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewPriorityLock(lockKey, priority, timeout, expried);
        }
    }
}
