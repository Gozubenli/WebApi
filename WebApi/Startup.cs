using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using WebApi.Data;
using WebApi.Utils;
using Microsoft.EntityFrameworkCore;
using System;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextPool<CrmDbContext>(options => options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr)));
            services.AddAuthorization();
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:sszzz";
                options.UseCamelCasing(true);
                options.UseMemberCasing();
            });
            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var contentRoot = env.ContentRootPath;
            loggerFactory.AddProvider(new LoggerProvider(contentRoot));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            string apiKeys = Configuration.GetValue<string>("ApiKeys");
            foreach (var userKey in apiKeys.Split(";"))
            {
                string user = userKey.Split(":")[0];
                string key = userKey.Split(":")[1];
                Singleton.Instance.ApiKey.Add(user, key);
            } 

            app.UseWhen(x => (x.Request.Path.Value.ToLower().Contains("api/")), //,  .StartsWithSegments("/aplus", StringComparison.OrdinalIgnoreCase)),
            builder =>
            {                
                builder.UseMiddleware<AuthenticationMiddleware>();
            });           

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
