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
        private Lock flowLock;

        public TokenBucketFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, double period) : base(database, flowKey, timeout, 0, (ushort)(count > 0 ? count - 1 : 0), 0)
        {
            this.period = period;
        }

        public TokenBucketFlow(SlockDatabase database, string flowKey, ushort count, uint timeout, double period) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, period)
        {

        }

        public void Acquire()
        {
            if (period < 3)
            {
                lock (this)
                {
                    uint expried = (uint)Math.Ceiling(period * 1000) | 0x04000000;
                    expried |= (this.expried & 0xffff0000);
                    flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                }
                flowLock.Acquire();
                return;
            }

            lock (this)
            {
                long now = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
                uint expried = (uint)(((long)Math.Ceiling(period)) - (now % ((long)Math.Ceiling((period)))));
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), 0, expried, count, 0);
            }

            try
            {
                flowLock.Acquire();
            }
            catch (LockTimeoutException)
            {
                uint expried = (uint)Math.Ceiling(period);
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                flowLock.Acquire();
            }
        }

        public async Task AcquireAsync()
        {
            if (period < 3)
            {
                lock (this)
                {
                    uint expried = (uint)Math.Ceiling(period * 1000) | 0x04000000;
                    expried |= (this.expried & 0xffff0000);
                    flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                }
                await flowLock.AcquireAsync();
                return;
            }

            lock (this)
            {
                long now = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
                uint expried = (uint)(((long)Math.Ceiling(period)) - (now % ((long)Math.Ceiling((period)))));
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), 0, expried, count, 0);
            }

            try
            {
                await flowLock.AcquireAsync();
            }
            catch (LockTimeoutException)
            {
                uint expried = (uint)Math.Ceiling(period);
                expried |= (this.expried & 0xffff0000);
                flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                await flowLock.AcquireAsync();
            }
        }
    }
}
