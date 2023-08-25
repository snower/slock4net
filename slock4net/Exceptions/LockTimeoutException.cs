using slock4net.Commands;

namespace slock4net.Exceptions
{
    public class LockTimeoutException : LockException
    {
        public LockTimeoutException(Command command, CommandResult commandResult) : base(command, commandResult)
        {
        }
    }
}