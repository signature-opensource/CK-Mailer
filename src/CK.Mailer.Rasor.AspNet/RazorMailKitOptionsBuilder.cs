using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CK.Mailer;
using Microsoft.Extensions.Configuration;
using RazorLight;

namespace CK.Mailer.Razor
{
    public class RazorMailKitOptionsBuilder
    {
        /// <summary>
        /// Gets the service collection in which the services are added.
        /// </summary>
        public IServiceCollection serviceCollection { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> service collection</param>
        public RazorMailKitOptionsBuilder( IServiceCollection services )
        {
            this.serviceCollection = services;
        }

        public RazorMailKitOptions BindConfiguration( IConfigurationSection section )
        {
            RazorMailKitOptions options = new RazorMailKitOptions();

            section.Bind( options );

            return options;
        }

        /// <summary>
        ///  add email service to di
        /// </summary>
        /// <param name="options"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public RazorMailKitOptionsBuilder ConfigureMailKit( RazorMailKitOptions options, ServiceLifetime lifetime = ServiceLifetime.Scoped )
        {
            MailKitClientProvider provider = new MailKitClientProvider( options );
            IRazorLightEngine engine;
            if( options.UseEmbeddedResources )
            {
                 engine = EngineFactory.CreateEmbedded( options.RootTypeInsideResourceAssembly );
            }
            else
            {
                engine = EngineFactory.CreatePhysical( options.ViewsPhysicalPath );
            }

            serviceCollection.TryAddSingleton<IRazorLightEngine>( engine );
            serviceCollection.TryAddSingleton<IMailKitClientProvider>( provider );

            serviceCollection.TryAdd( new ServiceDescriptor( typeof( IMailerService ), typeof( RazorMailerService ), lifetime ) );
            serviceCollection.TryAdd( new ServiceDescriptor( typeof( IRazorMailerService ), typeof( RazorMailerService ), lifetime ) );

            return this;
        }
    }
}
