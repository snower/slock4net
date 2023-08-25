using slock4net.Commands;
using System;

namespace slock4net
{
    public interface ISlockClient
    {
        public abstract void Open();
        public abstract ISlockClient TryOpen();
        public abstract void Close();
        public abstract CommandResult SendCommand(Command command);
        public abstract bool Ping();
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
        public abstract TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period);
        public abstract TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period);
        public abstract GroupEvent newGroupEvent(byte[] groupKey, UInt64 clientId, UInt64 versionId, uint timeout, uint expried);
        public abstract GroupEvent newGroupEvent(string groupKey, UInt64 clientId, UInt64 versionId, uint timeout, uint expried);
        public abstract TreeLock newTreeLock(byte[] parentKey, byte[] lockKey, uint timeout, uint expried);
        public abstract TreeLock newTreeLock(string parentKey, string lockKey, uint timeout, uint expried);
        public abstract TreeLock newTreeLock(byte[] lockKey, uint timeout, uint expried);
        public abstract TreeLock newTreeLock(string lockKey, uint timeout, uint expried);
    }
}
