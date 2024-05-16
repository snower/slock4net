using slock4net.Commands;
using System;
using System.Text;

namespace slock4net.datas
{
    public class LockResultData
    {
        protected byte[] data;

        public LockResultData(byte[] data)
        {
            this.data = data;
        }

        private int ValueOffset
        {
            get
            {
                if ((data[5] & ICommand.LOCK_DATA_FLAG_CONTAINS_PROPERTY) != 0)
                {
                    return ((((int)data[6]) & 0xff) | (((int)data[7]) & 0xff) << 8) + 8;
                }
                return 6;
            }
        }

        public byte[] RawData
        {
            get { return data; }
        }

        public byte[] DataAsBytes
        {
            get
            {
                int offset = ValueOffset;
                if (data.Length <= offset)
                {
                    return null;
                }
                byte[] value = new byte[data.Length - offset];
                Array.Copy(data, offset, value, 0, value.Length);
                return value;
            }
        }

        public string DataAsString
        {
            get
            {
                int offset = ValueOffset;
                if (data.Length <= offset)
                {
                    return "";
                }
                byte[] value = new byte[data.Length - offset];
                Array.Copy(data, offset, value, 0, value.Length);
                return Encoding.UTF8.GetString(value);
            }
        }

        public long DataAsLong
        {
            get
            {
                int offset = ValueOffset;
                long value = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (i + offset >= data.Length) break;
                    value |= ((((long)data[i + offset]) & 0xff) << (i * 8));
                }
                return value;
            }
        }
    }
}
