namespace NetSdrClient.Helpers;

public class MessageHeader
{
    private MessageHeader(int length, byte messageType)
    {
        Length = length;
        MessageType = messageType;
    }

    public int Length { get; }
    public byte MessageType { get; }

    public static MessageHeader FromBytes(byte[] bytes)
    {
        if (bytes.Length != 2)
            throw new ArgumentException("Header must be exactly 2 bytes.", nameof(bytes));

        // Parse header: First byte is LSB of length, first 3 bits of second byte are the message type,
        // and last 5 bits are the MSB of the length.
        var lengthLsb = bytes[0];
        var lengthMsbAndType = bytes[1];

        var messageType = (byte)(lengthMsbAndType & 0b00000111); // Extract the lowest 3 bits
        var lengthMsb = (lengthMsbAndType >> 3) & 0b00011111; // Extract the upper 5 bits

        var length = (lengthMsb << 8) | lengthLsb; // Combine MSB and LSB to form 13-bit length

        // Special case: Length 0 means actual length is 8194
        if (length == 0)
            length = 8194;

        return new MessageHeader(length, messageType);
    }

    public byte[] ToBytes()
    {
        var lengthToEncode = Length == 8194 ? 0 : Length; // Handle special case for 8194 bytes

        var lengthLsb = (byte)(lengthToEncode & 0xFF);
        var lengthMsbAndType = (byte)(((lengthToEncode >> 8) & 0x1F) | (MessageType & 0x07));

        return new[] { lengthLsb, lengthMsbAndType };
    }
}