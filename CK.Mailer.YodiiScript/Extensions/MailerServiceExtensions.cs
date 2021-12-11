using CK.Core;
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
        public static void SendInlineTemplate<T>( this IYodiiScriptMailerService @this,
                                                  IActivityMonitor m,
                                                  YodiiScriptMimeMessage message,
                                                  string content,
                                                  T model,
                                                  bool escapeHtmlModelChars = true )
        {
            var result = message.SetBodyFromYodiiScriptString( m, model, content, escapeHtmlModelChars );

            if( !String.IsNullOrEmpty( result.ErrorMessage ) )
            {
                m.Error( result.ErrorMessage );
            }

            m.Debug( result.Script );

            @this.Send( m, message );
        }

        public static void Send<T>( this IYodiiScriptMailerService @this,
                                    IActivityMonitor m,
                                    YodiiScriptMimeMessage message,
                                    string templatePath,
                                    T model,
                                    bool escapeHtmlModelChars = true )
        {
            if( !String.IsNullOrEmpty( @this.ViewsPhysicalPath ) && !Path.IsPathRooted( templatePath ) )
            {
                m.Info( "YodiiScript combine template path with ViewsPhysicalPath option" );

                templatePath = Path.Combine( @this.ViewsPhysicalPath, templatePath );

                m.Info( $"Template path result : {templatePath}" );
            }

            if( !File.Exists( templatePath ) )
            {
                m.Error( $"YodiiScript template not found : {templatePath}" );
                throw new FileNotFoundException();
            }
            
            var content = File.ReadAllText( templatePath );

            message.SetBodyFromYodiiScriptString( m, model, content, escapeHtmlModelChars );
            
            @this.Send( m, message );
        }
    }
}
