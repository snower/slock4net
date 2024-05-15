using slock4net.Commands;

namespace slock4net.datas
{
    public class LockShiftData : LockData
    {
        public LockShiftData(int length) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_SHIFT, ICommand.LOCK_DATA_FLAG_VALUE_TYPE_NUMBER, new byte[]{
                (byte)(length & 0xff), (byte)((length >> 8) & 0xff), (byte)((length >> 16) & 0xff), (byte)((length >> 24) & 0xff)})
        {
        }
    }
}
