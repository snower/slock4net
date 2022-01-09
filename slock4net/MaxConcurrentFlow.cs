using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace slock4net
{
    class MaxConcurrentFlow
    {
        private Database database;
        private byte[] flowKey;
        private UInt16 count;
        private UInt32 timeout;
        private UInt32 expried;
        private Lock flowLock;

        public MaxConcurrentFlow(Database database, byte[] flowKey, UInt16 count, UInt32 timeout, UInt32 expried)
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
            this.count = (UInt16)(count == 0 ? 0 : (UInt16)(count - 1));
            this.timeout = timeout;
            this.expried = expried;
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
    }
}