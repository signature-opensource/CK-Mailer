using CK.Mailer.MailKit;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Mailer.Tests;

[TestFixture]
public class MailKitSenderTests
{
    [Test]
    public async Task Do_nothing_Async()
    {
        var sender = new MailKitSender( new MailKitSenderOptions { SendEmail = false } );
        var response = await sender.SendAsync( TestHelper.Monitor, new SimpleEmail().From( "" ) );
        response.Successful.Should().BeTrue();
        response.MessageId.Should().BeNull();
    }
}
