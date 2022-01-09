using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Commands
{
    class LockCommandResult : CommandResult
    {
        public byte Flag { get; protected set; }
        public byte DatabaseId { get; protected set; }
        public byte[] LockId { get; protected set; }
        public byte[] LockKey { get; protected set; }
        public UInt16 LCount { get; protected set; }
        public UInt16 Count { get; protected set; }
        public byte LRCount { get; protected set; }
        public byte RCount { get; protected set; }

        public LockCommandResult() : base()
        {

        }

        public override byte[] DumpCommand()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(ICommand.MAGIC);
                    bw.Write(ICommand.VERSION);
                    bw.Write(ICommand.COMMAND_TYPE_LOCK);
                    bw.Write(this.RequestId, 0, 16);
                    bw.Write(this.Result);
                    bw.Write(this.Flag);
                    bw.Write(this.DatabaseId);
                    bw.Write(this.LockId, 0, 16);
                    bw.Write(this.LockKey, 0, 16);
                    bw.Write(this.LCount & 0xff);
                    bw.Write((this.LCount >> 8) & 0xff);
                    bw.Write(this.Count & 0xff);
                    bw.Write((this.Count >> 8) & 0xff);
                    bw.Write(this.LRCount);
                    bw.Write(this.RCount);
                    bw.Write(new byte[4], 0, 4);
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
                    this.Result = br.ReadByte();
                    this.Flag = br.ReadByte();
                    this.DatabaseId = br.ReadByte();
                    this.LockId = br.ReadBytes(16);
                    this.LockKey = br.ReadBytes(16);
                    this.LCount = (UInt16)(((UInt16)br.ReadByte()) | (((UInt16)br.ReadByte()) << 8));
                    this.Count = (UInt16)(((UInt16)br.ReadByte()) | (((UInt16)br.ReadByte()) << 8));
                    this.LRCount = (byte)br.ReadByte();
                    this.RCount = (byte)br.ReadByte();
                }
            }
            return this;
        }
    }
}
