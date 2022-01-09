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
    class ReentrantLock
    {
        private Database database;
        private byte[] lockKey;
        private UInt32 timeout;
        private UInt32 expried;
        private Lock reentrantLock;

        public ReentrantLock(Database database, byte[] lockKey, UInt32 timeout, UInt32 expried)
        {
            this.database = database;
            if (lockKey.Length > 16)
            {
                using (MD5 md5 = MD5.Create())
                {
                    this.lockKey = md5.ComputeHash(lockKey);
                }
            }
            else
            {
                this.lockKey = new byte[16];
                Array.Copy(lockKey, 0, this.lockKey, 16 - lockKey.Length, lockKey.Length);
            }
            this.timeout = timeout;
            this.expried = expried;
        }

        public void Acquire()
        {
            lock (this)
            {
                if (reentrantLock == null)
                {
                    reentrantLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0xff);
                }
            }
            reentrantLock.Acquire();
        }

        public void Release()
        {
            lock (this)
            {
                if (reentrantLock == null)
                {
                    reentrantLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0xff);
                }
            }
            reentrantLock.Release();
        }
    }
}
