﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfreeze.Services;
using Dfreeze.Services.Background;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfreeze
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
            services.AddLogging();
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddHttpContextAccessor();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<INameGeneratorService, NameGeneratorService>();
            services.AddSingleton<IFreezeService, FreezeService>();
            services.AddSingleton<IHtmlParserService, HtmlParserService>();
            services.AddSingleton<IFreezerStateHolder, FreezerStateHolder>();
            services.AddSingleton<IFreezerTasksProcessor, FreezerTasksProcessor>();
            services.AddSingleton<IAnalyticsService, AnalyticsService>();

            services.AddHostedService<FreezerBackgroundWorker>();
            services.AddHostedService<FreezerInfoUpdater>();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseExceptionHandler("/api/error");
                app.UseStatusCodePages();
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseMvc();
        }
    }
}
