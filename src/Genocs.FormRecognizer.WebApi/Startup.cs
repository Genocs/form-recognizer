using Genocs.FormRecognizer.WebApi.Extensions;
using Genocs.FormRecognizer.WebApi.Settings;
using Genocs.Integration.ML.CognitiveServices.Interfaces;
using Genocs.Integration.ML.CognitiveServices.Options;
using Genocs.Integration.ML.CognitiveServices.Services;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Security.Authentication;

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

            services.Configure<AzureCognitiveServicesConfig>(Configuration.GetSection("AzureCognitiveServicesConfig"));
            services.Configure<AzureStorageConfig>(Configuration.GetSection("AzureStorageConfig"));
            services.Configure<ImageClassifierConfig>(Configuration.GetSection("ImageClassifierConfig"));
            services.Configure<AzureCognitiveServicesConfig>(Configuration.GetSection("FormRecognizerConfig"));
            services.Configure<RabbitMQSettings>(Configuration.GetSection(RabbitMQSettings.Position));

            services.AddSingleton<StorageService>();
            services.AddSingleton<IFormRecognizer, FormRecognizerService>();
            services.AddSingleton<IImageClassifier, ImageClassifierService>();
            services.AddSingleton<ICardIdRecognizer, CardIdRecognizerService>();

            services.AddCustomCache(Configuration.GetSection("RedisConfig"));

            ConfigureMassTransit(services);

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

        /// <summary>
        /// Setup Masstransit with RabbitMQ transport and MongoDB persistance layer
        /// </summary>
        /// <param name="services">The service collection</param>
        private void ConfigureMassTransit(IServiceCollection services)
        {
            RabbitMQSettings rabbitMqSettings = new RabbitMQSettings();
            Configuration.GetSection(RabbitMQSettings.Position).Bind(rabbitMqSettings);

            if(RabbitMQSettings.IsNullOrEmpty(rabbitMqSettings))
            {
                return;
            }

            services.AddMassTransit(x =>
            {
                // Consumer
                //x.AddConsumer<SubmitOrderConsumer>();
                //x.AddConsumer<OrderAcceptedConsumer>();

                // Transport RabbitMQ
                x.UsingRabbitMq((context, cfg) =>
                {
                    RabbitMQSettings rabbitMqSettings = new RabbitMQSettings();
                    Configuration.GetSection(RabbitMQSettings.Position).Bind(rabbitMqSettings);

                    cfg.Host(rabbitMqSettings.URL, 5671, rabbitMqSettings.VirtualHost, h =>
                    {
                        h.Username(rabbitMqSettings.Username);
                        h.Password(rabbitMqSettings.Password);

                        h.UseSsl(s =>
                        {
                            s.Protocol = SslProtocols.Tls12;
                        });
                    });

                    //cfg.ReceiveEndpoint("submit-order", e =>
                    //{
                    //    e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10)));

                    //    e.Consumer(() => new SubmitOrderConsumer());
                    //});

                    //cfg.ReceiveEndpoint("order-accepted", e =>
                    //{
                    //    e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10)));

                    //    e.Consumer(() => new OrderAcceptedConsumer());
                    //});

                    //cfg.ReceiveEndpoint("home-made", e =>
                    //{
                    //    e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10)));

                    //    e.Consumer(() => new TaxFreeFormConsumer());
                    //});


                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddMassTransitHostedService();
        }
    }
}
