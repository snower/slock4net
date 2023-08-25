using slock4net.Commands;

namespace slock4net.Exceptions
{
    public class LockUnlockedException : LockException
    {
        public LockUnlockedException(Command command, CommandResult commandResult) : base(command, commandResult)
        {
        }
    }
}
