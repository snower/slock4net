namespace slock4net.Exceptions
{
    public class ClientClosedException : SlockException
    {
        public ClientClosedException(string message) : base(message)
        {
        }
    }
}