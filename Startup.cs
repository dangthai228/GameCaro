using Caro.Game.Hubs;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Caro.Game
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static AppSettings AppSettings;
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddControllers();


            services.AddCors(options =>
            {
                options.AddPolicy("Origins",
                builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed((host) => true);
                });
            });

            var appSettingsSection = Configuration.GetSection("AppSettings");

            AppSettings = appSettingsSection.Get<AppSettings>();
            services.Configure<AppSettings>(appSettingsSection);

            services.AddControllersWithViews();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoCaro", Version = "v1" });
            });

            //var key = new HMACSHA256(Convert.FromBase64String(AppSettings.SecretKey));
            var key = Encoding.ASCII.GetBytes(AppSettings.SecretKey);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(x =>
             {
                 x.Events = new JwtBearerEvents
                 {
                     OnMessageReceived = context =>
                     {
                         var path = context.HttpContext.Request.Path;
                         if (path.StartsWithSegments("/CaroHub"))
                         {
                             var jwtToken = context.Request.Query["access_token"];
                             // If the request is for our hub...
                             if (!string.IsNullOrEmpty(jwtToken))
                             {
                                 // Read the token out of the query string
                                 context.Token = jwtToken;
                             }
                         }
                         return Task.CompletedTask;
                     },

                     OnTokenValidated = context =>
                     {

                         return Task.CompletedTask;
                     }
                 };
                 x.RequireHttpsMetadata = false;
                 x.SaveToken = true;
                 x.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = false,
                     ValidateAudience = false,
                     IssuerSigningKey = new SymmetricSecurityKey(key),
                 };
             });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env , ILoggerFactory logger)
        {
            app.UseStaticFiles();
            app.UseRouting();
            LoggerCreator.Instance = logger;    
            app.UseCors("Origins");
            app.UseSwagger();
            /*Enabling Swagger ui, consider doing it on Development env only*/
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoCaro V1");
            });
           
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CaroHub>("/CaroHub");
                endpoints.MapControllers();
            });
        }
    }
}
