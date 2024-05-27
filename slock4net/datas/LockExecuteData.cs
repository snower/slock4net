using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace slock4net.datas
{
    public class LockExecuteData : LockData
    {
        public LockExecuteData(byte commandStage, LockCommand lockCommand) : base(commandStage, ICommand.LOCK_DATA_COMMAND_TYPE_EXECUTE, 0, lockCommand.DumpCommand())
        {
        }

        public LockExecuteData(LockCommand lockCommand) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_EXECUTE, 0, lockCommand.DumpCommand())
        {
        }

        public LockExecuteData(byte commandStage, LockCommand lockCommand, byte commandFlag) : base(commandStage, ICommand.LOCK_DATA_COMMAND_TYPE_EXECUTE, commandFlag, lockCommand.DumpCommand())
        {
        }

        public LockExecuteData(LockCommand lockCommand, byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_EXECUTE, commandFlag, lockCommand.DumpCommand())
        {
        }
    }
}
