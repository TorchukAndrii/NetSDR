using NetSdrCore.CommandConfigurators.Enums;

namespace NetSdrCore.CommandConfigurators;

public class FrequencyCommandConfigurator : BaseCommandConfigurator
{
    private readonly byte[] _frequencyBytes = new byte[5];
    private byte _channelId;

    public FrequencyCommandConfigurator()
    {
        ControlItemCode = (ushort)NetSdrCommandCode.SetFrequency;
        MessageLength = 0x0A;
    }

    public FrequencyCommandConfigurator SetChannelId(byte id)
    {
        _channelId = id;
        return this;
    }

    public FrequencyCommandConfigurator SetFrequency(ulong frequency)
    {
        // Convert 40-bit frequency to LSB-first order
        _frequencyBytes[0] = (byte)(frequency & 0xFF);
        _frequencyBytes[1] = (byte)((frequency >> 8) & 0xFF);
        _frequencyBytes[2] = (byte)((frequency >> 16) & 0xFF);
        _frequencyBytes[3] = (byte)((frequency >> 24) & 0xFF);
        _frequencyBytes[4] = (byte)((frequency >> 32) & 0xFF);
        return this;
    }

    public override byte[] Build()
    {
        return new byte[]
        {
            MessageLength, 0x00,
            (byte)ControlItemCode, (byte)(ControlItemCode >> 8),
            _channelId,
            _frequencyBytes[0], _frequencyBytes[1], _frequencyBytes[2], _frequencyBytes[3], _frequencyBytes[4]
        };
    }
}