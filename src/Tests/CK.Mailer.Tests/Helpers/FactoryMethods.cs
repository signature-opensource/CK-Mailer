using CK.Core;
using CK.Mailer.Razor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CK.Mailer.Tests
{
    public static class FM
    {
        private static int _i = 0;

        private static int get_new_i_value()
        {
            return Interlocked.Increment( ref _i );
        }

        private static string get_new_i_string_value()
        {
            return Interlocked.Increment( ref _i ).ToString();
        }

        private static string get_new_email_value()
        {
            return get_new_i_value().ToString() + "@ckmailer.org";
        }

        public static TemplateModel Create_TemplateModel_with_generated_value()
        {
            return new TemplateModel()
            {
                TheVariable = get_new_i_string_value()
        };
        }

        public static RazorMimeMessage Create_RazorMailModel_with_to_from_subject_body_generated_values()
        {
            var model = new RazorMimeMessage();

            model.To.Add( get_new_email_value() );
            model.From.Add( get_new_email_value() );

            model.Subject = get_new_i_string_value();
            
            return model;
        }

        public static SimpleMimeMessage Create_BasicMailModel_with_to_from_subject_body_generated_values()
        {
            var model = new SimpleMimeMessage();

            model.To.Add( get_new_email_value() );
            model.From.Add( get_new_email_value() );

            model.Subject = get_new_i_string_value();
            model.SetHtmlBody( get_new_i_string_value() );

            return model;
        }

        public static SimpleMimeMessage Create_BasicMailModel_without_from_mail_address()
        {
            var model = new SimpleMimeMessage();

            model.To.Add( get_new_email_value() );

            model.Subject = get_new_i_string_value();
            model.SetHtmlBody( get_new_i_string_value() );

            return model;
        }

        public static IActivityMonitor Get_ActivityMonitor( 
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 )
        {
            return new ActivityMonitor( $"{memberName}-{sourceFilePath}" );
        }
    }
}
