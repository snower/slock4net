using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace slock4net.datas
{
    internal class LockExecuteData : LockData
    {
        public LockExecuteData(byte commandStage, LockCommand lockCommand) : base(commandStage, ICommand.LOCK_DATA_COMMAND_TYPE_EXECUTE, 0, lockCommand.DumpCommand())
        {
        }

        public LockExecuteData(LockCommand lockCommand) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_EXECUTE, 0, lockCommand.DumpCommand())
        {
        }
    }
}
