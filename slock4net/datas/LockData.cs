using slock4net.Exceptions;
using System;

namespace slock4net.datas
{
    public class LockData
    {
        protected byte commandStage;
        protected byte commandType;
        protected byte commandFlag;
        protected byte[] value;

        public LockData(byte commandStage, byte commandType, byte commandFlag, byte[] value)
        {
            this.commandStage = commandStage;
            this.commandType = commandType;
            this.commandFlag = commandFlag;
            this.value = value;
        }

        public virtual byte[] DumpData()
        {
            if (value == null) {
                throw new LockDataException();
            }
            byte[] data = new byte[value.Length + 6];
            data[0] = (byte) ((value.Length + 2) & 0xff);
            data[1] = (byte) (((value.Length + 2) >> 8 ) & 0xff);
            data[2] = (byte) (((value.Length + 2) >> 16 ) & 0xff);
            data[3] = (byte) (((value.Length + 2) >> 24 ) & 0xff);
            data[4] = (byte) (((commandStage << 6) & 0xc0) | (commandType & 0x3f));
            data[5] = commandFlag;
        
            Array.Copy(value, 0, data, 6, value.Length);
            return data;
        }
    }
}
