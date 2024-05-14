using slock4net.Commands;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class ReentrantLock : AbstractExecution
    {
        private Lock reentrantLock;

        public ReentrantLock(SlockDatabase database, byte[] lockKey, uint timeout, uint expried) : base(database, lockKey, timeout, expried)
        {
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
