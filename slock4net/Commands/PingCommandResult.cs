using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Commands
{
    class PingCommandResult : CommandResult
    {
        public PingCommandResult() : base()
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
                    bw.Write(new byte[44], 0, 44);
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
                }
            }
            return this;
        }
    }
}
