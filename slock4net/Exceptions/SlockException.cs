using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace slock4net.Exceptions
{
    public class SlockException : Exception
    {
        public SlockException() : base()
        {
        }

        public SlockException(String message) : base(message)
        {
        }
    }
}

