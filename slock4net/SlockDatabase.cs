using System.Text;

namespace slock4net
{
    public class SlockDatabase
    {
        private ushort defaultTimeoutFlag;
        private ushort defaultExpriedFlag;
        public ISlockClient Client { get; protected set; }
        public byte DatabaseId { get; protected set; }
        public ushort DefaultTimeoutFlag { 
            get { return defaultTimeoutFlag; } 
            set { defaultTimeoutFlag = value; } 
        }
        public ushort DefaultExpriedFlag {
            get { return defaultExpriedFlag; }
            set { defaultExpriedFlag = value; } 
        }

        public SlockDatabase(ISlockClient client, byte databaseId, ushort defaultTimeoutFlag, ushort defaultExpriedFlag)
        {
            this.Client = client;
            this.DatabaseId = databaseId;
            this.DefaultTimeoutFlag = defaultTimeoutFlag;
            this.DefaultExpriedFlag = defaultExpriedFlag;
        }

        public void Close()
        {
            this.Client = null;
        }

        private uint MergeTimeoutFlag(uint timeout)
        {
            if (defaultTimeoutFlag != 0)
            {
                timeout |= ((((uint)defaultTimeoutFlag) & 0xffff) << 16);
            }
            return timeout;
        }

        private uint MergeExpriedFlag(uint expried)
        {
            if (defaultExpriedFlag != 0)
            {
                expried |= ((((uint)defaultExpriedFlag) & 0xffff) << 16);
            }
            return expried;
        }

        public Lock NewLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new Lock(this, lockKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public Lock NewLock(string lockKey, uint timeout, uint expried)
        {
            return new Lock(this, Encoding.UTF8.GetBytes(lockKey), MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public Event NewEvent(byte[] eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return new Event(this, eventKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried), defaultSeted);
        }

        public Event NewEvent(string eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            return new Event(this, Encoding.UTF8.GetBytes(eventKey), MergeTimeoutFlag(timeout), MergeExpriedFlag(expried), defaultSeted);
        }

        public ReentrantLock NewReentrantLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new ReentrantLock(this, lockKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public ReentrantLock NewReentrantLock(string lockKey, uint timeout, uint expried)
        {
            return new ReentrantLock(this, Encoding.UTF8.GetBytes(lockKey), MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public ReadWriteLock NewReadWriteLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new ReadWriteLock(this, lockKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public ReadWriteLock NewReadWriteLock(string lockKey, uint timeout, uint expried)
        {
            return new ReadWriteLock(this, Encoding.UTF8.GetBytes(lockKey), MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public Semaphore NewSemaphore(byte[] semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return new Semaphore(this, semaphoreKey, count, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public Semaphore NewSemaphore(string semaphoreKey, ushort count, uint timeout, uint expried)
        {
            return new Semaphore(this, Encoding.UTF8.GetBytes(semaphoreKey), count, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(byte[] flowKey, ushort count, uint timeout, uint expried)
        {
            return new MaxConcurrentFlow(this, flowKey, count, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public MaxConcurrentFlow NewMaxConcurrentFlow(string flowKey, ushort count, uint timeout, uint expried)
        {
            return new MaxConcurrentFlow(this, Encoding.UTF8.GetBytes(flowKey), count, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public TokenBucketFlow NewTokenBucketFlow(byte[] flowKey, ushort count, uint timeout, double period)
        {
            return new TokenBucketFlow(this, flowKey, count, MergeTimeoutFlag(timeout), period);
        }

        public TokenBucketFlow NewTokenBucketFlow(string flowKey, ushort count, uint timeout, double period)
        {
            return new TokenBucketFlow(this, Encoding.UTF8.GetBytes(flowKey), count, MergeTimeoutFlag(timeout), period);
        }

        public GroupEvent NewGroupEvent(byte[] groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return new GroupEvent(this, groupKey, clientId, versionId, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public GroupEvent NewGroupEvent(string groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            return new GroupEvent(this, groupKey, clientId, versionId, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public TreeLock NewTreeLock(byte[] parentKey, byte[] lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, parentKey, lockKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public TreeLock NewTreeLock(string parentKey, string lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, parentKey, lockKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public TreeLock NewTreeLock(byte[] lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, lockKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }

        public TreeLock NewTreeLock(string lockKey, uint timeout, uint expried)
        {
            return new TreeLock(this, lockKey, MergeTimeoutFlag(timeout), MergeExpriedFlag(expried));
        }
    }
}
