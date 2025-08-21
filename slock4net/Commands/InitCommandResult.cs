using System.IO;

namespace slock4net.Commands
{
    public class InitCommandResult : CommandResult
    {
        public byte InitType { get; protected set; }

        public InitCommandResult() : base()
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
                    bw.Write(this.InitType);
                    bw.Write(new byte[43], 0, 43);
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
                    this.Result = br.ReadByte();
                    this.InitType = br.ReadByte();
                }
            }
            return this;
        }
    }
}
