﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EdaSample.Common.Events;
using EdaSample.EventBus.Simple;
using EdaSample.EventStores.Dapper;
using EdaSample.Integration.AspNetCore;
using EdaSample.Services.Customer.EventHandlers;
using EdaSample.Services.Customer.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EdaSample.Services.Customer
{
    public class Startup
    {
        private readonly ILogger logger;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            this.logger = loggerFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            this.logger.LogInformation("正在对服务进行配置...");

            services.AddMvc();

            services.AddTransient<IEventStore>(serviceProvider => 
                new DapperEventStore(Configuration["mssql:connectionString"], 
                    serviceProvider.GetRequiredService<ILogger<DapperEventStore>>()));

            var eventHandlerExecutionContext = new EventHandlerExecutionContext(services, 
                sc => sc.BuildServiceProvider());
            services.AddSingleton<IEventHandlerExecutionContext>(eventHandlerExecutionContext);
            services.AddSingleton<IEventBus, PassThroughEventBus>();

            this.logger.LogInformation("服务配置完成，已注册到IoC容器！");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<CustomerCreatedEvent, CustomerCreatedEventHandler>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
