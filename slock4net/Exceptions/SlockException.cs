using System;

namespace slock4net.Exceptions
{
    public class SlockException : Exception
    {
        public SlockException() : base()
        {
        }

        public SlockException(string message) : base(message)
        {
        }
    }
}

