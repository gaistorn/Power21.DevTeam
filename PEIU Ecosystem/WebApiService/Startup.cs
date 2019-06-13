using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PES.Toolkit;
using PES.Toolkit.Config;
using StackExchange.Redis.Extensions.Core.Configuration;
using Power21.PEIUEcosystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PES.Toolkit.Auth;
using System.Globalization;
using Microsoft.AspNetCore.Identity.UI.Services;
using PES.Toolkit.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Rewrite;

namespace PES.Service.WebApiService
{
    public class Startup
    {
        IList<CultureInfo> supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ko-KR"),
            new CultureInfo("ko"),
        };

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //MongoDB.Driver.MongoClient client = new MongoDB.Driver.MongoClient(Configuration.GetConnectionString("mongodb"));

            services.AddDbContext<AccountRecordContext>(
                options => options.UseMySql(Configuration.GetConnectionString("mysqldb"))
                );

            services.AddIdentity<AccountModel, IdentityRole>()
                .AddEntityFrameworkStores<AccountRecordContext>()
                .AddErrorDescriber<Localization.LocalizedIdentityErrorDescriber>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "http://www.peiu.com",
                        ValidAudience = "http://www.peiu.com",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JasonWebTokenManager.Secret))
                    };
                })
                .AddCookie();

            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");
            ConfigureIdentity(services);
            services.AddCors();

            var map_reduces = Configuration.GetSection("MongoMapReduces").Get<IEnumerable<MongoMapReduceConfig>>();
            var email_config = Configuration.GetSection("EmailSender").Get<EmailConfigOption>();

            var redisConfiguration = Configuration.GetSection("redis").Get<RedisConfiguration>();
            services.AddSingleton(redisConfiguration);
            services.AddSingleton(email_config);
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            //services.AddSingleton(client);


            //IServiceCollection cols  = services.AddSingleton<IBackgroundMongoTaskQueue, MongoBackgroundTaskQueue>();
            //services.AddSingleton<MQTTDaegunSubscribe>();


            //services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(
            //        builder =>
            //        {
            //            builder.WithOrigins("http://118.216.255.118:3011")
            //            .AllowAnyHeader()
            //                            .AllowAnyMethod();
            //        });
            //    options.AddPolicy("PeiuPolicy",
            //    builder =>
            //    {
            //        builder.AllowAnyOrigin()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader();
            //        //.AllowCredentials();
            //    });
            //});
            //services.Configure<MvcOptions>(options =>
            //{
            //    options.Filters.Add(new RequireHttpsAttribute());
            //});
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddPortableObjectLocalization();
            services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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

            var withOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();

            app.UseHttpsRedirection();
            app.UseCors(builder =>
            {
                builder
                    //.AllowAnyOrigin()
                    .WithOrigins(withOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .AllowCredentials();
            });
            //app.UseCors("PeiuPolicy");
            //app.UseMiddleware(typeof(CorsMiddleware));
            //var options = new RewriteOptions().AddRedirectToHttps(StatusCodes.Status301MovedPermanently, 3012);
            app.UseRequestLocalization();
            app.UseAuthentication();
            
            app.UseMvc();
        }

        private void ConfigureIdentity(IServiceCollection services)
        {


            // Configure Localization
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ko-KR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Account2/AccessDenied2");
                options.Cookie.Domain = null;
                options.Cookie.Name = "PEIU.Auth.Cookie";
                //options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(720);
                //options.LoginPath = new PathString("/api/auth/logintoredirec");
                //options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;

                
            });
            var pass_options = Configuration.GetSection("PasswordPolicy").Get<PasswordOptions>();
            
            services.Configure<IdentityOptions>(options =>
            {
                options.Password = pass_options;
            });

            //    // Lockout settings
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            //    options.Lockout.MaxFailedAccessAttempts = 10;



            //    //// Cookie settings
            //    //options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
            //    //options.Cookies.ApplicationCookie.LoginPath = "/Account/LogIn";
            //    //options.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOff";

            //    // User settings
            //    options.User.RequireUniqueEmail = true;

            //});

            //services.ConfigureApplicationCookie(options =>
            //{
            //    // Cookie settings
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.Expiration = TimeSpan.FromDays(150);
            //    // If the LoginPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/Login.
            //    options.LoginPath = "/Account/Login";
            //    // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/AccessDenied.
            //    //options.AccessDeniedPath = "/Account/AccessDenied";
            //    options.AccessDeniedPath = "/home";
            //    options.SlidingExpiration = true;
            //});
        }

    }
}
