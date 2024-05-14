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

        public byte[] RawData
        {
            get { return data; }
        }

        public byte[] DataAsBytes
        {
            get
            {
                if (data.Length <= 6)
                {
                    return null;
                }
                byte[] result = new byte[data.Length - 6];
                Array.Copy(data, 6, result, 0, data.Length);
                return result;
            }
        }

        public string DataAsString
        {
            get
            {
                if (data.Length <= 6)
                {
                    return "";
                }
                byte[] result = new byte[data.Length - 6];
                Array.Copy(data, 6, result, 0, data.Length);
                return Encoding.UTF8.GetString(result);
            }
        }

        public long DataAsLong
        {
            get
            {
                long value = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (i + 6 >= data.Length) break;
                    value |= ((((long)data[i + 6]) & 0xff) << (i * 8));
                }
                return value;
            }
        }
    }
}
