namespace Genocs.FormRecognizer.Worker.Options;

public class RabbitMQSettings
{
    public static string Position = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}