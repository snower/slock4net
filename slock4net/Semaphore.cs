using slock4net.Commands;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class Semaphore : AbstractExecution
    {
        public Semaphore(SlockDatabase database, byte[] semaphoreKey, ushort count, uint timeout, uint expried) : base(database, semaphoreKey, timeout, expried, (ushort)(count > 0 ? count - 1 : 0), 0)
        {
        }

        public Semaphore(SlockDatabase database, string semaphoreKey, ushort count, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(semaphoreKey), count, timeout, expried)
        {

        }

        public void Acquire()
        {
            Lock flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
            flowLock.Acquire();
        }

        public async Task AcquireAsync()
        {
            Lock flowLock = new Lock(database, lockKey, LockCommand.GenLockId(), timeout, expried, count, (byte)0);
            await flowLock.AcquireAsync();
        }

        public void Release()
        {
            Lock flowLock = new Lock(database, lockKey, new byte[16], timeout, expried, count, (byte)0);
            flowLock.Release(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED);
        }

        public async Task ReleaseAsync()
        {
            Lock flowLock = new Lock(database, lockKey, new byte[16], timeout, expried, count, (byte)0);
            await flowLock.ReleaseAsync(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED);
        }
    }
}
