namespace Genocs.Integration.CognitiveServices.Options;

public class RabbitMQSettings
{
    public static string Position = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}