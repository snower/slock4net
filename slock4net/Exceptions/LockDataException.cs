using System;
using System.Collections.Generic;
using System.Text;

namespace slock4net.Exceptions
{
    public class LockDataException : SlockException
    {
        public LockDataException(string message) : base(message)
        {
        }
    }
}
