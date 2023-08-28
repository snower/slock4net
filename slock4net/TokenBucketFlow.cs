using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class TokenBucketFlow
    {
        private SlockDatabase database;
        private byte[] flowKey;
        private ushort count;
        private uint timeout;
        private double period;
        private uint expriedFlag;
        private Lock flowLock;

        public TokenBucketFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, double period)
        {
            this.database = database;
            if (flowKey.Length > 16)
            {
                using (MD5 md5 = MD5.Create())
                {
                    this.flowKey = md5.ComputeHash(flowKey);
                }
            }
            else
            {
                this.flowKey = new byte[16];
                Array.Copy(flowKey, 0, this.flowKey, 16 - flowKey.Length, flowKey.Length);
            }
            this.count = (ushort)(count > 0 ? count - 1 : 0);
            this.timeout = timeout;
            this.period = period;
        }

        public TokenBucketFlow(SlockDatabase database, string flowKey, ushort count, uint timeout, double period) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, period)
        {

        }

        public void SetExpriedFlag(uint expriedFlag)
        {
            this.expriedFlag = expriedFlag;
        }

        public void Acquire()
        {
            if (period < 3)
            {
                lock (this)
                {
                    uint expried = (uint)Math.Ceiling(period * 1000) | 0x04000000;
                    expried |= (expriedFlag << 16);
                    flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                }
                flowLock.Acquire();
                return;
            }

            lock (this)
            {
                long now = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
                uint expried = (uint)(((long)Math.Ceiling(period)) - (now % ((long)Math.Ceiling((period)))));
                expried |= (expriedFlag << 16);
                flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), 0, expried, count, 0);
            }

            try
            {
                flowLock.Acquire();
            }
            catch (LockTimeoutException)
            {
                uint expried = (uint)Math.Ceiling(period);
                expried |= (expriedFlag << 16);
                flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, 0);
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
                    expried |= (expriedFlag << 16);
                    flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                }
                await flowLock.AcquireAsync();
                return;
            }

            lock (this)
            {
                long now = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
                uint expried = (uint)(((long)Math.Ceiling(period)) - (now % ((long)Math.Ceiling((period)))));
                expried |= (expriedFlag << 16);
                flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), 0, expried, count, 0);
            }

            try
            {
                await flowLock.AcquireAsync();
            }
            catch (LockTimeoutException)
            {
                uint expried = (uint)Math.Ceiling(period);
                expried |= (expriedFlag << 16);
                flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                await flowLock.AcquireAsync();
            }
        }
    }
}
