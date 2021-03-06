﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using StreamingWebshop.Core.ApplicationService;
using StreamingWebshop.Core.ApplicationService.Services;
using StreamingWebshop.Core.DomainService;
using StreamingWebshop.Infrastructure.Data;
using StreamingWebshop.Infrastructure.Data.Repositories;
using StreamShopRestAPI.Helpers;

namespace StreamShopRestAPI
{
    public class Startup
    {

        private IConfiguration _conf { get; }

        private IHostingEnvironment _env { get; set; }

        //For online Database / Azure
        public Startup(IHostingEnvironment env)
        {
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _conf = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Create a byte array with random values. This byte array is used
            // to generate a key for signing JWT tokens.
            Byte[] secretBytes = new byte[40];
            Random rand = new Random();
            rand.NextBytes(secretBytes);

            // Add JWT based authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
                    ValidateLifetime = true, //validate the expiration and not before values in the token
                    ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            });

            /*
            //In-Memory Database FakeSQL DB
            services.AddDbContext<Context>(
                opt => opt.UseInMemoryDatabase("FakeSQL-DB"));
                */

            if (_env.IsDevelopment())
            {
                services.AddDbContext<Context>(
                    opt => opt.UseSqlite("Data Source=StreamBoss.db"));
            }
            //For SQLite DB.. Needs actual lists and tables
            //ConnectionString fra Azure (Online)
            else if (_env.IsProduction())
            {
                services.AddDbContext<Context>(
                    opt => opt.UseSqlServer(_conf.GetConnectionString("defaultConnection")));
            }

            //dependency injection

            //Product
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            //User
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            // Register the AuthenticationHelper in the helpers folder for dependency
            // injection. It must be registered as a singleton service. The AuthenticationHelper
            // is instantiated with a parameter. The parameter is the previously created
            // "secretBytes" array, which is used to generate a key for signing JWT tokens,
            services.AddSingleton<IAuthenticationHelper>(new AuthenticationHelper(secretBytes));

            //For Ignoring Loop References
            services.AddMvc().AddJsonOptions(Options =>
            {
                Options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //For Cors Options
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        //.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
                        //.WithOrigins("FIREBASE").AllowAnyHeader().AllowAnyMethod()
                        .WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //for database on startup
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetService<Context>();

                    DBSeeder.SeedDB(ctx);
                }

            }
            else
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetService<Context>();
                    ctx.Database.EnsureCreated();
                }
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            //Allow Any Cors
            ////app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
