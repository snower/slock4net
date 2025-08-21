using System.IO;

namespace slock4net.Commands
{
    public class CommandResult : ICommand
    {
        public byte Magic { get; protected set; }
        public byte Version { get; protected set; }
        public byte CommandType { get; protected set; }
        public byte[] RequestId { get; protected set; }
        public byte Result { get; protected set; }
        public CommandResult()
        {

        }
        public byte GetCommandType()
        {
            return this.CommandType;
        }
        public byte[] GetRequestId()
        {
            return this.RequestId;
        }
        public virtual byte[] DumpCommand()
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
                    bw.Write(new byte[44], 0, 44);
                }
                return ms.GetBuffer();
            }
        }

        public virtual CommandResult LoadCommandData(byte[] _)
        {
            return this;
        }

        public virtual ICommand LoadCommand(byte[] buffer)
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
                }
            }
            return this;
        }

        public virtual bool HasExtraData()
        {
            return false;
        }
    }
}
