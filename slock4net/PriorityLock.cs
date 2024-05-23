using slock4net.Commands;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class PriorityLock : AbstractExecution
    {
        private byte priority;
        private Lock priorityLock;

        public byte Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public PriorityLock(SlockDatabase database, byte[] lockKey, byte priority, uint timeout, uint expried, ushort count)
            : base(database, lockKey, timeout, expried, (ushort)(count > 0 ? count - 1 : 0), (byte)0)
        {
            this.priority = priority;
        }

        public PriorityLock(SlockDatabase database, byte[] lockKey, byte priority, uint timeout, uint expried)
            : this(database, lockKey, priority, timeout, expried, (ushort)0)
        {
        }

        public PriorityLock(SlockDatabase database, string lockKey, byte priority, uint timeout, uint expried, ushort count)
            : this(database, Encoding.UTF8.GetBytes(lockKey), priority, timeout, expried, count)
        {
        }

        public PriorityLock(SlockDatabase database, string lockKey, byte priority, uint timeout, uint expried)
            : this(database, Encoding.UTF8.GetBytes(lockKey), priority, timeout, expried, (ushort)0)
        {
        }

        public void Acquire()
        {
            lock (this)
            {
                if (priorityLock == null)
                {
                    priorityLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout | 0x00100000, expried, count, priority);
                }
            }
            priorityLock.Acquire();
        }

        public async Task AcquireAsync()
        {
            lock (this)
            {
                if (priorityLock == null)
                {
                    priorityLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout | 0x00100000, expried, count, priority);
                }
            }
            await priorityLock.AcquireAsync();
        }

        public void Release()
        {
            lock (this)
            {
                if (priorityLock == null)
                {
                    priorityLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout | 0x00100000, expried, count, priority);
                }
            }
            priorityLock.Release();
        }

        public async Task ReleaseAsync()
        {
            lock (this)
            {
                if (priorityLock == null)
                {
                    priorityLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout | 0x00100000, expried, count, priority);
                }
            }
            await priorityLock.ReleaseAsync();
        }
    }
}
