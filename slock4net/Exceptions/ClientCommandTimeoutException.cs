namespace slock4net.Exceptions
{
    public class ClientCommandTimeoutException : SlockException
    {
        public ClientCommandTimeoutException(string message) : base(message)
        {
        }
    }
}
