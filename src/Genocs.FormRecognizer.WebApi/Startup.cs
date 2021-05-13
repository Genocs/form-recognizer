using Genocs.Integration.MSAzure.Options;
using Genocs.Integration.MSAzure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Genocs.FormRecognizer.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AzureStorageConfig>(Configuration.GetSection("AzureStorageConfig"));
            services.Configure<AzureCognitiveServicesConfig>(Configuration.GetSection("AzureCognitiveServicesConfig"));
            services.Configure<AzureCognitiveServicesImageClassifierConfig>(Configuration.GetSection("AzureCognitiveServicesImageClassifierConfig"));

            services.AddSingleton<StorageService>();
            services.AddSingleton<FormRecognizerService>();
            services.AddSingleton<FormClassifierService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Genocs FormRecognizer WebApi", Version = "v1", Description = "Genocs FormRecognizer WebApi service" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Genocs.FormRecognizerService v1"));


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
