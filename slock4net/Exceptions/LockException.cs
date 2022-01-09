using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Exceptions
{
    class LockException : SlockException
    {
        static String[] ERROR_MSG = new String[]{
            "OK",
            "UNKNOWN_MAGIC",
            "UNKNOWN_VERSION",
            "UNKNOWN_DB",
            "UNKNOWN_COMMAND",
            "LOCKED_ERROR",
            "UNLOCK_ERROR",
            "UNOWN_ERROR",
            "TIMEOUT",
            "EXPRIED",
            "RESULT_STATE_ERROR",
            "UNKNOWN_ERROR"
    };

        private Command command;
        private CommandResult commandResult;

        public LockException(Command command, CommandResult commandResult) : base(GetErrMessage(commandResult))
        {
            this.command = command;
            this.commandResult = commandResult;
        }

        protected static String GetErrMessage(CommandResult commandResult)
        {
            if (commandResult == null)
            {
                return "UNKNOWN_ERROR";
            }
            if (commandResult.Result > 0 && commandResult.Result < ERROR_MSG.Length)
            {
                return "Code " + commandResult.Result.ToString() + " " + ERROR_MSG[commandResult.Result];
            }
            return "Code " + commandResult.Result.ToString();
        }

        public Command GetCommand()
        {
            return command;
        }

        public CommandResult GetCommandResult()
        {
            return commandResult;
        }
    }
}