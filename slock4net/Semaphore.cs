using slock4net.Commands;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class Semaphore
    {
        private SlockDatabase database;
        private byte[] semaphoreKey;
        private uint timeout;
        private uint expried;
        private ushort count;

        public Semaphore(SlockDatabase database, byte[] semaphoreKey, ushort count, uint timeout, uint expried)
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
            this.count = (ushort)(count > 0 ? count - 1 : 0);
            this.timeout = timeout;
            this.expried = expried;
        }

        public Semaphore(SlockDatabase database, string semaphoreKey, ushort count, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(semaphoreKey), count, timeout, expried)
        {

        }

        public void Acquire()
        {
            Lock flowLock = new Lock(database, semaphoreKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
            flowLock.Acquire();
        }

        public async Task AcquireAsync()
        {
            Lock flowLock = new Lock(database, semaphoreKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
            await flowLock.AcquireAsync();
        }

        public void Release()
        {
            Lock flowLock = new Lock(database, semaphoreKey, new byte[16], timeout, expried, count, (byte)0);
            flowLock.Release(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED);
        }

        public async Task ReleaseAsync()
        {
            Lock flowLock = new Lock(database, semaphoreKey, new byte[16], timeout, expried, count, (byte)0);
            await flowLock.ReleaseAsync(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED);
        }
    }
}
