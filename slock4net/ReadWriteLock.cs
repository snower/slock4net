using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class ReadWriteLock
    {
        private SlockDatabase database;
        private byte[] lockKey;
        private uint timeout;
        private uint expried;
        private LinkedList<Lock> readLocks;
        private Lock writeLock;

        public ReadWriteLock(SlockDatabase database, byte[] lockKey, uint timeout, uint expried)
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
            this.readLocks = new LinkedList<Lock>();
        }

        public ReadWriteLock(SlockDatabase database, string lockKey, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(lockKey), timeout, expried)
        {

        }

        public void AcquireWrite()
        {
            lock (this)
            {
                if (writeLock == null)
                {
                    writeLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0);
                }
            }
            writeLock.Acquire();
        }

        public async Task AcquireWriteAsync()
        {
            lock (this)
            {
                if (writeLock == null)
                {
                    writeLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0);
                }
            }
            await writeLock.AcquireAsync();
        }

        public void ReleaseWrite()
        {
            lock (this)
            {
                if (writeLock == null)
                {
                    writeLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0);
                }
            }
            writeLock.Release();
        }

        public async Task ReleaseWriteAsync()
        {
            lock (this)
            {
                if (writeLock == null)
                {
                    writeLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0, 0);
                }
            }
            await writeLock.ReleaseAsync();
        }

        public void AcquireRead()
        {
            Lock readLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0xffff, 0);
            readLock.Acquire();
            lock (this)
            {
                readLocks.AddLast(readLock);
            }
        }

        public async Task AcquireReadAsync()
        {
            Lock readLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0xffff, 0);
            await readLock.AcquireAsync();
            lock (this)
            {
                readLocks.AddLast(readLock);
            }
        }

        public void ReleaseRead()
        {
            Lock readLock;
            lock (this)
            {
                readLock = readLocks.First.Value;
                if (readLock == null)
                {
                    return;
                }
                try
                {
                    readLocks.RemoveFirst();
                }
                catch (InvalidProgramException)
                {
                }
            }
            readLock.Release();
        }

        public async Task ReleaseReadAsync()
        {
            Lock readLock;
            lock (this)
            {
                readLock = readLocks.First.Value;
                if (readLock == null)
                {
                    return;
                }
                try
                {
                    readLocks.RemoveFirst();
                }
                catch (InvalidProgramException)
                {
                }
            }
            await readLock.ReleaseAsync();
        }

        public void Acquire()
        {
            this.AcquireWrite();
        }

        public async Task AcquireAsync()
        {
            await this.AcquireWriteAsync();
        }

        public void Release()
        {
            this.ReleaseWrite();
        }

        public async Task ReleaseAsync()
        {
            await this.ReleaseWriteAsync();
        }
    }
}