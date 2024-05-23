using slock4net.Commands;

namespace slock4net.datas
{
    public class LockUnsetData : LockData
    {
        public LockUnsetData() : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_UNSET, 0, new byte[0])
        {
        }

        public LockUnsetData(byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_UNSET, commandFlag, new byte[0])
        {
        }
    }
}
