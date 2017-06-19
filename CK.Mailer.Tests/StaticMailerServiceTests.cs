using CK.Core;
using MimeKit;
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
            bodyBuilder.TextBody = "SimpleSendMail.SimpleSendMail";
            message.Body = bodyBuilder.ToMessageBody();

            await StaticMailerService.SendMailAsync( m, message, DefaultMailKitOptions.Default );
        }
    }
}
