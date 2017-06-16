using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CK.Mailer;
using Microsoft.Extensions.Configuration;

namespace CK.Mailer
{
    public class MailKitOptionsBuilder : IMailKitOptionsBuilder
    {
        /// <summary>
        /// Gets the service collection in which the interception based services are added.
        /// </summary>
        public IServiceCollection serviceCollection { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> service collection</param>
        public MailKitOptionsBuilder(IServiceCollection services)
        {
            this.serviceCollection = services;
        }

        //TODO move this method as extension method in another package
        public MailKitOptions BindConfiguration( IConfigurationSection section )
        {
            MailKitOptions options = new MailKitOptions();

            section.Bind( options );

            return options;
        }

        /// <summary>
        ///  add email service to di
        /// </summary>
        /// <param name="options"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public IMailKitOptionsBuilder ConfigureMailKit(MailKitOptions options, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            AddProviderService(options);
            serviceCollection.TryAdd(new ServiceDescriptor(typeof(IMailerService), typeof(MailerService), lifetime));
            return this;
        }

        private void AddProviderService(MailKitOptions options)
        {
            MailKitClientProvider provider = new MailKitClientProvider(options);
            serviceCollection.TryAddSingleton<IMailKitClientProvider>(provider);
        }
    }
}
