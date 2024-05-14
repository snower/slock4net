using slock4net.Commands;
using slock4net.datas;
using slock4net.Exceptions;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class Lock : AbstractExecution {
        private readonly byte[] lockId;

        public byte[] LockKey { get { return lockKey; } }
        public byte[] LockId { get { return lockId; } }

        public Lock(SlockDatabase database, byte[] lockKey, byte[] lockId, uint timeout, uint expried, ushort count, byte rCount) : base(database, lockKey, timeout, expried, count, rCount) {
            if (lockId == null) {
                this.lockId = LockCommand.GenLockId();
            } else {
                if (lockId.Length > 16) {
                    using (MD5 md5 = MD5.Create())
                    {
                        this.lockId = md5.ComputeHash(lockId);
                    }
                } else {
                    this.lockId = new byte[16];
                    Array.Copy(lockId, 0, this.lockId, 16 - lockId.Length, lockId.Length);
                }
            }
        }

        public Lock(SlockDatabase database, byte[] lockKey, uint timeout, uint expried) : this(database, lockKey, null, timeout, expried, 0, 0) {
        }

        public Lock(SlockDatabase database, string lockKey, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(lockKey), null, timeout, expried, 0, 0)
        {
        }

        public LockCommandResult Acquire(byte flag) { 
            return Acquire(flag, null);
        }

        public LockCommandResult Acquire(byte flag, LockData lockData) {
            LockCommand command = new LockCommand(ICommand.COMMAND_TYPE_LOCK, lockData != null ? ((byte) (flag | ICommand.LOCK_FLAG_CONTAINS_DATA)) : flag,
                database.DatabaseId, lockKey, lockId, timeout, expried, count, rCount, lockData);
            LockCommandResult commandResult = (LockCommandResult)database.Client.SendCommand(command);
            currentLockData = commandResult.LockResultData;
            if (commandResult.Result == ICommand.COMMAND_RESULT_SUCCED) {
                return commandResult;
            }

            switch (commandResult.Result) {
                case ICommand.COMMAND_RESULT_LOCKED_ERROR:
                    throw new LockLockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNLOCK_ERROR:
                    throw new LockUnlockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNOWN_ERROR:
                    throw new LockNotOwnException(command, commandResult);
                case ICommand.COMMAND_RESULT_TIMEOUT:
                    throw new LockTimeoutException(command, commandResult);
                default:
                    throw new LockException(command, commandResult);
            }
        }

        public async Task<LockCommandResult> AcquireAsync(byte flag)
        { 
            return await AcquireAsync(flag, null);
        }

        public async Task<LockCommandResult> AcquireAsync(byte flag, LockData lockData)
        {
            LockCommand command = new LockCommand(ICommand.COMMAND_TYPE_LOCK, lockData != null ? ((byte)(flag | ICommand.LOCK_FLAG_CONTAINS_DATA)) : flag,
                database.DatabaseId, lockKey, lockId, timeout, expried, count, rCount, lockData);
            LockCommandResult commandResult = (LockCommandResult) await database.Client.SendCommandAsync(command);
            currentLockData = commandResult.LockResultData;
            if (commandResult.Result == ICommand.COMMAND_RESULT_SUCCED)
            {
                return commandResult;
            }

            switch (commandResult.Result)
            {
                case ICommand.COMMAND_RESULT_LOCKED_ERROR:
                    throw new LockLockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNLOCK_ERROR:
                    throw new LockUnlockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNOWN_ERROR:
                    throw new LockNotOwnException(command, commandResult);
                case ICommand.COMMAND_RESULT_TIMEOUT:
                    throw new LockTimeoutException(command, commandResult);
                default:
                    throw new LockException(command, commandResult);
            }
        }

        public void Acquire(LockData lockData)
        {
            this.Acquire(0, lockData);
        }

        public void Acquire() {
            this.Acquire(0, null);
        }
        public async Task AcquireAsync(LockData lockData)
        {
            await this.AcquireAsync(0, lockData);
        }

        public async Task AcquireAsync()
        {
            await this.AcquireAsync(0, null);
        }

        public LockCommandResult Release(byte flag)
        {
            return Release(flag, null);
        }

        public LockCommandResult Release(byte flag, LockData lockData) {
            LockCommand command = new LockCommand(ICommand.COMMAND_TYPE_UNLOCK, lockData != null ? ((byte)(flag | ICommand.UNLOCK_FLAG_CONTAINS_DATA)) : flag,
                database.DatabaseId, lockKey, lockId, timeout, expried, count, rCount, lockData);
            LockCommandResult commandResult = (LockCommandResult)database.Client.SendCommand(command);
            currentLockData = commandResult.LockResultData;
            if (commandResult.Result == ICommand.COMMAND_RESULT_SUCCED) {
                return commandResult;
            }

            switch (commandResult.Result) {
                case ICommand.COMMAND_RESULT_LOCKED_ERROR:
                    throw new LockLockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNLOCK_ERROR:
                    throw new LockUnlockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNOWN_ERROR:
                    throw new LockNotOwnException(command, commandResult);
                case ICommand.COMMAND_RESULT_TIMEOUT:
                    throw new LockTimeoutException(command, commandResult);
                default:
                    throw new LockException(command, commandResult);
            }
        }

        public async Task<LockCommandResult> ReleaseAsync(byte flag)
        {
            return await ReleaseAsync(flag, null);
        }

        public async Task<LockCommandResult> ReleaseAsync(byte flag, LockData lockData)
        {
            LockCommand command = new LockCommand(ICommand.COMMAND_TYPE_UNLOCK, lockData != null ? ((byte)(flag | ICommand.UNLOCK_FLAG_CONTAINS_DATA)) : flag,
                database.DatabaseId, lockKey, lockId, timeout, expried, count, rCount);
            LockCommandResult commandResult = (LockCommandResult) await database.Client.SendCommandAsync(command);
            currentLockData = commandResult.LockResultData;
            if (commandResult.Result == ICommand.COMMAND_RESULT_SUCCED)
            {
                return commandResult;
            }

            switch (commandResult.Result)
            {
                case ICommand.COMMAND_RESULT_LOCKED_ERROR:
                    throw new LockLockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNLOCK_ERROR:
                    throw new LockUnlockedException(command, commandResult);
                case ICommand.COMMAND_RESULT_UNOWN_ERROR:
                    throw new LockNotOwnException(command, commandResult);
                case ICommand.COMMAND_RESULT_TIMEOUT:
                    throw new LockTimeoutException(command, commandResult);
                default:
                    throw new LockException(command, commandResult);
            }
        }

        public void Release(LockData lockData)
        {
            this.Release(0, lockData);
        }

        public void Release() {
            this.Release(0, null);
        }

        public async Task ReleaseAsync(LockData lockData)
        {
            await this.ReleaseAsync(0, lockData);
        }

        public async Task ReleaseAsync()
        {
            await this.ReleaseAsync(0, null);
        }

        public CommandResult Show()
        {
            return Show(null);
        }

        public CommandResult Show(LockData lockData) {
            try {
                this.Acquire(ICommand.LOCK_FLAG_SHOW_WHEN_LOCKED, lockData);
            } catch (LockNotOwnException e) {
                return e.CommandResult;
            }
            return null;
        }

        public async Task<CommandResult> ShowAsync()
        {
            return await ShowAsync(null);
        }

        public async Task<CommandResult> ShowAsync(LockData lockData)
        {
            try
            {
                await this.AcquireAsync(ICommand.LOCK_FLAG_SHOW_WHEN_LOCKED, lockData);
            }
            catch (LockNotOwnException e)
            {
                return e.CommandResult;
            }
            return null;
        }

        public void Update()
        {
            Update(null);
        }

        public void Update(LockData lockData) {
            try {
                this.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED, lockData);
            } catch (LockLockedException) {
            }
        }

        public async Task UpdateAsync()
        {
            await UpdateAsync(null);
        }

        public async Task UpdateAsync(LockData lockData)
        {
            try
            {
                await this.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED, lockData);
            }
            catch (LockLockedException)
            {
            }
        }

        public void ReleaseHead()
        {
            ReleaseHead(null);
        }

        public void ReleaseHead(LockData lockData) {
            this.Release(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED, lockData);
        }

        public async Task ReleaseHeadAsync()
        {
            await ReleaseHeadAsync(null);
        }

        public async Task ReleaseHeadAsync(LockData lockData)
        {
            await this.ReleaseAsync(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED, lockData);
        }

        public LockCommandResult ReleaseHeadRetoLockWait()
        {
            return ReleaseHeadRetoLockWait(null);
        }

        public LockCommandResult ReleaseHeadRetoLockWait(LockData lockData)
        {
            return this.Release((byte) (ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED | ICommand.UNLOCK_FLAG_SUCCED_TO_LOCK_WAIT), lockData);
        }

        public async Task<LockCommandResult> ReleaseHeadRetoLockWaitAsync()
        {
            return await ReleaseHeadRetoLockWaitAsync(null);
        }

        public async Task<LockCommandResult> ReleaseHeadRetoLockWaitAsync(LockData lockData)
        {
            return await this.ReleaseAsync((byte)(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED | ICommand.UNLOCK_FLAG_SUCCED_TO_LOCK_WAIT), lockData);
        }
    }
}