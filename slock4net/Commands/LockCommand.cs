using slock4net.datas;
using slock4net.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace slock4net.Commands
{
    public class LockCommand : Command
    {
        private static long lockIdIndex = 0;
        public byte Flag { get; protected set; }
        public byte DatabaseId { get; protected set; }
        public byte[] LockId { get; protected set; }
        public byte[] LockKey { get; protected set; }
        public uint Timeout { get; protected set; }
        public uint Expried { get; protected set; }
        public ushort Count { get; protected set; }
        public byte RCount { get; protected set; }
        public LockData LockData { get; protected set; }

        public LockCommand(byte commandType, byte flag, byte datbaseId, byte[] lockKey, byte[] lockId,
            uint timeout, uint expried, ushort count, byte rCount, LockData lockData) : base(commandType)
        {
            this.Flag = flag;
            this.DatabaseId = datbaseId;
            this.LockKey = lockKey;
            this.LockId = lockId;
            this.Timeout = timeout;
            this.Expried = expried;
            this.Count = count;
            this.RCount = rCount;
            this.LockData = lockData;
        }

        public LockCommand(byte commandType, byte flag, byte datbaseId, byte[] lockKey, byte[] lockId,
           uint timeout, uint expried, ushort count, byte rCount) : this(commandType, flag, datbaseId, lockKey, lockId, timeout, expried, count, rCount, null)
        {
        }

        public override byte[] DumpCommand()
        {
            using (MemoryStream ms = new MemoryStream(64))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(ICommand.MAGIC);
                    bw.Write(ICommand.VERSION);
                    bw.Write(this.CommandType);
                    bw.Write(this.RequestId, 0, 16);
                    bw.Write(this.Flag);
                    bw.Write(this.DatabaseId);
                    bw.Write(this.LockId, 0, 16);
                    bw.Write(this.LockKey, 0, 16);
                    bw.Write((byte)(this.Timeout & 0xff));
                    bw.Write((byte)((this.Timeout >> 8) & 0xff));
                    bw.Write((byte)((this.Timeout >> 16) & 0xff));
                    bw.Write((byte)((this.Timeout >> 24) & 0xff));
                    bw.Write((byte)(this.Expried & 0xff));
                    bw.Write((byte)((this.Expried >> 8) & 0xff));
                    bw.Write((byte)((this.Expried >> 16) & 0xff));
                    bw.Write((byte)((this.Expried >> 24) & 0xff));
                    bw.Write((byte)(this.Count & 0xff));
                    bw.Write((byte)((this.Count >> 8) & 0xff));
                    bw.Write(this.RCount);
                }
                return ms.GetBuffer();
            }
        }

        public override ICommand LoadCommand(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    this.Magic = br.ReadByte();
                    this.Version = br.ReadByte();
                    this.CommandType = br.ReadByte();
                    this.RequestId = br.ReadBytes(16);
                    this.Flag = br.ReadByte();
                    this.DatabaseId = br.ReadByte();
                    this.LockId = br.ReadBytes(16);
                    this.LockKey = br.ReadBytes(16);
                    this.Timeout = ((uint)br.ReadByte()) | (((uint)br.ReadByte()) << 8) | (((uint)br.ReadByte()) << 16) 
                        | (((uint)br.ReadByte()) << 24);
                    this.Expried = ((uint)br.ReadByte()) | (((uint)br.ReadByte()) << 8) | (((uint)br.ReadByte()) << 16)
                        | (((uint)br.ReadByte()) << 24);
                    this.Count = (ushort)(((ushort)br.ReadByte()) | (((ushort)br.ReadByte()) << 8));
                    this.RCount = (byte)br.ReadByte();
                }
            }
            return this;
        }

        public override bool HasExtraData()
        {
            return (Flag & ICommand.LOCK_FLAG_CONTAINS_DATA) != 0;
        }

        public override byte[] GetExtraData()
        {
            if ((Flag & ICommand.LOCK_FLAG_CONTAINS_DATA) == 0) return null;
            if (LockData == null) {
                throw new LockDataException("Data is null");
            }
            return LockData.DumpData();
        }

        public static byte[] GenLockId()
        {
            using (MemoryStream ms = new MemoryStream(16))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    long timestamp = (new DateTimeOffset(DateTime.Now)).ToUnixTimeMilliseconds();
                    long randNumber = (long)(random.NextDouble() * 0xffffffffffffL);
                    long ri = System.Threading.Interlocked.Increment(ref lockIdIndex) & 0x7fffffffL;
                    bw.Write((byte)((timestamp >> 40) & 0xff));
                    bw.Write((byte)((timestamp >> 32) & 0xff));
                    bw.Write((byte)((timestamp >> 24) & 0xff));
                    bw.Write((byte)((timestamp >> 16) & 0xff));
                    bw.Write((byte)((timestamp >> 8) & 0xff));
                    bw.Write((byte)(timestamp & 0xff));
                    bw.Write((byte)((randNumber >> 40) & 0xff));
                    bw.Write((byte)((randNumber >> 32) & 0xff));
                    bw.Write((byte)((randNumber >> 24) & 0xff));
                    bw.Write((byte)((randNumber >> 16) & 0xff));
                    bw.Write((byte)((randNumber >> 8) & 0xff));
                    bw.Write((byte)(randNumber & 0xff));
                    bw.Write((byte)((ri >> 24) & 0xff));
                    bw.Write((byte)((ri >> 16) & 0xff));
                    bw.Write((byte)((ri >> 8) & 0xff));
                    bw.Write((byte)(ri & 0xff));
                }
                return ms.GetBuffer();
            }
        }

        public override bool WaitWaiter()
        {
            if (this.waiter == null)
            {
                return false;
            }

            try
            {
                return this.waiter.WaitOne(((int)(this.Timeout & 0xffff) + 120) * 1000);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override Task<bool> WaitTask()
        {
            if (this.taskCompletionSource == null)
            {
                return null;
            }
            Task.Delay(((int)(this.Timeout & 0xffff) + 120) * 1000, this.timeoutCancellationTokenSource.Token).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.Canceled || this.taskCompletionSource == null) return;
                this.taskCompletionSource.SetException(new ClientCommandTimeoutException("The client waits for command execution to return a timeout"));
                this.taskCompletionSource = null;
            });
            return this.taskCompletionSource.Task;
        }
    }
}
