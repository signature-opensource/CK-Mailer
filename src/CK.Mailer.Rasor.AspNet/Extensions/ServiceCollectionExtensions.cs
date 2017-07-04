using CK.Mailer.Razor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.Razor
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddMailKitRazor( this IServiceCollection serviceCollection, Action<RazorMailKitOptionsBuilder> getOptions )
        {
            if( serviceCollection == null ) throw new ArgumentNullException( nameof( serviceCollection ) );
            if( getOptions == null ) throw new ArgumentNullException( nameof( getOptions ) );

            getOptions.Invoke( new RazorMailKitOptionsBuilder( serviceCollection ) );

            return serviceCollection;
        }
    }
}