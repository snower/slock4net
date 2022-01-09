using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace slock4net
{
    public class Semaphore
    {
        private Database database;
        private byte[] semaphoreKey;
        private UInt32 timeout;
        private UInt32 expried;
        private UInt16 count;

        public Semaphore(Database database, byte[] semaphoreKey, UInt16 count, UInt32 timeout, UInt32 expried)
        {
            this.database = database;
            if (semaphoreKey.Length > 16)
            {
                using (MD5 md5 = MD5.Create())
                {
                    this.semaphoreKey = md5.ComputeHash(semaphoreKey);
                }
            }
            else
            {
                this.semaphoreKey = new byte[16];
                Array.Copy(semaphoreKey, 0, this.semaphoreKey, 16 - semaphoreKey.Length, semaphoreKey.Length);
            }
            this.count = (UInt16)(count > 0 ? count - 1 : 0);
            this.timeout = timeout;
            this.expried = expried;
        }

        public void Acquire()
        {
            Lock flowLock = new Lock(database, semaphoreKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
            flowLock.Acquire();
        }

        public void Release()
        {
            Lock flowLock = new Lock(database, semaphoreKey, new byte[16], timeout, expried, count, (byte)0);
            flowLock.Release(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED);
        }
    }
}
