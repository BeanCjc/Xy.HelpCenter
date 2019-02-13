﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Xy.HelpCenter.Autofac;
using Xy.IRepository.Base;
using AutoMapper;

namespace Xy.HelpCenter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            BaseDbConfig.ConnectionString = Configuration.GetSection("AppSettings:SqlServerConnection").Value;
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            #region Swagger
            services.AddSwaggerGen(options =>
            {
                //注册swaggerAPI文档服务,单版本
                options.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Version = "v1.0",
                    Title = "帮助中心接口文档",
                    Description = "HelpCenter Http Api v1.0"
                });
                //添加注释
                //var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;
                //var basePath = Directory.GetCurrentDirectory();
                //options.IncludeXmlComments(Path.Combine(basePath, "Xy.HelpCenter.xml"));
                options.IncludeXmlComments(Path.Combine(Directory.GetCurrentDirectory(), "Xy.HelpCenter.xml"));

                //方案名称“Blog.Core”可自定义，上下一致即可
                options.AddSecurityDefinition("Bearer", new Swashbuckle.AspNetCore.Swagger.ApiKeyScheme { Name = "Authorization", In = "header", Description = "Format: Bearer {auth_token}", Type = "apiKey" });

                //jwt认证方式，此方式为全局添加
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "Bearer", new string[] { } }, });
            });
            #endregion

            //AutoMapper
            services.AddAutoMapper(typeof(Startup));

            //log4net

            //auth

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //Autofac DI
            return AutofacHelper.RegisterServices(services);
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSwagger().UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "帮助中心v1.0");
            });
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseStatusCodePages();//把错误码返回前台，比如是404
            app.UseMvc();
        }
    }
}
