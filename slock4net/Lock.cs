using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace slock4net
{
    public class Lock {
        private SlockDatabase database;
        private byte[] lockKey;
        private byte[] lockId;
        private uint timeout;
        private uint expried;
        private ushort count;
        private byte rCount;

        public byte[] LockKey { get { return lockKey; } }
        public byte[] LockId { get { return lockId; } }

        public Lock(SlockDatabase database, byte[] lockKey, byte[] lockId, uint timeout, uint expried, ushort count, byte rCount) {
            this.database = database;
            if (lockKey.Length > 16) {
                using (MD5 md5 = MD5.Create())
                {
                    this.lockKey = md5.ComputeHash(lockKey);
                }
            } else {
                this.lockKey = new byte[16];
                Array.Copy(lockKey, 0, this.lockKey, 16 - lockKey.Length, lockKey.Length);
            }
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
            this.timeout = timeout;
            this.expried = expried;
            this.count = count;
            this.rCount = rCount;
        }

        public Lock(SlockDatabase database, byte[] lockKey, uint timeout, uint expried) : this(database, lockKey, null, timeout, expried, 0, 0) {
        }

        public Lock(SlockDatabase database, string lockKey, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(lockKey), null, timeout, expried, 0, 0)
        {
        }

        public LockCommandResult Acquire(byte flag) {
            LockCommand command = new LockCommand(ICommand.COMMAND_TYPE_LOCK, flag, database.DatabaseId, lockKey,
                    lockId, timeout, expried, count, rCount);
            LockCommandResult commandResult = (LockCommandResult)database.Client.SendCommand(command);
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

        public void Acquire() {
            this.Acquire(0);
        }

        public LockCommandResult Release(byte flag) {
            LockCommand command = new LockCommand(ICommand.COMMAND_TYPE_UNLOCK, flag, database.DatabaseId, lockKey,
                    lockId, timeout, expried, count, rCount);
            LockCommandResult commandResult = (LockCommandResult)database.Client.SendCommand(command);
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

        public void Release() {
            this.Release(0);
        }

        public CommandResult Show() {
            try {
                this.Acquire(ICommand.LOCK_FLAG_SHOW_WHEN_LOCKED);
            } catch (LockNotOwnException e) {
                return e.GetCommandResult();
            }
            return null;
        }

        public void Update() {
            try {
                this.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
            } catch (LockLockedException) {
            }
        }

        public void ReleaseHead() {
            this.Release(ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED);
        }

        public LockCommandResult ReleaseHeadRetoLockWait()
        {
            return this.Acquire((byte) (ICommand.UNLOCK_FLAG_UNLOCK_FIRST_LOCK_WHEN_UNLOCKED | ICommand.UNLOCK_FLAG_SUCCED_TO_LOCK_WAIT));
        }
    }
}