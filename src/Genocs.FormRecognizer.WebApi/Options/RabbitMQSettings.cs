namespace Genocs.FormRecognizer.WebApi.Options;

public class RabbitMQSettings
{
    public static string Position = "RabbitMQSettings";

    public string HostName { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}