using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Exceptions
{
    public class LockLockedException : LockException
    {
        public LockLockedException(Command command, CommandResult commandResult) : base(command, commandResult)
        {
        }
    }
}