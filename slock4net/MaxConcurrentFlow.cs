using slock4net.Commands;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class MaxConcurrentFlow : AbstractExecution
    {
        private Lock flowLock;

        public MaxConcurrentFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, uint expried) : base(database, flowKey, timeout, expried, (ushort)(count == 0 ? 0 : (ushort)(count - 1)), 0)
        {
        }

        public MaxConcurrentFlow(SlockDatabase database, string flowKey, ushort count, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, expried)
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
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
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
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
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
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
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
                        flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
                    }
                }
            }
            await flowLock.ReleaseAsync();
        }
    }
}