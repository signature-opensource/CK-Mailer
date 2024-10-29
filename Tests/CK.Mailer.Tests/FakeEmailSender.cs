using CK.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer.Tests;

sealed class FakeEmailSender : IEmailSender
{
    internal FakeEmailSender( string endpoint, string token )
    {
        Endpoint = endpoint;
        Token = token;
    }

    public string Endpoint { get; }

    public string Token { get; }

    public static List<SimpleEmail> SentEmails { get; } = [];

    public Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
    {
        SentEmails.Add( email );
        return Task.FromResult( new SendResponse() );
    }
}
