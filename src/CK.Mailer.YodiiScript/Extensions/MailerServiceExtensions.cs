using CK.Core;
using CK.Mailer.Razor;
using CK.Mailer.YodiiScript;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public static class MailerServiceExtensions
    {
        public static void SendInlineTemplate<T>(
            this IYodiiScriptMailerService @this,
            IActivityMonitor m,
            YodiiScriptMimeMessage message,
            string content,
            T model )
        {
            var result = message.SetBodyFromYodiiScriptString( m, model, content );

            if( !String.IsNullOrEmpty( result.ErrorMessage ) )
            {
                m.Error().Send( result.ErrorMessage );
            }

            m.Debug().Send( result.Script );

            @this.Send( m, message );
        }

        public static void Send<T>(
            this IYodiiScriptMailerService @this,
            IActivityMonitor m,
            YodiiScriptMimeMessage message,
            string templatePath,
            T model )
        {
            if( !String.IsNullOrEmpty( @this.ViewsPhysicalPath ) && !Path.IsPathRooted( templatePath ) )
            {
                m.Info().Send( "YodiiScript combine template path with ViewsPhysicalPath option" );

                templatePath = Path.Combine( @this.ViewsPhysicalPath, templatePath );

                m.Info().Send( "Template path result : {0}", templatePath );
            }

            if( !File.Exists( templatePath ) )
            {
                m.Error().Send( "YodiiScript template not found : {0}", templatePath );
                throw new FileNotFoundException();
            }
            
            var content = File.ReadAllText( templatePath );

            message.SetBodyFromYodiiScriptString( m, model, content );
            
            @this.Send( m, message );
        }
    }
}
