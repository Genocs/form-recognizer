namespace Genocs.FormRecognizer.WebApi.Settings
{
    public class RedisSettings
    {
        public const string Position = "RabbitMQSettings";
        public string ConnectionStringAdmin => $"{this.ConnectionStringTxn},allowAdmin=true";

        public string ConnectionStringTxn { get; internal set; }

        public override string ToString()
        {
            return $"{ConnectionStringTxn}";
        }
    }
}
