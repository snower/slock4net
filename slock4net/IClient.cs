using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace slock4net
{
    interface IClient
    {
        public abstract void Open();
        public abstract void Close();
        public abstract CommandResult SendCommand(Command command);
        public abstract bool Ping();
        public abstract Database SelectDatabase(byte databaseId);
        public abstract Lock NewLock(byte[] lockKey, UInt32 timeout, UInt32 expried);
        public abstract Event NewEvent(byte[] eventKey, UInt32 timeout, UInt32 expried, bool defaultSeted);
        public abstract ReentrantLock NewReentrantLock(byte[] lockKey, UInt32 timeout, UInt32 expried);
        public abstract ReadWriteLock NewReadWriteLock(byte[] lockKey, UInt32 timeout, UInt32 expried);
        public abstract Semaphore NewSemaphore(byte[] semaphoreKey, UInt16 count, UInt32 timeout, UInt32 expried);
        public abstract MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, UInt16 count, UInt32 timeout, UInt32 expried);
        public abstract TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, UInt16 count, UInt32 timeout, double period);
    }
}
