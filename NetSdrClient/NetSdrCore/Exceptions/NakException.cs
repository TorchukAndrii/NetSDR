namespace NetSdrCore.Exceptions;

public class NakException : Exception
{
    public NakException() : base("Received NAK: Control Item not supported.") { }
}