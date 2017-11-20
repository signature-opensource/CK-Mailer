using CK.Core;
using CK.Mailer.Razor;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Script;

namespace CK.Mailer.Tests
{
    [TestFixture]
    public class YodiiScriptEngineTest
    {
        [Test]
        public void YodiiScriptEngine_SandBox()
        {
            var model = new
            {
                Truc = "fezrzerze"
            };

            var c = new GlobalContext();
            c.Register( "Model", model );

            var e = new TemplateEngine( c );
            var result = e.Process( "Hello, <%= Model.Truc %>" );

            result.Text.Should().Be( $"Hello, {model.Truc}" );
        }


        [Test]
        [Explicit]
        public async Task YodiiScript_SandBox_Email_sender()
        {
            ActivityMonitor m = new ActivityMonitor( "StaticMailerServiceTests.SandBox_Email_sender" );

            YodiiScriptMimeMessage mailModel = new YodiiScriptMimeMessage( "benjamin.crosnier@invenietis.com" );
            mailModel.Subject = "Coucou Benjamin";

            var options = new MailKitOptions()
            {
                Host = "app-smtp.invenietis.net",
                Port = 587,
                UsePickupDirectory = true,
                PickupDirectoryPath = "./PickupDirectory",
                Password = "1C59vMW17530o1bfs56l",
                SendMails = true,
                User = "invback@invenietis.net",
                UseSSL = false,
                DefaultSenderEmail = "no-reply@ttge.fr"
            };

            var model = new
            {
                Truc = "benjamin",
                Spams = new []
                {
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dqidjqzk",
                    "dsdoies"
                }
            };

            mailModel.SetBodyFromYodiiScriptString( m, model, "Hello, <%= Model.Truc %> <% foreach( s in Model.Spams ) { %> <%= s %> <% } %>" );

            await StaticMailerService.SendMailAsync( m, options, mailModel );
        }
    }
}
