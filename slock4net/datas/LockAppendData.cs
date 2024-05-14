
using slock4net.Commands;
using System.Text;

namespace slock4net.datas
{
    public class LockAppendData : LockData
    {
        public LockAppendData(byte[] value) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_APPEND, 0, value)
        {
        }

        public LockAppendData(string value) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_APPEND, 0, Encoding.UTF8.GetBytes(value))
        {
        }
    }
}
