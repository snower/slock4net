using System;

namespace slock4net.Exceptions
{
    public class SlockException : Exception
    {
        public SlockException() : base("slock exception")
        {
        }

        public SlockException(string message) : base(message)
        {
        }
    }
}

