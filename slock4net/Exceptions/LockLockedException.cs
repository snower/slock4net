using slock4net.Commands;

namespace slock4net.Exceptions
{
    public class LockLockedException : LockException
    {
        public LockLockedException(Command command, CommandResult commandResult) : base(command, commandResult)
        {
        }
    }
}