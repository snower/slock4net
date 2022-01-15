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
    public class ReadWriteLock
    {
        private SlockDatabase database;
        private byte[] lockKey;
        private UInt32 timeout;
        private UInt32 expried;
        private LinkedList<Lock> readLocks;
        private Lock writeLock;

        public ReadWriteLock(SlockDatabase database, byte[] lockKey, UInt32 timeout, UInt32 expried)
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

        public ReadWriteLock(SlockDatabase database, string lockKey, UInt32 timeout, UInt32 expried) : this(database, Encoding.UTF8.GetBytes(lockKey), timeout, expried)
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

        public void AcquireRead()
        {
            Lock readLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, 0xffff, 0);
            readLock.Acquire();
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

        public void Acquire()
        {
            this.AcquireWrite();
        }

        public void Release()
        {
            this.ReleaseWrite();
        }
    }
}