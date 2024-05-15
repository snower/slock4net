namespace slock4net.Exceptions
{
    public class EventWaitTimeoutException : SlockException {
        public EventWaitTimeoutException() : base("event wait timeout")
        {
        }
    }
}
