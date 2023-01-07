namespace Genocs.FormRecognizer.WebApi.Options;

public class RedisSettings
{
    public const string Position = "RedisSettings";
    public string ConnectionStringAdmin => $"{ConnectionStringTxn},allowAdmin=true";

    public string ConnectionStringTxn { get; internal set; }

    public override string ToString()
    {
        return ConnectionStringTxn;
    }
}
