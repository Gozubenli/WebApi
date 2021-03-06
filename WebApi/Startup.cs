using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using WebApi.Data;
using WebApi.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

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
            //services.AddDbContextPool<CrmDbContext>(options => options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr)));


            //Sql Server Baglantisi
            //string sqlServerConnection = Configuration.GetConnectionString("SqlServerConnection");
            //services.AddDbContextFactory<abkmcomt_aplusContext>(options =>
            //options.UseSqlServer(sqlServerConnection, o => o.CommandTimeout(30)));


            services.AddDbContextFactory<CrmDbContext>(options => options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr)));
            services.AddAuthorization();
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:sszzz";
                options.UseCamelCasing(true);
                options.UseMemberCasing();
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
            });
            //services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<CrmDbContext>().AddDefaultTokenProviders();
            //services.AddIdentity<ApplicationUser, ApplicationRole>().AddDefaultTokenProviders();
            services.AddMemoryCache();
            services.AddCors();
            services.AddSession();
            services.AddResponseCompression();


            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            var contentRoot = env.ContentRootPath;
            var parent = Directory.GetParent(contentRoot).FullName;
            loggerFactory.AddProvider(new LoggerProvider(parent));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true)
               .AllowCredentials());

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
