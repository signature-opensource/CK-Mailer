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
            this IRazorMailerService @this,
            IActivityMonitor m,
            string from,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new RazorMimeMessage( from, to, subject );

            message.SetRazorBodyFromString( @this.RazorEngine, model, content );
            
            return @this.SendAsync( m, message );
        }

        public static void SendInlineTemplate<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            string from,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new RazorMimeMessage( from, to, subject );

            message.SetRazorBodyFromString( @this.RazorEngine, model, content );

            @this.Send( m, message );
        }

        public static Task SendInlineTemplateAsync<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new RazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBodyFromString( @this.RazorEngine, model, content );

            return @this.SendAsync( m, message );
        }

        public static void SendInlineTemplate<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string content,
            T model )
        {
            var message = new RazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBodyFromString( @this.RazorEngine, model, content );

            @this.Send( m, message );
        }

        public static Task SendAsync<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string template,
            T model )
        {
            var message = new RazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBody( @this.RazorEngine, model, template );

            return @this.SendAsync( m, message );
        }

        public static void Send<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            string to,
            string subject,
            string template,
            T model )
        {
            var message = new RazorMimeMessage( to );
            message.Subject = subject;

            message.SetRazorBody( @this.RazorEngine, model, template );

            @this.Send( m, message );
        }

        public static Task SendAsync<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            RazorMimeMessage message,
            string template,
            T model )
        {
            message.SetRazorBody( @this.RazorEngine, model, template );

            return @this.SendAsync( m, message );
        }

        public static void Send<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            RazorMimeMessage message,
            string template,
            T model )
        {
            message.SetRazorBody( @this.RazorEngine, model, template );

            @this.Send( m, message );
        }
        
        public static Task SendInlineTemplateAsync<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            RazorMimeMessage message,
            string content,
            T model )
        {
            message.SetRazorBodyFromString( @this.RazorEngine, model, content );

            return @this.SendAsync( m, message );
        }

        public static void SendInlineTemplate<T>(
            this IRazorMailerService @this,
            IActivityMonitor m,
            RazorMimeMessage message,
            string content,
            T model )
        {
            message.SetRazorBodyFromString( @this.RazorEngine, model, content );

            @this.Send( m, message );
        }
    }
}
