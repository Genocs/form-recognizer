namespace Genocs.FormRecognizer.WebApi.Options;

public class RabbitMQConfig
{
    public const string Position = "RabbitMQConfig";
    public string URL { get; set; } = "localhost";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";


    public static bool IsNullOrEmpty(RabbitMQConfig rabbitMQSettings)
    {
        if (rabbitMQSettings == null)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(rabbitMQSettings.URL))
        {
            return true;
        }

        return false;
    }
}


