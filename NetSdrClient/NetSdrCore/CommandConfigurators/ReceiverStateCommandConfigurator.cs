using NetSdrCore.CommandConfigurators.Enums;

namespace NetSdrCore.CommandConfigurators;

public class ReceiverStateCommandConfigurator : BaseCommandConfigurator
{
    private byte _captureMode;
    private byte _channelSpecifier;
    private byte _fifoCount;
    private byte _runStopControl;

    public ReceiverStateCommandConfigurator()
    {
        ControlItemCode = (ushort)NetSdrCommandCode.SetReceiverState;
        MessageLength = 0x08;
    }

    public ReceiverStateCommandConfigurator SetStartCommand()
    {
        _channelSpecifier = 0x80;
        _captureMode = 0x80;
        _fifoCount = 0x00;
        _runStopControl = (byte)ReceiverStateRunStopControl.Start;
        return this;
    }

    public ReceiverStateCommandConfigurator SetStopCommand() //parameters 1,3, and 4 are ignored for the stop command
    {
        _channelSpecifier = 0x00;
        _captureMode = 0x00;
        _fifoCount = 0x00;
        _runStopControl = (byte)ReceiverStateRunStopControl.Stop;
        return this;
    }

    public override byte[] Build()
    {
        return new byte[]
        {
            MessageLength, 0x00,
            (byte)ControlItemCode, (byte)(ControlItemCode >> 8),
            _channelSpecifier, _runStopControl, _captureMode, _fifoCount
        };
    }
}