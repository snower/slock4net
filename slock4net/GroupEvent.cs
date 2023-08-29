using slock4net.Commands;
using slock4net.Exceptions;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class GroupEvent
    {
        private SlockDatabase database;
        private byte[] groupKey;
        private ulong clientId;
        private ulong versionId;
        private uint timeout;
        private uint expried;

        public GroupEvent(SlockDatabase database, byte[] groupKey, ulong clientId, ulong versionId, uint timeout, uint expried)
        {
            this.database = database;
            this.groupKey = groupKey;
            this.clientId = clientId;
            this.versionId = versionId;
            this.timeout = timeout;
            this.expried = expried;
        }

        public GroupEvent(SlockDatabase database, string groupKey, ulong clientId, ulong versionId, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(groupKey), clientId, versionId, timeout, expried)
        {
        }

        public void Clear()
        {
            byte[] lockId = EncodeLockId(0, versionId);
            uint timeout = this.timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16);
            Lock eventLock = new Lock(database, groupKey, lockId, timeout, expried, (ushort)0, (byte)0);
            eventLock.Update();
        }

        public async Task ClearAsync()
        {
            byte[] lockId = EncodeLockId(0, versionId);
            uint timeout = this.timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16);
            Lock eventLock = new Lock(database, groupKey, lockId, timeout, expried, (ushort)0, (byte)0);
            await eventLock.UpdateAsync();
        }


        public void Set()
        {
            Lock eventLock = new Lock(database, groupKey, new byte[16], timeout, expried, (ushort)0, (byte)0);
            try
            {
                eventLock.ReleaseHead();
            }
            catch (LockUnlockedException)
            {
            }
        }

        public async Task SetAsync()
        {
            Lock eventLock = new Lock(database, groupKey, new byte[16], timeout, expried, (ushort)0, (byte)0);
            try
            {
                await eventLock.ReleaseHeadAsync();
            }
            catch (LockUnlockedException)
            {
            }
        }

        public bool IsSet()
        {
            Lock checkLock = new Lock(database, groupKey, LockCommand.GenLockId(), 0, 0, (ushort)0, (byte)0);
            try
            {
                checkLock.Acquire();
            }
            catch (LockTimeoutException)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> IsSetAsync()
        {
            Lock checkLock = new Lock(database, groupKey, LockCommand.GenLockId(), 0, 0, (ushort)0, (byte)0);
            try
            {
                await checkLock.AcquireAsync();
            }
            catch (LockTimeoutException)
            {
                return false;
            }
            return true;
        }

        public void Wakeup()
        {
            byte[] lockId = new byte[16];
            uint timeout = this.timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16);
            Lock eventLock = new Lock(database, groupKey, lockId, timeout, expried, (ushort)0, (byte)0);
            LockCommandResult lockCommandResult = eventLock.ReleaseHeadRetoLockWait();
            byte[] rlockId = lockCommandResult.LockId;
            if (!EqualBytes(lockId, rlockId))
            {
                versionId = ((ulong)rlockId[0]) | (((ulong)rlockId[1]) << 8) | (((ulong)rlockId[2]) << 16) | (((ulong)rlockId[3]) << 24)
                        | (((ulong)rlockId[4]) << 32) | (((ulong)rlockId[5]) << 40) | (((ulong)rlockId[6]) << 48) | (((ulong)rlockId[7]) << 56);
            }
        }

        public async Task WakeupAsync()
        {
            byte[] lockId = new byte[16];
            uint timeout = this.timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16);
            Lock eventLock = new Lock(database, groupKey, lockId, timeout, expried, (ushort)0, (byte)0);
            LockCommandResult lockCommandResult = await eventLock.ReleaseHeadRetoLockWaitAsync();
            byte[] rlockId = lockCommandResult.LockId;
            if (!EqualBytes(lockId, rlockId))
            {
                versionId = ((ulong)rlockId[0]) | (((ulong)rlockId[1]) << 8) | (((ulong)rlockId[2]) << 16) | (((ulong)rlockId[3]) << 24)
                        | (((ulong)rlockId[4]) << 32) | (((ulong)rlockId[5]) << 40) | (((ulong)rlockId[6]) << 48) | (((ulong)rlockId[7]) << 56);
            }
        }

        public void Wait(uint timeout)
        {
            byte[] lockId = EncodeLockId(clientId, versionId);
            Lock waitLock = new Lock(database, groupKey, lockId, timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16),
                    0, 0, (byte)0);
            try
            {
                LockCommandResult lockCommandResult = waitLock.Acquire((byte)0);
                byte[] rlockId = lockCommandResult.LockId;
                if (!EqualBytes(lockId, rlockId))
                {
                    versionId = ((ulong)rlockId[0]) | (((ulong)rlockId[1]) << 8) | (((ulong)rlockId[2]) << 16) | (((ulong)rlockId[3]) << 24)
                            | (((ulong)rlockId[4]) << 32) | (((ulong)rlockId[5]) << 40) | (((ulong)rlockId[6]) << 48) | (((ulong)rlockId[7]) << 56);
                }
            }
            catch (LockTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
            catch (ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
        }

        public async Task WaitAsync(uint timeout)
        {
            byte[] lockId = EncodeLockId(clientId, versionId);
            Lock waitLock = new Lock(database, groupKey, lockId, timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16),
                    0, 0, (byte)0);
            try
            {
                LockCommandResult lockCommandResult = await waitLock.AcquireAsync((byte)0);
                byte[] rlockId = lockCommandResult.LockId;
                if (!EqualBytes(lockId, rlockId))
                {
                    versionId = ((ulong)rlockId[0]) | (((ulong)rlockId[1]) << 8) | (((ulong)rlockId[2]) << 16) | (((ulong)rlockId[3]) << 24)
                            | (((ulong)rlockId[4]) << 32) | (((ulong)rlockId[5]) << 40) | (((ulong)rlockId[6]) << 48) | (((ulong)rlockId[7]) << 56);
                }
            }
            catch (LockTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
            catch (ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
        }

        public void WaitAndTimeoutRetryClear(uint timeout)
        {
            byte[] lockId = EncodeLockId(clientId, versionId);
            Lock waitLock = new Lock(database, groupKey, lockId, timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16),
                    0, (ushort)0, (byte)0);
            try
            {
                LockCommandResult lockCommandResult = waitLock.Acquire((byte)0);
                byte[] rlockId = lockCommandResult.LockId;
                if (!EqualBytes(lockId, rlockId))
                {
                    versionId = ((ulong)rlockId[0]) | (((ulong)rlockId[1]) << 8) | (((ulong)rlockId[2]) << 16) | (((ulong)rlockId[3]) << 24)
                            | (((ulong)rlockId[4]) << 32) | (((ulong)rlockId[5]) << 40) | (((ulong)rlockId[6]) << 48) | (((ulong)rlockId[7]) << 56);
                }
            }
            catch (SlockException ex) when (ex is LockTimeoutException || ex is ClientCommandTimeoutException)
            {
                Lock eventLock = new Lock(database, EncodeLockId(0, versionId), groupKey,
                    this.timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16), expried, (ushort)0, (byte)0);
                try
                {
                    eventLock.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
                    try
                    {
                        eventLock.Release();
                    }
                    catch (SlockException)
                    {
                    }
                    return;
                }
                catch (SlockException)
                {
                }
                throw new EventWaitTimeoutException();
            }
        }

        public async Task WaitAndTimeoutRetryClearAsync(uint timeout)
        {
            byte[] lockId = EncodeLockId(clientId, versionId);
            Lock waitLock = new Lock(database, groupKey, lockId, timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16),
                    0, (ushort)0, (byte)0);
            try
            {
                LockCommandResult lockCommandResult = await waitLock.AcquireAsync((byte)0);
                byte[] rlockId = lockCommandResult.LockId;
                if (!EqualBytes(lockId, rlockId))
                {
                    versionId = ((ulong)rlockId[0]) | (((ulong)rlockId[1]) << 8) | (((ulong)rlockId[2]) << 16) | (((ulong)rlockId[3]) << 24)
                            | (((ulong)rlockId[4]) << 32) | (((ulong)rlockId[5]) << 40) | (((ulong)rlockId[6]) << 48) | (((ulong)rlockId[7]) << 56);
                }
            }
            catch (SlockException ex) when (ex is LockTimeoutException || ex is ClientCommandTimeoutException)
            {
                Lock eventLock = new Lock(database, EncodeLockId(0, versionId), groupKey,
                    this.timeout | (ICommand.TIMEOUT_FLAG_LESS_LOCK_VERSION_IS_LOCK_SUCCED << 16), expried, (ushort)0, (byte)0);
                try
                {
                    await eventLock.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
                    try
                    {
                        await eventLock.ReleaseAsync();
                    }
                    catch (SlockException)
                    {
                    }
                    return;
                }
                catch (SlockException)
                {
                }
                throw new EventWaitTimeoutException();
            }
        }

        private byte[] EncodeLockId(ulong clientId, ulong versionId)
        {
            return new byte[16] { (byte)(versionId & 0xff), (byte)((versionId >> 8) & 0xff), (byte)((versionId >> 16) & 0xff), (byte)((versionId >> 24) & 0xff),
            (byte)((versionId >> 32) & 0xff), (byte)((versionId >> 40) & 0xff), (byte)((versionId >> 48) & 0xff), (byte)((versionId >> 56) & 0xff),
            (byte)(clientId & 0xff), (byte)((clientId >> 8) & 0xff), (byte)((clientId >> 16) & 0xff), (byte)((clientId >> 24) & 0xff),
            (byte)((clientId >> 32) & 0xff), (byte)((clientId >> 40) & 0xff), (byte)((clientId >> 48) & 0xff), (byte)((clientId >> 56) & 0xff) };
        }

        public static bool EqualBytes(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
    }
}
