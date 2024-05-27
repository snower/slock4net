using slock4net.Commands;

namespace slock4net.datas
{
    public class LockPopData : LockData
    {
        public LockPopData(int count) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_POP, ICommand.LOCK_DATA_FLAG_VALUE_TYPE_NUMBER, new byte[]{
                (byte)(count & 0xff), (byte)((count >> 8) & 0xff), (byte)((count >> 16) & 0xff), (byte)((count >> 24) & 0xff)})
        {
        }

        public LockPopData(int count, byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_CURRENT, ICommand.LOCK_DATA_COMMAND_TYPE_POP, (byte)(commandFlag | ICommand.LOCK_DATA_FLAG_VALUE_TYPE_NUMBER), new byte[]{
                (byte)(count & 0xff), (byte)((count >> 8) & 0xff), (byte)((count >> 16) & 0xff), (byte)((count >> 24) & 0xff)})
        {
        }
    }
}
