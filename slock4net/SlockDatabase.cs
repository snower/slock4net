using System;
using System.Collections.Generic;
using System.Text;

namespace slock4net
{
    public class SlockDatabase
    {
        public ISlockClient Client { get; protected set; }
        public byte DatabaseId { get; protected set; }

        public SlockDatabase(ISlockClient client, byte databaseId)
        {
            this.Client = client;
            this.DatabaseId = databaseId;
        }

        public void Close()
        {
            this.Client = null;
        }

        public Lock NewLock(byte[] lockKey, UInt32 timeout, UInt32 expried)
        {
            return new Lock(this, lockKey, timeout, expried);
        }

        public Lock NewLock(string lockKey, UInt32 timeout, UInt32 expried)
        {
            return new Lock(this, Encoding.UTF8.GetBytes(lockKey), timeout, expried);
        }

        public Event NewEvent(byte[] eventKey, UInt32 timeout, UInt32 expried, bool defaultSeted)
        {
            return new Event(this, eventKey, timeout, expried, defaultSeted);
        }

        public Event NewEvent(string eventKey, UInt32 timeout, UInt32 expried, bool defaultSeted)
        {
            return new Event(this, Encoding.UTF8.GetBytes(eventKey), timeout, expried, defaultSeted);
        }

        public ReentrantLock NewReentrantLock(byte[] lockKey, UInt32 timeout, UInt32 expried)
        {
            return new ReentrantLock(this, lockKey, timeout, expried);
        }

        public ReentrantLock NewReentrantLock(string lockKey, UInt32 timeout, UInt32 expried)
        {
            return new ReentrantLock(this, Encoding.UTF8.GetBytes(lockKey), timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(byte[] lockKey, UInt32 timeout, UInt32 expried)
        {
            return new ReadWriteLock(this, lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(string lockKey, UInt32 timeout, UInt32 expried)
        {
            return new ReadWriteLock(this, Encoding.UTF8.GetBytes(lockKey), timeout, expried);
        }

        public Semaphore NewSemaphore(byte[] semaphoreKey, UInt16 count, UInt32 timeout, UInt32 expried)
        {
            return new Semaphore(this, semaphoreKey, count, timeout, expried);
        }

        public Semaphore NewSemaphore(string semaphoreKey, UInt16 count, UInt32 timeout, UInt32 expried)
        {
            return new Semaphore(this, Encoding.UTF8.GetBytes(semaphoreKey), count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, UInt16 count, UInt32 timeout, UInt32 expried)
        {
            return new MaxConcurrentFlow(this, flowKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, UInt16 count, UInt32 timeout, UInt32 expried)
        {
            return new MaxConcurrentFlow(this, Encoding.UTF8.GetBytes(flowKey), count, timeout, expried);
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, UInt16 count, UInt32 timeout, double period)
        {
            return new TokenBucketFlow(this, flowKey, count, timeout, period);
        }

        public TokenBucketFlow NewTokenBucketFlow(string flowKey, UInt16 count, UInt32 timeout, double period)
        {
            return new TokenBucketFlow(this, Encoding.UTF8.GetBytes(flowKey), count, timeout, period);
        }
    }
}
