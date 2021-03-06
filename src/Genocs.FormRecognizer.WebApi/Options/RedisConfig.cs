namespace Genocs.FormRecognizer.WebApi.Options
{
    public class RedisConfig
    {
        public const string Position = "RedisConfig";
        public string ConnectionStringAdmin => $"{ConnectionStringTxn},allowAdmin=true";

        public string ConnectionStringTxn { get; internal set; }

        public override string ToString()
        {
            return $"{ConnectionStringTxn}";
        }
    }
}
