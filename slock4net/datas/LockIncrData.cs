using slock4net.Commands;

namespace slock4net.datas
{
    public class LockIncrData : LockData
    {
        public LockIncrData(long incrValue) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_INCR, ICommand.LOCK_DATA_FLAG_VALUE_TYPE_NUMBER, new byte[]{
                (byte)(incrValue & 0xff), (byte)((incrValue >> 8) & 0xff), (byte)((incrValue >> 16) & 0xff), (byte)((incrValue >> 24) & 0xff),
                (byte)((incrValue >> 32) & 0xff), (byte)((incrValue >> 40) & 0xff), (byte)((incrValue >> 48) & 0xff), (byte)((incrValue >> 56) & 0xff)})
        {
        }
    }
}
