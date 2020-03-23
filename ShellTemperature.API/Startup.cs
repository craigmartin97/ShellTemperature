using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShellTemperature.Data;
using ShellTemperature.Repository;
using ShellTemperature.Repository.Interfaces;
using System;

namespace ShellTemperature.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option => option.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Configure the database
            services.AddDbContext<ShellDb>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ShellConnection")
                    , optionsBuilder =>
                    {
                        optionsBuilder.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
                    }));

            // Add in scoped items
            services.AddScoped<IShellTemperatureRepository<ShellTemp>, ShellTemperatureRepository>();
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

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
