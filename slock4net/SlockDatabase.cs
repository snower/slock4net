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

        public Lock NewLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new Lock(this, lockKey, timeout, expried);
        }

        public Lock NewLock(string lockKey, uint timeout, uint expried)
        {
            return new Lock(this, Encoding.UTF8.GetBytes(lockKey), timeout, expried);
        }

        public Event NewEvent(byte[] eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return new Event(this, eventKey, timeout, expried, defaultSeted);
        }

        public Event NewEvent(string eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return new Event(this, Encoding.UTF8.GetBytes(eventKey), timeout, expried, defaultSeted);
        }

        public ReentrantLock NewReentrantLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new ReentrantLock(this, lockKey, timeout, expried);
        }

        public ReentrantLock NewReentrantLock(string lockKey, uint timeout, uint expried)
        {
            return new ReentrantLock(this, Encoding.UTF8.GetBytes(lockKey), timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new ReadWriteLock(this, lockKey, timeout, expried);
        }

        public ReadWriteLock NewReadWriteLock(string lockKey, uint timeout, uint expried)
        {
            return new ReadWriteLock(this, Encoding.UTF8.GetBytes(lockKey), timeout, expried);
        }

        public Semaphore NewSemaphore(byte[] semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return new Semaphore(this, semaphoreKey, count, timeout, expried);
        }

        public Semaphore NewSemaphore(string semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return new Semaphore(this, Encoding.UTF8.GetBytes(semaphoreKey), count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried)
        {
            return new MaxConcurrentFlow(this, flowKey, count, timeout, expried);
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, ushort count, uint timeout, uint expried)
        {
            return new MaxConcurrentFlow(this, Encoding.UTF8.GetBytes(flowKey), count, timeout, expried);
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period)
        {
            return new TokenBucketFlow(this, flowKey, count, timeout, period);
        }

        public TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period)
        {
            return new TokenBucketFlow(this, Encoding.UTF8.GetBytes(flowKey), count, timeout, period);
        }

        public GroupEvent NewGroupEvent(byte[] groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return new GroupEvent(this, groupKey, clientId, versionId, timeout, expried);
        }

        public GroupEvent NewGroupEvent(string groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return new GroupEvent(this, groupKey, clientId, versionId, timeout, expried);
        }

        public TreeLock NewTreeLock(byte[] parentKey, byte[] lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, parentKey, lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(string parentKey, string lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, parentKey, lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, lockKey, timeout, expried);
        }

        public TreeLock NewTreeLock(string lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, lockKey, timeout, expried);
        }
    }
}
