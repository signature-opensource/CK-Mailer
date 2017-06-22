using System;
using NUnit.Framework;
using RazorLight;
using RazorLight.Extensions;
using FluentAssertions;

namespace CK.Mailer.Tests
{
    [TestFixture]
    public class RazorLightTests
    {
        [Test]
        [Explicit]
        public void RazorLight_Sample()
        {
            var engine = EngineFactory.CreatePhysical(@"C:\");

            string content = "Hello @Model.Name. Welcome to @Model.Title repository";

            var model = new
            {
                Name = "John Doe",
                Title = "RazorLight"
            };

            string result = engine.ParseString( content, model ); //Output: Hello John Doe. Welcome to RazorLight repository

            result.Should().BeEquivalentTo( "Hello John Doe. Welcome to RazorLight repository" );
        }
    }
}
