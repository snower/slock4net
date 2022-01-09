using System;
using System.Collections.Generic;
using System.Text;

namespace slock4net
{
    class Database
    {
        public IClient Client { get; protected set; }
        public byte DatabaseId { get; protected set; }

        public Database(IClient client, byte databaseId)
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

        public Event NewEvent(byte[] eventKey, UInt32 timeout, UInt32 expried, bool defaultSeted)
        {
            return new Event(this, eventKey, timeout, expried, defaultSeted);
        }

        public ReentrantLock NewReentrantLock(byte[] lockKey, UInt32 timeout, UInt32 expried)
        {
            return new ReentrantLock(this, lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(byte[] lockKey, UInt32 timeout, UInt32 expried)
        {
            return new ReadWriteLock(this, lockKey, timeout, expried);
        }

        public Semaphore NewSemaphore(byte[] semaphoreKey, UInt16 count, UInt32 timeout, UInt32 expried)
        {
            return new Semaphore(this, semaphoreKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, UInt16 count, UInt32 timeout, UInt32 expried)
        {
            return new MaxConcurrentFlow(this, flowKey, count, timeout, expried);
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, UInt16 count, UInt32 timeout, double period)
        {
            return new TokenBucketFlow(this, flowKey, count, timeout, period);
        }
    }
}
