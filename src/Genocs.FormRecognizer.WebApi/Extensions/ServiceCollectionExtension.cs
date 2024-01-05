using Genocs.FormRecognizer.WebApi.Options;
using MassTransit;

namespace Genocs.FormRecognizer.WebApi.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new RabbitMQSettings();
        configuration.GetSection(RabbitMQSettings.Position).Bind(settings);

        services.AddSingleton(settings);

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);

                // cfg.UseHealthCheck(context);
                cfg.Host(settings.HostName, settings.VirtualHost,
                    h =>
                    {
                        h.Username(settings.UserName);
                        h.Password(settings.Password);
                    });
            });
        });

        return services;
    }

    public static IServiceCollection AddCustomCache(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new RedisSettings();
        configuration.GetSection(RedisSettings.Position).Bind(settings);

        services.AddSingleton(settings);

        if (string.IsNullOrWhiteSpace(settings.ConnectionStringTxn))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = settings.ConnectionStringTxn;
            });
        }

        return services;
    }
}
