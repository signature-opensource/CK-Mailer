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
        
    }
}
