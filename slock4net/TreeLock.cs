using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class TreeLock : AbstractExecution
    {
        private readonly byte[] parentKey;
        private readonly bool isRoot;
        private TreeLeafLock leafLock;

        public TreeLock(SlockDatabase database, byte[] parentKey, byte[] lockKey, uint timeout, uint expried) : base(database, lockKey, timeout, expried)
        {
            this.parentKey = parentKey;
            this.isRoot = parentKey == null;
            this.leafLock = null;
        }

        public TreeLock(SlockDatabase database, string parentKey, string lockKey, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(parentKey), Encoding.UTF8.GetBytes(lockKey), timeout, expried)
        {
        }

        public TreeLock(SlockDatabase database, byte[] lockKey, uint timeout, uint expried) : this(database, null, lockKey, timeout, expried)
        {
        }

        public TreeLock(SlockDatabase database, string lockKey, uint timeout, uint expried) : this(database, null, Encoding.UTF8.GetBytes(lockKey), timeout, expried)
        {
        }

        public TreeLeafLock NewLeafLock()
        {
            return new TreeLeafLock(database, this, new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, (ushort)0xffff, (byte)0));
        }

        public TreeLeafLock LoadLeafLock(byte[] lockId)
        {
            return new TreeLeafLock(database, this, new Lock(database, lockKey, lockId, timeout, expried, (ushort)0xffff, (byte)0));
        }

        public TreeLock NewChild()
        {
            return new TreeLock(database, lockKey, LockCommand.GenLockId(), timeout, expried);
        }

        public TreeLock LoadChild(byte[] lockKey)
        {
            return new TreeLock(database, this.lockKey, lockKey, timeout, expried);
        }

        public void Acquire()
        {
            Lock checkLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, 0, (ushort)0, (byte)0);
            checkLock.Acquire();

            if (this.leafLock != null) return;
            TreeLeafLock leafLock = NewLeafLock();
            leafLock.Acquire();
            this.leafLock = leafLock;
        }

        public async Task AcquireAsync()
        {
            Lock checkLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, 0, (ushort)0, (byte)0);
            await checkLock.AcquireAsync();

            if (this.leafLock != null) return;
            TreeLeafLock leafLock = NewLeafLock();
            await leafLock.AcquireAsync();
            this.leafLock = leafLock;
        }

        public void Release()
        {
            if (leafLock == null) return;
            leafLock.Release();
            leafLock = null;
        }

        public async Task ReleaseAsync()
        {
            if (leafLock == null) return;
            await leafLock.ReleaseAsync();
            leafLock = null;
        }

        public void Wait(uint timeout)
        {
            Lock checkLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, 0, (ushort)0, (byte)0);
            checkLock.Acquire();
        }

        public async Task WaitAsync(uint timeout)
        {
            Lock checkLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, 0, (ushort)0, (byte)0);
            await checkLock.AcquireAsync();
        }

        public byte[] GetParentKey()
        {
            return parentKey;
        }

        public byte[] GetLockKey()
        {
            return lockKey;
        }

        public bool IsRoot()
        {
            return isRoot;
        }

        public class TreeLeafLock
        {
            private SlockDatabase database;
            private TreeLock treeLock;
            private Lock leafLock;

            public TreeLeafLock(SlockDatabase database, TreeLock treeLock, Lock leafLock)
            {
                this.database = database;
                this.treeLock = treeLock;
                this.leafLock = leafLock;
            }

            public void Acquire()
            {
                Lock childCheckLock = null;
                Lock parentCheckLock = null;

                if (!treeLock.IsRoot())
                {
                    childCheckLock = new Lock(database, treeLock.GetLockKey(), treeLock.GetParentKey(), 0, treeLock.expried, (ushort)0xffff, (byte)0);
                    try
                    {
                        childCheckLock.Acquire(ICommand.LOCK_FLAG_LOCK_TREE_LOCK);
                        parentCheckLock = new Lock(database, treeLock.GetParentKey(), treeLock.GetLockKey(), 0, treeLock.expried, (ushort)0xffff, (byte)0);
                        try
                        {
                            parentCheckLock.Acquire();
                        }
                        catch (LockLockedException)
                        {
                        }
                        catch (Exception)
                        {
                            childCheckLock.Release();
                            throw;
                        }
                    }
                    catch (LockLockedException)
                    {
                    }
                }

                try
                {
                    leafLock.Acquire();
                }
                catch (Exception)
                {
                    if (childCheckLock != null)
                    {
                        try
                        {
                            childCheckLock.Release();
                        }
                        catch (SlockException)
                        {
                        }
                    }
                    if (parentCheckLock != null)
                    {
                        try
                        {
                            parentCheckLock.Release();
                        }
                        catch (SlockException)
                        {
                        }
                    }
                    throw;
                }
            }

            public async Task AcquireAsync()
            {
                Lock childCheckLock = null;
                Lock parentCheckLock = null;

                if (!treeLock.IsRoot())
                {
                    childCheckLock = new Lock(database, treeLock.GetLockKey(), treeLock.GetParentKey(), 0, treeLock.expried, (ushort)0xffff, (byte)0);
                    try
                    {
                        await childCheckLock.AcquireAsync(ICommand.LOCK_FLAG_LOCK_TREE_LOCK);
                        parentCheckLock = new Lock(database, treeLock.GetParentKey(), treeLock.GetLockKey(), 0, treeLock.expried, (ushort)0xffff, (byte)0);
                        try
                        {
                            await parentCheckLock.AcquireAsync();
                        }
                        catch (LockLockedException)
                        {
                        }
                        catch (Exception)
                        {
                            await childCheckLock.ReleaseAsync();
                            throw;
                        }
                    }
                    catch (LockLockedException)
                    {
                    }
                }

                try
                {
                    await leafLock.AcquireAsync();
                }
                catch (Exception)
                {
                    if (childCheckLock != null)
                    {
                        try
                        {
                            await childCheckLock.ReleaseAsync();
                        }
                        catch (SlockException)
                        {
                        }
                    }
                    if (parentCheckLock != null)
                    {
                        try
                        {
                            await parentCheckLock.ReleaseAsync();
                        }
                        catch (SlockException)
                        {
                        }
                    }
                    throw;
                }
            }

            public void Release()
            {
                leafLock.Release(ICommand.UNLOCK_FLAG_UNLOCK_TREE_LOCK);
            }

            public async Task ReleaseAsync()
            {
                await leafLock.ReleaseAsync(ICommand.UNLOCK_FLAG_UNLOCK_TREE_LOCK);
            }

            public byte[] GetLockKey()
            {
                return leafLock.LockKey;
            }

            public byte[] GetLockId()
            {
                return leafLock.LockId;
            }
        }
    }
}
