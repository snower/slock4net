using System.IO;

namespace slock4net.Commands
{
    public class LockCommandResult : CommandResult
    {
        public byte Flag { get; protected set; }
        public byte DatabaseId { get; protected set; }
        public byte[] LockId { get; protected set; }
        public byte[] LockKey { get; protected set; }
        public ushort LCount { get; protected set; }
        public ushort Count { get; protected set; }
        public byte LRCount { get; protected set; }
        public byte RCount { get; protected set; }

        public LockCommandResult() : base()
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
                    bw.Write(this.Result);
                    bw.Write(this.Flag);
                    bw.Write(this.DatabaseId);
                    bw.Write(this.LockId, 0, 16);
                    bw.Write(this.LockKey, 0, 16);
                    bw.Write((byte)(this.LCount & 0xff));
                    bw.Write((byte)((this.LCount >> 8) & 0xff));
                    bw.Write((byte)(this.Count & 0xff));
                    bw.Write((byte)((this.Count >> 8) & 0xff));
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
                    this.LCount = (ushort)(((ushort)br.ReadByte()) | (((ushort)br.ReadByte()) << 8));
                    this.Count = (ushort)(((ushort)br.ReadByte()) | (((ushort)br.ReadByte()) << 8));
                    this.LRCount = (byte)br.ReadByte();
                    this.RCount = (byte)br.ReadByte();
                }
            }
            return this;
        }
    }
}
