using Analytics.Database;
using Analytics.Database.ConnectionFactories;
using Analytics.Database.QueryBuilders;
using AnalyticsCollector.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using System;

namespace AnalyticsCollector
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public class ElasticConfiguration
        {
            public string[] Nodes { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Index { get; set; }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options => options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

            var section = Configuration.GetSection(nameof(ElasticConfiguration));
            var elasticConfig = section.Get<ElasticConfiguration>();

            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            services.AddScoped<IConnectionFactory>(s => new ConnectionFactory(elasticConfig.Nodes, elasticConfig.Username, elasticConfig.Password, elasticConfig.Index, null));
            services.AddScoped<IAnalyticsEntryDtoMapper, AnalyticsEntryDtoMapper>();
            services.AddScoped<IBrowserResolver, BrowserResolver>();

            services.AddSingleton<IAnalyticsEntryDtoMapper>(s => new AnalyticsEntryDtoMapper(s.GetRequiredService<IBrowserResolver>()));

            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetPreflightMaxAge(TimeSpan.FromDays(1));
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("MyPolicy");
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
