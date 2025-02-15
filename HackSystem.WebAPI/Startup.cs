using HackSystem.Web.Authentication.Extensions;
using HackSystem.WebAPI.Authentication.Configurations;
using HackSystem.WebAPI.DataAccess;
using HackSystem.WebAPI.Model.Identity;
using HackSystem.WebAPI.Services.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HackSystem.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure Services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Config CORS
            services.AddCors(options => options.AddPolicy("AllowAny", builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            // Config Database context
            services.AddDbContext<HackSystemDBContext>(options =>
                    options
                        .UseSqlite(this.Configuration.GetConnectionString("HSDB"))
                        .UseLazyLoadingProxies());

            // Register Identity service and options
            services
                .AddIdentity<HackSystemUser, HackSystemRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequiredUniqueChars = 4;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.Lockout.AllowedForNewUsers = true;

                    options.SignIn.RequireConfirmedAccount = false;

                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<HackSystemDBContext>();

            // Config Jwt
            var jwtConfiguration = this.Configuration.GetSection("JwtConfiguration").Get<JwtAuthenticationOptions>();
            services
                .AddAutoMapper(typeof(Startup).Assembly)
                .AddAPIServices()
                .AddAPIAuthentication(jwtConfiguration);

            services
                .AddResponseCompression()
                .AddControllersWithViews()
                .AddNewtonsoftJson();
        }

        /// <summary>
        /// Config Request channel
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseCors("AllowAny");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication
            app.UseAuthentication();
            // Authorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
