using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.FormRecognizer.WebApi.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCustomCache(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration["ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
            });
        }

        return services;
    }
}
