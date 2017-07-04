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
        public static Task SendInlineTemplateAsync<T>( this IRazorMailerService @this
            , IActivityMonitor m
            , string from
            , string to
            , string subject
            , string content
            , T model )
        {
            var mailModel = new RazorMailModel<T>( model );
            mailModel.Recipients.To.Add( to );
            mailModel.Recipients.From.Add( from );
            mailModel.Subject = subject;

            return @this.SendAsync( m, mailModel.ProcessRazorString( @this.RazorEngine, content ) );
        }

        public static void SendInlineTemplate<T>( this IRazorMailerService @this
            , IActivityMonitor m
            , string from
            , string to
            , string subject
            , string content
            , T model )
        {
            var mailModel = new RazorMailModel<T>( model );
            mailModel.Recipients.To.Add( to );
            mailModel.Recipients.From.Add( from );
            mailModel.Subject = subject;

            @this.Send( m, mailModel.ProcessRazorString( @this.RazorEngine, content ) );
        }

        public static Task SendInlineTemplateAsync<T>( this IRazorMailerService @this
            , IActivityMonitor m
            , string to
            , string subject
            , string content
            , T model )
        {
            var mailModel = new RazorMailModel<T>( model );
            mailModel.Recipients.To.Add( to );
            mailModel.Subject = subject;

            return @this.SendAsync( m, mailModel.ProcessRazorString( @this.RazorEngine, content ) );
        }

        public static void SendInlineTemplate<T>( this IRazorMailerService @this
            , IActivityMonitor m
            , string to
            , string subject
            , string content
            , T model )
        {
            var mailModel = new RazorMailModel<T>( model );
            mailModel.Recipients.To.Add( to );
            mailModel.Subject = subject;

            @this.Send( m, mailModel.ProcessRazorString( @this.RazorEngine, content ) );
        }

        public static Task SendAsync<T>( this IRazorMailerService @this
            , IActivityMonitor m
            , string to
            , string subject
            , string template
            , T model )
        {
            var mailModel = new RazorMailModel<T>( model );
            mailModel.Recipients.To.Add( to );
            mailModel.Subject = subject;

            return @this.SendAsync( m, mailModel.ProcessRazorView( @this.RazorEngine, template ) );
        }

        public static void Send<T>( this IRazorMailerService @this
            , IActivityMonitor m
            , string to
            , string subject
            , string template
            , T model )
        {
            var mailModel = new RazorMailModel<T>( model );
            mailModel.Recipients.To.Add( to );
            mailModel.Subject = subject;

            @this.Send( m, mailModel.ProcessRazorView( @this.RazorEngine, template ) );
        }

        public static Task SendAsync<T>( this IRazorMailerService @this, IActivityMonitor m, string template, IRazorMailModel<T> mailModel )
        {
            return @this.SendAsync( m, mailModel.ProcessRazorView( @this.RazorEngine, template ) );
        }

        public static void Send<T>( this IRazorMailerService @this, IActivityMonitor m, string template, IRazorMailModel<T> mailModel )
        {
            @this.Send( m, mailModel.ProcessRazorView( @this.RazorEngine, template ) );
        }
        
        public static Task SendInlineTemplateAsync<T>( this IRazorMailerService @this, IActivityMonitor m, string content, IRazorMailModel<T> mailModel )
        {
            return @this.SendAsync( m, mailModel.ProcessRazorString( @this.RazorEngine, content ) );
        }

        public static void SendInlineTemplate<T>( this IRazorMailerService @this, IActivityMonitor m, string content, IRazorMailModel<T> mailModel )
        {
            @this.Send( m, mailModel.ProcessRazorString( @this.RazorEngine, content ) );
        }
    }
}
