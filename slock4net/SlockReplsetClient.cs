using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Generic;

namespace slock4net
{
    public class SlockReplsetClient : ISlockClient
    {
        protected string[] hosts;
        protected LinkedList<SlockClient> clients;
        protected LinkedList<SlockClient> livedClients;
        private volatile SlockClient livedLeaderClient;
        protected bool closed;
        protected SlockDatabase[] databases;

        public SlockReplsetClient(string[] hosts)
        {
            this.hosts = hosts;
            this.clients = new LinkedList<SlockClient>();
            this.livedClients = new LinkedList<SlockClient>();
            this.closed = false;
            this.databases = new SlockDatabase[256];
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
                throw new ClientUnconnectException();
            }
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

        public void Close()
        {
            this.closed = true;
            foreach (SlockClient client in this.clients)
            {
                client.Close();
            }
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

        public CommandResult SendCommand(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException();
            }

            SlockClient livedLeaderClient = this.livedLeaderClient;
            if (livedLeaderClient != null)
            {
                return livedLeaderClient.SendCommand(command);
            }

            LinkedListNode<SlockClient> firstNode = livedClients.First;
            if(firstNode == null || firstNode.Value == null)
            {
                throw new ClientUnconnectException();
            }
            return firstNode.Value.SendCommand(command);
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

        public SlockDatabase SelectDatabase(byte databaseId)
        {
            if (this.databases[databaseId] == null)
            {
                lock (this)
                {
                    if (this.databases[databaseId] == null)
                    {
                        this.databases[databaseId] = new SlockDatabase(this, databaseId);
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

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period);
        }

        public TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period)
        {
            return this.SelectDatabase(0).NewTokenBucketFlow(flowKey, count, timeout, period);
        }

        public GroupEvent newGroupEvent(byte[] groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewGroupEvent(groupKey, clientId, versionId, timeout, expried);
        }

        public GroupEvent newGroupEvent(string groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewGroupEvent(groupKey, clientId, versionId, timeout, expried);
        }

        public TreeLock newTreeLock(byte[] parentKey, byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(parentKey, lockKey, timeout, expried);
        }

        public TreeLock newTreeLock(string parentKey, string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(parentKey, lockKey, timeout, expried);
        }

        public TreeLock newTreeLock(byte[] lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(lockKey, timeout, expried);
        }

        public TreeLock newTreeLock(string lockKey, uint timeout, uint expried)
        {
            return this.SelectDatabase(0).NewTreeLock(lockKey, timeout, expried);
        }
    }
}
