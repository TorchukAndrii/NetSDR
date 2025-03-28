namespace NetSdrCore.CommandConfigurators;

public abstract class BaseCommandConfigurator
{
    protected ushort ControlItemCode;
    protected byte MessageLength;

    public abstract byte[] Build();

    public static implicit operator byte[](BaseCommandConfigurator config)
    {
        return config.Build();
    }
}