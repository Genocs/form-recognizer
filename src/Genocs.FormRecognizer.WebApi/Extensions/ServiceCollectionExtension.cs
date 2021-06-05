using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.FormRecognizer.WebApi.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddCustomCache(this IServiceCollection services, IConfiguration configuration)
        {
            string server = configuration["Server"];
            string port = configuration["Port"];

            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(port))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    string server = configuration["Server"];
                    string port = configuration["Port"];
                    string connectionString = $"{server}:{port}";
                    options.Configuration = connectionString;
                });
            }

            return services;
        }
    }
}
