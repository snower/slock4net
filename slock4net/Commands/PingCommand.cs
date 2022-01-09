using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Commands
{
    class PingCommand : Command
    {
        public PingCommand() : base(ICommand.COMMAND_TYPE_PING)
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
                    bw.Write(new byte[45], 0, 45);
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
                }
            }
            return this;
        }
    }
}
