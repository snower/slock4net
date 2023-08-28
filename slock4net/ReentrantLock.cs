using slock4net.Commands;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class ReentrantLock
    {
        private SlockDatabase database;
        private byte[] lockKey;
        private uint timeout;
        private uint expried;
        private Lock reentrantLock;

        public ReentrantLock(SlockDatabase database, byte[] lockKey, uint timeout, uint expried)
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

        public ReentrantLock(SlockDatabase database, string lockKey, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(lockKey), timeout, expried)
        {

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

        public async Task AcquireAsync()
        {
            lock (this)
            {
                if (reentrantLock == null)
                {
                    reentrantLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0xff);
                }
            }
            await reentrantLock.AcquireAsync();
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

        public async Task ReleaseAsync()
        {
            lock (this)
            {
                if (reentrantLock == null)
                {
                    reentrantLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0xff);
                }
            }
            await reentrantLock.ReleaseAsync();
        }
    }
}
