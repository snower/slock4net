using slock4net.datas;
using System;
using System.Security.Cryptography;

namespace slock4net
{
    public abstract class AbstractExecution
    {
        protected readonly SlockDatabase database;
        protected byte[] lockKey;
        protected uint timeout;
        protected uint expried;
        protected ushort count;
        protected byte rCount;
        protected LockResultData currentLockData;

        public AbstractExecution(SlockDatabase database, byte[] lockKey, uint timeout, uint expried, ushort count, byte rCount)
        {
            this.database = database;
            if (lockKey.Length > 16)
            {
                using (MD5 md5 = MD5.Create())
                {
                    this.lockKey = md5.ComputeHash(lockKey);
                }
            }
            else
            {
                this.lockKey = new byte[16];
                Array.Copy(lockKey, 0, this.lockKey, 16 - lockKey.Length, lockKey.Length);
            }
            this.timeout = timeout;
            this.expried = expried;
            this.count = count;
            this.rCount = rCount;
        }

        public AbstractExecution(SlockDatabase database, byte[] lockKey, uint timeout, uint expried)
            : this(database, lockKey, timeout, expried, 0, 0)
        {
        }

        public ushort Timeout 
        {
            get { return (ushort)timeout; }
            set { this.timeout = (((uint)value) & 0xffff) | (this.timeout & 0xffff0000); }
        }


        public ushort TimeoutFlag
        {
            get { return (ushort)(timeout >> 16); }
            set { this.timeout = ((((uint)value) & 0xffff) << 16) | (this.timeout & 0xffff); }
        }

        public void UpdateTimeoutFlag(ushort timeoutFlag)
        {
            this.timeout = ((((uint)timeoutFlag) & 0xffff) << 16) | this.timeout;
        }

        public ushort Expried
        {
            get { return (ushort)expried; }
            set { this.expried = (((uint)expried) & 0xffff) | (this.expried & 0xffff0000); }
        }

        public ushort ExpriedFlag
        {
            get { return (ushort)(expried >> 16); }
            set { this.expried = ((((uint)value) & 0xffff) << 16) | (this.expried & 0xffff); }
        }

        public void UpdateExpriedFlag(ushort expriedFlag)
        {
            this.expried = ((((uint)expriedFlag) & 0xffff) << 16) | this.expried;
        }

        public ushort Count
        {
            get { return count; }
            set { this.count = value; }
        }

        public byte RCount
        {
            get { return rCount; }
            set { this.rCount = value; }
        }

        public LockResultData CurrentLockData
        {
            get { return currentLockData; }
        }

        public byte[] CurrentLockDataAsBytes
        {
            get
            {
                if (currentLockData == null)
                {
                    return null;
                }
                return currentLockData.DataAsBytes;
            }
        }

        public string CurrentLockDataAsString
        {
            get
            {
                if (currentLockData == null)
                {
                    return null;
                }
                return currentLockData.DataAsString;
            }
        }

        public long CurrentLockDataAsLong
        {
            get
            {
                if (currentLockData == null)
                {
                    return 0L;
                }
                return currentLockData.DataAsLong;
            }
        }
    }
}
