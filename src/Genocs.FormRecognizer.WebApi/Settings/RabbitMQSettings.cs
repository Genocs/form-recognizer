namespace Genocs.FormRecognizer.WebApi.Settings
{
    public class RabbitMQSettings
    {
        public const string Position = "RabbitMQSettings";
        public string URL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }


        public static bool IsNullOrEmpty(RabbitMQSettings rabbitMQSettings)
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


