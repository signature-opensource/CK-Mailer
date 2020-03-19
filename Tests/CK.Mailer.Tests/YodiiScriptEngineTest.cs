using CK.Core;
using CK.Mailer.YodiiScript;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yodii.Script;

namespace CK.Mailer.Tests
{
    public class ModelWithMethod
    {
        public string TheVariable { get; set; }

        public string Reverse( string variable )
        {
            return String.Join( "", variable.Reverse() );
        }
    }

    [TestFixture]
    public class YodiiScriptEngineTest
    {
        [Test]
        [Explicit]
        public void YodiiScriptEngine_SandBox()
        {
            var model = new ModelWithMethod()
            {
                TheVariable = "azerty"
            };

            var c = new GlobalContext();
            c.Register( "Model", model );

            var e = new TemplateEngine( c );
            var result = e.Process( "Hello, <%= Model.Reverse( Model.TheVariable ) %>" );

            //remove white space and line break
            string content = Regex.Replace( result.Text, @"\s+", string.Empty );

            content.Should().Be( $"Hello,{model.Reverse( model.TheVariable )}" );
        }


        [Test]
        public void YodiiScript_simple_call_model_method_with_model_variable()
        {
            var model = new ModelWithMethod()
            {
                TheVariable = "azerty"
            };

            var c = new GlobalContext();
            c.Register( "Model", model );

            var e = new TemplateEngine( c );
            var result = e.Process( "Hello, <%= Model.Reverse( Model.TheVariable ) %>" );

            result.Text.Should().Be( $"Hello, {model.Reverse( model.TheVariable )}" );
        }

        [Test]
        public void YodiiScript_simple_call_model_method_with_temp_variable()
        {
            var model = new ModelWithMethod()
            {
                TheVariable = "azerty"
            };

            var c = new GlobalContext();
            c.Register( "Model", model );

            var e = new TemplateEngine( c );
            var result = e.Process( "Hello, <% let v = Model.TheVariable; %> <%= Model.Reverse( v ) %>" );

            //remove white space and line break
            string content = Regex.Replace( result.Text, @"\s+", string.Empty );

            content.Should().Be( $"Hello,{model.Reverse( model.TheVariable )}" );
        }
        
        [Test]
        [Explicit]
        public async Task YodiiScript_SandBox_Email_sender()
        {
            ActivityMonitor m = new ActivityMonitor( "StaticMailerServiceTests.SandBox_Email_sender" );

            YodiiScriptMimeMessage mailModel = new YodiiScriptMimeMessage( "franck.bontemps@invenietis.com" );
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
                Spams = new[]
                {
                    "normal",
                    "<b> bold </b>",
                    "<!-- commentaire -->",
                    "normal"
                }
            };

            mailModel.SetBodyFromYodiiScriptString( m, model, "<b>Hello</b>, <%= Model.Truc %> <% foreach( s in Model.Spams ) { %> <%= s %> <% } %> normal in content <!-- comment in content -->" );

            await StaticMailerService.SendMailAsync( m, options, mailModel );
        }
    }
}
