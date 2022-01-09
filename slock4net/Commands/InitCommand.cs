using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Commands
{
    class InitCommand : Command
    {
        private static long clientIdIndex = 0;

        public byte[] ClientId { get; protected set; }

        public InitCommand(byte[] clientId) : base(ICommand.COMMAND_TYPE_INIT)
        {
            this.ClientId = clientId;
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
                    bw.Write(this.ClientId, 0, 16);
                    bw.Write(new byte[29], 0, 29);
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
                    this.ClientId = br.ReadBytes(16);
                }
            }
            return this;
        }

        public static byte[] GenClientId()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    long timestamp = (new DateTimeOffset(DateTime.Now)).ToUnixTimeMilliseconds();
                    long randNumber = (long)((new Random()).NextDouble() * 0xffffffffffffffffL);
                    long ri = System.Threading.Interlocked.Increment(ref clientIdIndex) & 0x7fffffffL;
                    bw.Write((byte)(timestamp >> 40) & 0xff);
                    bw.Write((byte)(timestamp >> 32) & 0xff);
                    bw.Write((byte)(timestamp >> 24) & 0xff);
                    bw.Write((byte)(timestamp >> 16) & 0xff);
                    bw.Write((byte)(timestamp >> 8) & 0xff);
                    bw.Write((byte)timestamp & 0xff);
                    bw.Write((byte)(randNumber >> 40) & 0xff);
                    bw.Write((byte)(randNumber >> 32) & 0xff);
                    bw.Write((byte)(randNumber >> 24) & 0xff);
                    bw.Write((byte)(randNumber >> 16) & 0xff);
                    bw.Write((byte)(randNumber >> 8) & 0xff);
                    bw.Write((byte)randNumber & 0xff);
                    bw.Write((byte)(ri >> 24) & 0xff);
                    bw.Write((byte)(ri >> 16) & 0xff);
                    bw.Write((byte)(ri >> 8) & 0xff);
                    bw.Write((byte)ri & 0xff);
                }
                return ms.ToArray();
            }
        }
    }
}
