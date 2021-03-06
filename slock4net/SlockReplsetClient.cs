using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace slock4net
{
    public class SlockReplsetClient : ISlockClient
    {
        protected String[] hosts;
        protected LinkedList<SlockClient> clients;
        protected LinkedList<SlockClient> livedClients;
        protected bool closed;
        protected SlockDatabase[] databases;

        public SlockReplsetClient(String[] hosts)
        {
            this.hosts = hosts;
            this.clients = new LinkedList<SlockClient>();
            this.livedClients = new LinkedList<SlockClient>();
            this.closed = false;
            this.databases = new SlockDatabase[256];
        }
        public void Open()
        {
            foreach (String host in this.hosts)
            {
                String[] hostInfo = host.Split(":");
                if (hostInfo.Length != 2)
                {
                    continue;
                }

                SlockClient client = new SlockClient(hostInfo[0], Convert.ToInt32(hostInfo[1]), this, databases);
                this.clients.AddLast(client);
                client.TryOpen();
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

        public void AddLivedClient(SlockClient client)
        {
            lock (this)
            {
                this.livedClients.AddLast(client);
            }
        }

        public void RemoveLivedClient(SlockClient client)
        {
            lock (this)
            {
                this.livedClients.Remove(client);
            }
        }

        public CommandResult SendCommand(Command command)
        {
            if (this.closed)
            {
                throw new ClientClosedException();
            }

            LinkedListNode<SlockClient> firstNode = livedClients.First;
            if(firstNode == null || firstNode.Value == null)
            {
                throw new ClientUnconnectException();
            }
            return firstNode.Value.SendCommand(command);
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
    }
}
