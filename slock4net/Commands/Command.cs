using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Commands
{
    class Command : ICommand
    {
        private static long requestIdIndex = 0;
        public byte Magic { get; protected set; }
        public byte Version { get; protected set; }
        public byte CommandType { get; protected set; }
        public byte[] RequestId { get; protected set; }

        protected System.Threading.Semaphore waiter;
        public CommandResult CommandResult;
        public Command(byte commandType)
        {
            this.CommandType = commandType;
            this.RequestId = Command.GenRequestId();
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
                }
            }
            return this;
        }

        public static byte[] GenRequestId()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    long timestamp = (new DateTimeOffset(DateTime.Now)).ToUnixTimeMilliseconds();
                    long randNumber = (long) ((new Random()).NextDouble() * 0xffffffffffffffffL);
                    long ri = System.Threading.Interlocked.Increment(ref requestIdIndex) & 0x7fffffffL;
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

        public virtual bool CreateWaiter()
        {
            this.waiter = new System.Threading.Semaphore(1, 1);
            return true;
        }

        public virtual bool WakeupWaiter()
        {
            if (this.waiter == null)
            {
                return false;
            }
            this.waiter.Release();
            return true;
        }

        public virtual bool WaiteWaiter()
        {
            if (this.waiter == null)
            {
                return false;
            }

            try
            {
                return this.waiter.WaitOne(5000);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
