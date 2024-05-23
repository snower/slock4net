using slock4net.Commands;
using System;
using System.Threading.Tasks;

namespace slock4net
{
    public interface ISlockClient
    {
        void SetDefaultTimeoutFlag(ushort defaultTimeoutFlag);
        void SetDefaultExpriedFlag(ushort defaultExpriedFlag);
        public abstract void Open();
        public abstract Task OpenAsync();
        public abstract ISlockClient TryOpen();
        public abstract Task<ISlockClient> TryOpenAsync();
        public abstract void Close();
        public abstract Task CloseAsync();
        public abstract CommandResult SendCommand(Command command);
        public abstract bool Ping();
        public abstract Task<CommandResult> SendCommandAsync(Command command);
        public abstract Task<bool> PingAsync();
        public abstract SlockDatabase SelectDatabase(byte databaseId);
        public abstract Lock NewLock(byte[] lockKey, uint timeout, uint expried);
        public abstract Lock NewLock(string lockKey, uint timeout, uint expried);
        public abstract Event NewEvent(byte[] eventKey, uint timeout, uint expried, bool defaultSeted);
        public abstract Event NewEvent(string eventKey, uint timeout, uint expried, bool defaultSeted);
        public abstract ReentrantLock NewReentrantLock(byte[] lockKey, uint timeout, uint expried);
        public abstract ReentrantLock NewReentrantLock(string lockKey, uint timeout, uint expried);
        public abstract ReadWriteLock NewReadWriteLock(byte[] lockKey, uint timeout, uint expried);
        public abstract ReadWriteLock NewReadWriteLock(string lockKey, uint timeout, uint expried);
        public abstract Semaphore NewSemaphore(byte[] semaphoreKey, ushort count, uint timeout, uint expried);
        public abstract Semaphore NewSemaphore(string semaphoreKey, ushort count, uint timeout, uint expried);
        public abstract MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried);
        public abstract MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, ushort count, uint timeout, uint expried);
        public abstract MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried, byte priority);
        public abstract MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, ushort count, uint timeout, uint expried, byte priority);
        public abstract TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period);
        public abstract TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period);
        public abstract TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period, byte priority);
        public abstract TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period, byte priority);
        public abstract GroupEvent NewGroupEvent(byte[] groupKey, UInt64 clientId, UInt64 versionId, uint timeout, uint expried);
        public abstract GroupEvent NewGroupEvent(string groupKey, UInt64 clientId, UInt64 versionId, uint timeout, uint expried);
        public abstract TreeLock NewTreeLock(byte[] parentKey, byte[] lockKey, uint timeout, uint expried);
        public abstract TreeLock NewTreeLock(string parentKey, string lockKey, uint timeout, uint expried);
        public abstract TreeLock NewTreeLock(byte[] lockKey, uint timeout, uint expried);
        public abstract TreeLock NewTreeLock(string lockKey, uint timeout, uint expried);
        public abstract PriorityLock NewPriorityLock(byte[] lockKey, byte priority, uint timeout, uint expried);
        public abstract PriorityLock NewPriorityLock(string lockKey, byte priority, uint timeout, uint expried);
    }
}
