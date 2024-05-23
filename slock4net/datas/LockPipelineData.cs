using slock4net.Commands;
using slock4net.Exceptions;
using System;

namespace slock4net.datas
{
    public class LockPipelineData : LockData
    {
        private readonly LockData[] lockDatas;

        public LockPipelineData(LockData[] lockDatas) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_PIPELINE, (byte)0, null)
        {
            this.lockDatas = lockDatas;
        }

        public LockPipelineData(LockData[] lockDatas, byte commandFlag) : base(ICommand.LOCK_DATA_STAGE_LOCK, ICommand.LOCK_DATA_COMMAND_TYPE_PIPELINE, commandFlag, null)
        {
            this.lockDatas = lockDatas;
        }

        public override byte[] DumpData()
        {
            if (lockDatas == null || lockDatas.Length == 0) {
                throw new LockDataException("Data value is null");
            }
            byte[][] values = new byte[lockDatas.Length][];
            int valueLength = 0;
            for (int i = 0; i<lockDatas.Length; i++) {
                values[i] = lockDatas[i].DumpData();
                valueLength += values[i].Length;
            }
            byte[] data = new byte[valueLength + 6];
            data[0] = (byte)((valueLength + 2) & 0xff);
            data[1] = (byte)(((valueLength + 2) >> 8) & 0xff);
            data[2] = (byte)(((valueLength + 2) >> 16) & 0xff);
            data[3] = (byte)(((valueLength + 2) >> 24) & 0xff);
            data[4] = (byte)(((commandStage << 6) & 0xc0) | (commandType & 0x3f));
            data[5] = commandFlag;
            for (int i = 0, j = 6; i < values.Length; i++)
            {
                Array.Copy(values[i], 0, data, j, values[i].Length);
                j += values[i].Length;
            }
            return data;
        }
    }
}
