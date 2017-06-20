using CK.Core;
using MimeKit;
using MimeKit.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer.Tests
{
    [TestFixture]
    public class StaticMailerServiceTests
    {
        [Test]
        public async Task SimpleSendMail()
        {
            ActivityMonitor m = new ActivityMonitor( "SimpleSendMail.SimpleSendMail" );

            MimeMessage message = new MimeMessage();

            message.From.Add( new MailboxAddress( "SimpleSendMail@StaticMailerServiceTests.fr" ) );
            message.To.Add( new MailboxAddress( "SimpleSendMail@StaticMailerServiceTests.fr" ) );

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "StaticMailerServiceTests.SimpleSendMail";
            message.Body = bodyBuilder.ToMessageBody();

            var body = new TextPart( TextFormat.Html );
            body.SetText( Encoding.UTF8, "StaticMailerServiceTests.SimpleSendMail" );
            message.Body = body;

            await StaticMailerService.SendMailAsync( m, message, DefaultMailKitOptions.Default );
        }

        [Test]
        public async Task SpamBenjamin()
        {
            ActivityMonitor m = new ActivityMonitor( "StaticMailerServiceTests.SimpleSendMail" );

            MimeMessage message = new MimeMessage();

            message.From.Add( new MailboxAddress( "test@invenietis.com" ) );
            message.To.Add( new MailboxAddress( "benjamin.crosnier@invenietis.com" ) );

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "StaticMailerServiceTests.SpamBenjamin";
            message.Body = bodyBuilder.ToMessageBody();

            var body = new TextPart( TextFormat.Html );
            body.SetText( Encoding.UTF8, "StaticMailerServiceTests.SpamBenjamin" );
            message.Body = body;

            var options = new MailKitOptions()
            {
                Host = "app-smtp.invenietis.net",
                Port = 587,
                UsePickupDirectory = true,
                PickupDirectoryPath = "./PickupDirectory",
                Password = "1C59vMW17530o1bfs56l",
                SendMails = false,
                User = "invback@invenietis.net",
                UseSSL = false
            };

            await StaticMailerService.SendMailAsync( m, message, options );
        }

        [Test]
        public async Task SpamIdrissWithBasicMailModel()
        {
            ActivityMonitor m = new ActivityMonitor( "StaticMailerServiceTests.SpamIdrissWithBasicMailModel" );

            BasicMailModel mailModel = new BasicMailModel( "franck.bontemps@invenietis.com", "idriss.hippocrate@invenietis.com", "Coucou Idriss", "Je suis là pour te spam" );
            
            var options = new MailKitOptions()
            {
                Host = "app-smtp.invenietis.net",
                Port = 587,
                UsePickupDirectory = true,
                PickupDirectoryPath = "./PickupDirectory",
                Password = "1C59vMW17530o1bfs56l",
                SendMails = false,
                User = "invback@invenietis.net",
                UseSSL = false
            };

            await StaticMailerService.SendMailAsync( m, mailModel, options );
            StaticMailerService.SendMail( m, mailModel, options );
            await StaticMailerService.SendMailAsync( m, mailModel.ToMimeMessage(), options );
            StaticMailerService.SendMail( m, mailModel.ToMimeMessage(), options );

            var provider = new MailKitClientProvider( options );

            await StaticMailerService.SendMailAsync( m, mailModel, provider );
            StaticMailerService.SendMail( m, mailModel, provider );
            await StaticMailerService.SendMailAsync( m, mailModel.ToMimeMessage(), provider );
            StaticMailerService.SendMail( m, mailModel.ToMimeMessage(), provider );
        }
    }
}
