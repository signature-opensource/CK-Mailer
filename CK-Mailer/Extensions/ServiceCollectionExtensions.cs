using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddMailKit( this IServiceCollection serviceCollection, Action<MailKitOptionsBuilder> getOptions )
        {
            if( serviceCollection == null ) throw new ArgumentNullException( nameof( serviceCollection ) );
            if( getOptions == null ) throw new ArgumentNullException( nameof( getOptions ) );

            getOptions.Invoke( new MailKitOptionsBuilder( serviceCollection ) );

            return serviceCollection;
        }
    }
}