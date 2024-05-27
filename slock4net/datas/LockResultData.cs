using slock4net.Commands;
using System;
using System.Collections.Generic;
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

        public IList<byte[]> DataAsList
        {
            get
            {
                if (data == null || data[4] == ICommand.LOCK_DATA_COMMAND_TYPE_UNSET || (data[5] & ICommand.LOCK_DATA_FLAG_VALUE_TYPE_ARRAY) == 0)
                {
                    return null;
                }
                List<byte[]> values = new List<byte[]>();
                int index = ValueOffset;
                while (index + 4 < data.Length)
                {
                    int valueLen = ((int)data[index] & 0xff) | ((int)data[index + 1] & 0xff) << 8 | ((int)data[index + 2] & 0xff) << 16 | ((int)data[index + 3] & 0xff) << 24;
                    if (valueLen == 0)
                    {
                        index += 4;
                        continue;
                    }
                    byte[] value = new byte[valueLen];
                    Array.Copy(data, index + 4, value, 0, valueLen);
                    values.Add(value);
                    index += valueLen + 4;
                }
                return values;
            }
        }

        public IList<string> DataAsStringList
        {
            get
            {
                if (data == null || data[4] == ICommand.LOCK_DATA_COMMAND_TYPE_UNSET || (data[5] & ICommand.LOCK_DATA_FLAG_VALUE_TYPE_ARRAY) == 0)
                {
                    return null;
                }
                IList<string> values = new List<string>();
                int index = ValueOffset;
                while (index + 4 < data.Length)
                {
                    int valueLen = ((int)data[index] & 0xff) | ((int)data[index + 1] & 0xff) << 8 | ((int)data[index + 2] & 0xff) << 16 | ((int)data[index + 3] & 0xff) << 24;
                    if (valueLen == 0)
                    {
                        index += 4;
                        continue;
                    }
                    values.Add(Encoding.UTF8.GetString(data, index + 4, valueLen));
                    index += valueLen + 4;
                }
                return values;
            }
        }

        public IDictionary<string, byte[]> DataAsDictionary
        {
            get
            {
                if (data == null || data[4] == ICommand.LOCK_DATA_COMMAND_TYPE_UNSET || (data[5] & ICommand.LOCK_DATA_FLAG_VALUE_TYPE_KV) == 0)
                {
                    return null;
                }
                IDictionary<string, byte[]> values = new Dictionary<string, byte[]>();
                int index = ValueOffset;
                while (index + 4 < data.Length)
                {
                    int keyLen = ((int)data[index] & 0xff) | ((int)data[index + 1] & 0xff) << 8 | ((int)data[index + 2] & 0xff) << 16 | ((int)data[index + 3] & 0xff) << 24;
                    if (keyLen == 0)
                    {
                        index += 4;
                        continue;
                    }
                    string key = Encoding.UTF8.GetString(data, index + 4, keyLen);
                    index += keyLen + 4;
                    int valueLen = ((int)data[index] & 0xff) | ((int)data[index + 1] & 0xff) << 8 | ((int)data[index + 2] & 0xff) << 16 | ((int)data[index + 3] & 0xff) << 24;
                    if (valueLen == 0)
                    {
                        index += 4;
                        continue;
                    }
                    byte[] value = new byte[valueLen];
                    Array.Copy(data, index + 4, value, 0, valueLen);
                    values.TryAdd(key, value);
                    index += valueLen + 4;
                }
                return values;
            }
        }

        public IDictionary<string, string> DataAsStringDictionary
        {
            get
            {
                if (data == null || data[4] == ICommand.LOCK_DATA_COMMAND_TYPE_UNSET || (data[5] & ICommand.LOCK_DATA_FLAG_VALUE_TYPE_KV) == 0)
                {
                    return null;
                }
                IDictionary<string, string> values = new Dictionary<string, string>();
                int index = ValueOffset;
                while (index + 4 < data.Length)
                {
                    int keyLen = ((int)data[index] & 0xff) | ((int)data[index + 1] & 0xff) << 8 | ((int)data[index + 2] & 0xff) << 16 | ((int)data[index + 3] & 0xff) << 24;
                    if (keyLen == 0)
                    {
                        index += 4;
                        continue;
                    }
                    string key = Encoding.UTF8.GetString(data, index + 4, keyLen);
                    index += keyLen + 4;
                    int valueLen = ((int)data[index] & 0xff) | ((int)data[index + 1] & 0xff) << 8 | ((int)data[index + 2] & 0xff) << 16 | ((int)data[index + 3] & 0xff) << 24;
                    if (valueLen == 0)
                    {
                        index += 4;
                        continue;
                    }
                    values.TryAdd(key, Encoding.UTF8.GetString(data, index + 4, valueLen));
                    index += valueLen + 4;
                }
                return values;
            }
        }
    }
}
