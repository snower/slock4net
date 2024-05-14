using slock4net.Commands;
using slock4net.datas;
using System;

namespace slock4net.Exceptions
{
    public class LockException : SlockException
    {
        static string[] ERROR_MSG = new string[]{
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

        private readonly Command command;
        private readonly CommandResult commandResult;

        public LockException(Command command, CommandResult commandResult) : base(GetErrMessage(commandResult))
        {
            this.command = command;
            this.commandResult = commandResult;
        }

        protected static string GetErrMessage(CommandResult commandResult)
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

        public Command Command
        {
            get { return command; }
        }

        public CommandResult CommandResult
        {
            get { return commandResult; }
        }

        public LockResultData LockData
        {
            get {
                if (commandResult is LockCommandResult lockCommandResult) {
                    return lockCommandResult.LockResultData;
                }
                return null;
            }
        }

        public byte[] LockDataAsBytes
        {
            get {
                if (commandResult is LockCommandResult lockCommandResult)
                {
                    return lockCommandResult.LockResultData.DataAsBytes;
                }
                return null;
            }
        }

        public string LockDataAsString
        {
            get
            {
                if (commandResult is LockCommandResult lockCommandResult)
                {
                    return lockCommandResult.LockResultData.DataAsString;
                }
                return null;
            }
        }

        public long LockDataAsLong
        {
            get
            {
                if (commandResult is LockCommandResult lockCommandResult)
                {
                    return lockCommandResult.LockResultData.DataAsLong;
                }
                return 0L;
            }
        }
    }
}