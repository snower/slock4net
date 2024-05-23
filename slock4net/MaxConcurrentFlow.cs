using slock4net.Commands;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class MaxConcurrentFlow : AbstractExecution
    {
        private byte priority;
        private Lock flowLock;

        public byte Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public MaxConcurrentFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, uint expried, byte priority) : base(database, flowKey, timeout, expried, (ushort)(count == 0 ? 0 : (ushort)(count - 1)), 0)
        {
            this.priority = priority;
        }

        public MaxConcurrentFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, uint expried) : this(database, flowKey, count, timeout, expried, 0)
        {
        }

        public MaxConcurrentFlow(SlockDatabase database, string flowKey, ushort count, uint timeout, uint expried, byte priority) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, expried, priority)
        {
        }

        public MaxConcurrentFlow(SlockDatabase database, string flowKey, ushort count, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, expried, 0)
        {
        }

        public void Acquire()
        {
            if (flowLock == null)
            {
                lock (this)
                {
                    if (flowLock == null)
                    {
                        uint timeout = priority > 0 ? this.timeout | 0x00100000 : this.timeout;
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                    }
                }
            }
            flowLock.Acquire();
        }

        public async Task AcquireAsync()
        {
            if (flowLock == null)
            {
                lock (this)
                {
                    if (flowLock == null)
                    {
                        uint timeout = priority > 0 ? this.timeout | 0x00100000 : this.timeout;
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                    }
                }
            }
            await flowLock.AcquireAsync();
        }

        public void Release()
        {
            if (flowLock == null)
            {
                lock (this)
                {
                    if (flowLock == null)
                    {
                        uint timeout = priority > 0 ? this.timeout | 0x00100000 : this.timeout;
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                    }
                }
            }
            flowLock.Release();
        }

        public async Task ReleaseAsync()
        {
            if (flowLock == null)
            {
                lock (this)
                {
                    if (flowLock == null)
                    {
                        uint timeout = priority > 0 ? this.timeout | 0x00100000 : this.timeout;
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                    }
                }
            }
            await flowLock.ReleaseAsync();
        }
    }
}