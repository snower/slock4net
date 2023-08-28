using slock4net.Commands;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class MaxConcurrentFlow
    {
        private SlockDatabase database;
        private byte[] flowKey;
        private ushort count;
        private uint timeout;
        private uint expried;
        private Lock flowLock;

        public MaxConcurrentFlow(SlockDatabase database, byte[] flowKey, ushort count, uint timeout, uint expried)
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
            this.count = (ushort)(count == 0 ? 0 : (ushort)(count - 1));
            this.timeout = timeout;
            this.expried = expried;
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
                        flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
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
                        flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
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
                        flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
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
                        flowLock = new Lock(database, flowKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
                    }
                }
            }
            await flowLock.ReleaseAsync();
        }
    }
}