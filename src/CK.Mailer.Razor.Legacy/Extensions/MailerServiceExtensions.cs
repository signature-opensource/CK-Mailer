using CK.Core;
using CK.Mailer.Razor;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public static class MailerServiceExtensions
    {
        public static Task SendInlineTemplateAsync<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            string from,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new LegacyRazorMimeMessage( from, to, subject );

            message.SetRazorBodyFromString( model, content );
            
            return @this.SendAsync( m, message );
        }

        public static void SendInlineTemplate<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            string from,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new LegacyRazorMimeMessage( from, to, subject );

            message.SetRazorBodyFromString( model, content );

            @this.Send( m, message );
        }

        public static Task SendInlineTemplateAsync<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new LegacyRazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBodyFromString( model, content );

            return @this.SendAsync( m, message );
        }

        public static void SendInlineTemplate<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new LegacyRazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBodyFromString( model, content );

            @this.Send( m, message );
        }

        public static Task SendAsync<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string template,
            T model )
        {
            var message = new LegacyRazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBody( @this.PathProvider, model, template );

            return @this.SendAsync( m, message );
        }

        public static void Send<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string template,
            T model )
        {
            var message = new LegacyRazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBody( @this.PathProvider, model, template );

            @this.Send( m, message );
        }

        public static Task SendAsync<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            LegacyRazorMimeMessage message,
            string template,
            T model )
        {
            message.SetRazorBody( @this.PathProvider, model, template );

            return @this.SendAsync( m, message );
        }

        public static void Send<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            LegacyRazorMimeMessage message,
            string template,
            T model )
        {
            message.SetRazorBody( @this.PathProvider, model, template );

            @this.Send( m, message );
        }
        
        public static Task SendInlineTemplateAsync<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            LegacyRazorMimeMessage message,
            string content,
            T model )
        {
            message.SetRazorBodyFromString( model, content );

            return @this.SendAsync( m, message );
        }

        public static void SendInlineTemplate<T>(
            this ILegacyRazorMailerService @this,
            IActivityMonitor m,
            LegacyRazorMimeMessage message,
            string content,
            T model )
        {
            message.SetRazorBodyFromString( model, content );

            @this.Send( m, message );
        }
    }
}
