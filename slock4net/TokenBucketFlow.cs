using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class TokenBucketFlow : AbstractExecution
    {
        private readonly double period;
        private byte priority;
        private Lock flowLock;

        public byte Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public TokenBucketFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, double period, byte priority) : base(database, flowKey, timeout, 0, (ushort)(count > 0 ? count - 1 : 0), 0)
        {
            this.period = period;
            this.priority = priority;
        }

        public TokenBucketFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, double period) : this(database, flowKey, count, timeout, period, 0)
        {
            this.period = period;
        }

        public TokenBucketFlow(SlockDatabase database, string flowKey, ushort count, uint timeout, double period, byte priority) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, period, priority)
        {

        }

        public TokenBucketFlow(SlockDatabase database, string flowKey, ushort count, uint timeout, double period) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, period, 0)
        {

        }

        public void Acquire()
        {
            uint timeout = priority > 0 ? this.timeout | 0x00100000 : this.timeout;
            if (period < 3)
            {
                lock (this)
                {
                    uint expried = (uint)Math.Ceiling(period * 1000) | 0x04000000;
                    expried |= (this.expried & 0xffff0000);
                    flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                }
                flowLock.Acquire();
                return;
            }

            lock (this)
            {
                long now = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
                uint expried = (uint)(((long)Math.Ceiling(period)) - (now % ((long)Math.Ceiling((period)))));
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), 0, expried, count, priority);
            }

            try
            {
                flowLock.Acquire();
            }
            catch (LockTimeoutException)
            {
                uint expried = (uint)Math.Ceiling(period);
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                flowLock.Acquire();
            }
        }

        public async Task AcquireAsync()
        {
            uint timeout = priority > 0 ? this.timeout | 0x00100000 : this.timeout;
            if (period < 3)
            {
                lock (this)
                {
                    uint expried = (uint)Math.Ceiling(period * 1000) | 0x04000000;
                    expried |= (this.expried & 0xffff0000);
                    flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                }
                await flowLock.AcquireAsync();
                return;
            }

            lock (this)
            {
                long now = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
                uint expried = (uint)(((long)Math.Ceiling(period)) - (now % ((long)Math.Ceiling((period)))));
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), 0, expried, count, priority);
            }

            try
            {
                await flowLock.AcquireAsync();
            }
            catch (LockTimeoutException)
            {
                uint expried = (uint)Math.Ceiling(period);
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, priority);
                await flowLock.AcquireAsync();
            }
        }
    }
}
