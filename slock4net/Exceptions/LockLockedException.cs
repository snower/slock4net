using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Exceptions
{
    class LockLockedException : LockException
    {
        public LockLockedException(Command command, CommandResult commandResult) : base(command, commandResult)
        {
        }
    }
}