namespace Genocs.FormRecognizer.WebApi.Options
{
    public class RabbitMQConfig
    {
        public const string Position = "RabbitMQConfig";
        public string URL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }


        public static bool IsNullOrEmpty(RabbitMQConfig rabbitMQSettings)
        {
            if (rabbitMQSettings == null)
            {
                return true;
            }

            if (rabbitMQSettings.URL == null)
            {
                return true;
            }

            return false;
        }
    }
}


