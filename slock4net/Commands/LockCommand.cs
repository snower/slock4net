using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Commands
{
    public class LockCommand : Command
    {
        private static long lockIdIndex = 0;
        public byte Flag { get; protected set; }
        public byte DatabaseId { get; protected set; }
        public byte[] LockId { get; protected set; }
        public byte[] LockKey { get; protected set; }
        public UInt32 Timeout { get; protected set; }
        public UInt32 Expried { get; protected set; }
        public UInt16 Count { get; protected set; }
        public byte RCount { get; protected set; }
        public LockCommand(byte commandType, byte flag, byte datbaseId, byte[] lockKey, byte[] lockId,
            UInt32 timeout, UInt32 expried, UInt16 count, byte rCount) : base(commandType)
        {
            this.Flag = flag;
            this.DatabaseId = datbaseId;
            this.LockKey = lockKey;
            this.LockId = lockId;
            this.Timeout = timeout;
            this.Expried = expried;
            this.Count = count;
            this.RCount = rCount;
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
                return ms.ToArray();
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
                    this.Timeout = ((UInt32)br.ReadByte()) | (((UInt32)br.ReadByte()) << 8) | (((UInt32)br.ReadByte()) << 16) 
                        | (((UInt32)br.ReadByte()) << 24);
                    this.Expried = ((UInt32)br.ReadByte()) | (((UInt32)br.ReadByte()) << 8) | (((UInt32)br.ReadByte()) << 16)
                        | (((UInt32)br.ReadByte()) << 24);
                    this.Count = (UInt16)(((UInt16)br.ReadByte()) | (((UInt16)br.ReadByte()) << 8));
                    this.RCount = (byte)br.ReadByte();
                }
            }
            return this;
        }

        public static byte[] GenLockId()
        {
            using (MemoryStream ms = new MemoryStream(16))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    long timestamp = (new DateTimeOffset(DateTime.Now)).ToUnixTimeMilliseconds();
                    long randNumber = (long)((new Random()).NextDouble() * 0xffffffffffffffffL);
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
                return ms.ToArray();
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
                return this.waiter.WaitOne(((int)(this.Timeout & 0xffff) + 1) * 1000);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
