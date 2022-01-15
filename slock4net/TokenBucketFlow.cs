using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace slock4net
{
    public class TokenBucketFlow
    {
        private SlockDatabase database;
        private byte[] flowKey;
        private UInt16 count;
        private UInt32 timeout;
        private double period;
        private UInt32 expriedFlag;
        private Lock flowLock;

        public TokenBucketFlow(SlockDatabase database, byte[] flowKey, UInt16 count, UInt32 timeout, double period)
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
            this.count = (UInt16)(count > 0 ? count - 1 : 0);
            this.timeout = timeout;
            this.period = period;
        }

        public TokenBucketFlow(SlockDatabase database, string flowKey, UInt16 count, UInt32 timeout, double period) : this(database, Encoding.UTF8.GetBytes(flowKey), count, timeout, period)
        {

        }

        public void SetExpriedFlag(UInt32 expriedFlag)
        {
            this.expriedFlag = expriedFlag;
        }

        public void Acquire()
        {
            if (period < 3)
            {
                lock (this)
                {
                    UInt32 expried = (UInt32)Math.Ceiling(period * 1000) | 0x04000000;
                    expried = expried | (expriedFlag << 16);
                    flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                }
                flowLock.Acquire();
                return;
            }

            lock (this)
            {
                long now = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
                UInt32 expried = (UInt32)(((long)Math.Ceiling(period)) - (now % ((long)Math.Ceiling((period)))));
                expried = expried | (expriedFlag << 16);
                flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), 0, expried, count, 0);
            }

            try
            {
                flowLock.Acquire();
            }
            catch (LockTimeoutException)
            {
                UInt32 expried = (UInt32)Math.Ceiling(period);
                expried = expried | (expriedFlag << 16);
                flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, 0);
                flowLock.Acquire();
            }
        }
    }
}
