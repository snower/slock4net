using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.Text;

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
        public abstract Lock NewLock(byte[] lockKey, UInt32 timeout, UInt32 expried);
        public abstract Lock NewLock(string lockKey, UInt32 timeout, UInt32 expried);
        public abstract Event NewEvent(byte[] eventKey, UInt32 timeout, UInt32 expried, bool defaultSeted);
        public abstract Event NewEvent(string eventKey, UInt32 timeout, UInt32 expried, bool defaultSeted);
        public abstract ReentrantLock NewReentrantLock(byte[] lockKey, UInt32 timeout, UInt32 expried);
        public abstract ReentrantLock NewReentrantLock(string lockKey, UInt32 timeout, UInt32 expried);
        public abstract ReadWriteLock NewReadWriteLock(byte[] lockKey, UInt32 timeout, UInt32 expried);
        public abstract ReadWriteLock NewReadWriteLock(string lockKey, UInt32 timeout, UInt32 expried);
        public abstract Semaphore NewSemaphore(byte[] semaphoreKey, UInt16 count, UInt32 timeout, UInt32 expried);
        public abstract Semaphore NewSemaphore(string semaphoreKey, UInt16 count, UInt32 timeout, UInt32 expried);
        public abstract MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, UInt16 count, UInt32 timeout, UInt32 expried);
        public abstract MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, UInt16 count, UInt32 timeout, UInt32 expried);
        public abstract TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, UInt16 count, UInt32 timeout, double period);
        public abstract TokenBucketFlow NewTokenBucketFlow(string flowKey, UInt16 count, UInt32 timeout, double period);
    }
}
