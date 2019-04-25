using jb_core_webapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace jb_core_webapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IJellyblogDbContext, JellyblogDbContext>();
            services.AddSingleton<IJellyblogDbFileService, JellyblogDbFileService>();
            services.AddSingleton<IJellyblogDbUserService, JellyblogDbUserService>();
            services.AddSingleton<IJellybolgDbRefreshTokenService, JellybolgDbRefreshTokenService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = !Environment.IsDevelopment();
                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidIssuer = Configuration.GetValue<string>("AuthTokenIssuer");
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudience = Configuration.GetValue<string>("AuthTokenAudience");
                    options.TokenValidationParameters.ValidateLifetime = true;
                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetValue<string>("AuthTokenKey")));
                });

            //services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options => // CookieAuthenticationOptions
            //    {
            //        options.ClaimsIssuer = Configuration.GetValue<string>("AuthTokenIssuer");
            //        options.Cookie.Expiration = System.TimeSpan.FromDays(3);
            //        options.Cookie.HttpOnly = true;
            //        options.Cookie.Name = "jb.sess";
            //        options.ExpireTimeSpan = System.TimeSpan.FromDays(3);
            //        // override to 401 status response
            //        options.Events.OnRedirectToLogin = context => 
            //        {
            //            // not beatiful
            //            context.Response.Redirect(null);
            //            context.Response.StatusCode = 401;
            //            return Task.FromResult(0);
            //        };
            //    });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
            }

            app.UseAuthentication();

            // Not actually need if works behind nginx
            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
