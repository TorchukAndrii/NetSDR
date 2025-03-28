namespace NetSdrClient.Exceptions;

public class TcpCommunicationException : Exception
{
    public TcpCommunicationException(string message, Exception ex) : base(message, ex)
    {
    }

    public TcpCommunicationException(string message) : base(message)
    {
    }
}