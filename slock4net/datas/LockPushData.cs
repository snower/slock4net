using slock4net.Commands;
using System.Text;

namespace slock4net.datas
{
    public class LockPushData : LockData
    {
        public LockPushData(byte[] value) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_PUSH, 0, value)
        {
        }

        public LockPushData(string value) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_PUSH, 0, Encoding.UTF8.GetBytes(value))
        {
        }

        public LockPushData(byte[] value, byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_PUSH, commandFlag, value)
        {
        }

        public LockPushData(string value, byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_PUSH, commandFlag, Encoding.UTF8.GetBytes(value))
        {
        }
    }
}
