using slock4net.Commands;

namespace slock4net.Exceptions
{
    public class LockNotOwnException : LockException
    {
        public LockNotOwnException(Command command, CommandResult commandResult) : base(command, commandResult)
        {
        }
    }
}
