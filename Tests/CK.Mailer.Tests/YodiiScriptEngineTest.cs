using CK.Core;
using CK.Mailer.YodiiScript;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yodii.Script;
using static CK.Testing.BasicTestHelper;

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
            Assume.That( TestHelper.IsExplicitAllowed, "Press Ctrl key to allow this test to run." );

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
            Assume.That( TestHelper.IsExplicitAllowed, "Press Ctrl key to allow this test to run." );
            ActivityMonitor m = new ActivityMonitor( "StaticMailerServiceTests.SandBox_Email_sender" );

            string defaultRecipient = TestHelperConfiguration.Default.Get( "Smtp/DefaultRecipient" );
            defaultRecipient.Should().NotBeNullOrWhiteSpace();
            string password = TestHelperConfiguration.Default.Get( "Smtp/Password" );
            password.Should().NotBeNullOrWhiteSpace();

            YodiiScriptMimeMessage mailModel = new YodiiScriptMimeMessage( defaultRecipient );
            mailModel.Subject = "Coucou Benjamin";

            var options = new MailKitOptions()
            {
                Host = TestHelperConfiguration.Default.Get( "Smtp/Host" ),
                Port = TestHelperConfiguration.Default.GetInt32( "Smtp/Port" ).GetValueOrDefault( 587 ),
                UsePickupDirectory = true,
                PickupDirectoryPath = "./PickupDirectory",
                Password = password,
                SendMails = true,
                User = TestHelperConfiguration.Default.Get( "Smtp/User" ),
                UseSSL = false,
                DefaultSenderEmail = TestHelperConfiguration.Default.Get( "Smtp/DefaultSenderEmail" )
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
