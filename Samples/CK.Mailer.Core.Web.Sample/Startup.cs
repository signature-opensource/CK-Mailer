using System;
using CK.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CK.Mailer.Core.Web.Sample
{
    public class Startup
    {
        readonly IConfiguration _configuration;
        readonly IActivityMonitor _activityMonitor;

        public Startup( IConfiguration configuration, IWebHostEnvironment env )
        {
            _configuration = configuration;
            _activityMonitor = new ActivityMonitor( $"App {env.ApplicationName}/{env.EnvironmentName} on {Environment.MachineName}/{Environment.UserName}." );
        }

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddControllers();

            //Add MailKit
            services.AddMailKit( optionBuilder =>
            {
                MailKitOptions options = optionBuilder.BindConfiguration( _configuration.GetSection( "Smtp" ) );
                optionBuilder.ConfigureMailKit( options );
            } );
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints( endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            } );
        }
    }
}
