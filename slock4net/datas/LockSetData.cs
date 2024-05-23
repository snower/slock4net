using slock4net.Commands;
using System.Text;

namespace slock4net.datas
{
    public class LockSetData : LockData
    {
        public LockSetData(byte[] value) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_SET, 0, value)
        {
        }

        public LockSetData(string value) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_SET, 0, Encoding.UTF8.GetBytes(value))
        {
        }

        public LockSetData(byte[] value, byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_SET, commandFlag, value)
        {
        }

        public LockSetData(string value, byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_SET, commandFlag, Encoding.UTF8.GetBytes(value))
        {
        }
    }
}
