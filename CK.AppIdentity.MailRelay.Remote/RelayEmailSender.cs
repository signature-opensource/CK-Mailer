using CK.AppIdentity.Cris;
using CK.Core;
using CK.Cris;
using CK.IO.MailRelay;
using CK.Mailer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CK.AppIdentity.MailRelay;

// A relay sender: sends the mails to a remote that is a "MailRelay" for its local party (it declares "MailRelayRemote").
// Named after the remote ("$CorpRelay") in the MailSenders of its local party.
sealed class RelayEmailSender : IEmailSender
{
    // The maximal time to wait for the relay's answer.
    static readonly TimeSpan _timeout = TimeSpan.FromSeconds( 30 );

    readonly IRemoteParty _relay;
    readonly PocoDirectory _pocos;
    readonly string? _senderName;

    internal RelayEmailSender( IRemoteParty relay, PocoDirectory pocos, string? senderName )
    {
        _relay = relay;
        _pocos = pocos;
        _senderName = senderName;
    }

    public async Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
    {
        var response = new SendResponse();
        var channel = _relay.GetFeature<CrisChannelFeature>();
        if( channel == null )
        {
            response.ErrorMessages.Add( $"The MailRelay '{_relay}' has no Cris channel." );
            monitor.Error( response.ErrorMessages[0] );
            return response;
        }
        var mailSender = _relay;
        try
        {
            var result = await channel.SendCommand( monitor, ToCommand( email ) )
                                      .WithResult<ISendMailResult>().Result
                                      .WaitAsync( _timeout, token ).ConfigureAwait( false );
            response.MessageId = result.MessageId;
            foreach( var e in result.ErrorMessages ) response.ErrorMessages.Add( e );
        }
        catch( Exception ex )
        {
            response.ErrorMessages.Add( $"Unable to send the mail to the MailRelay '{mailSender}': {ex.Message}" );
            monitor.Error( response.ErrorMessages[^1], ex );
        }
        return response;
    }

    // The From is not sent: the relay uses the address of its sender.
    ISendMailCommand ToCommand( SimpleEmail email )
    {
        return _pocos.Create<ISendMailCommand>( c =>
        {
            c.Sender = _senderName;
            c.Subject = email.Subject;
            c.HtmlBody = email.HtmlBody;
            c.PlaintextBody = email.PlaintextAlternativeBody;
            c.Priority = (MailPriority)email.Priority;
            foreach( var a in email.ToAddresses ) c.To.Add( ToAddress( a ) );
            foreach( var a in email.CcAddresses ) c.Cc.Add( ToAddress( a ) );
            foreach( var a in email.BccAddresses ) c.Bcc.Add( ToAddress( a ) );
            foreach( var a in email.ReplyToAddresses ) c.ReplyTo.Add( ToAddress( a ) );
            foreach( var (k, v) in email.Headers ) c.Headers.Add( k, v );
            foreach( var a in email.Attachments )
            {
                using var data = new MemoryStream();
                a.Data.CopyTo( data );
                c.Attachments.Add( _pocos.Create<IMailAttachment>( m =>
                {
                    m.FileName = a.Filename;
                    m.ContentType = a.ContentType;
                    m.ContentId = a.ContentId;
                    m.IsInline = a.IsInline;
                    m.Data = data.ToArray();
                } ) );
            }
        } );
    }

    IMailAddress ToAddress( Address a ) => _pocos.Create<IMailAddress>( m =>
    {
        m.Email = a.Email;
        m.Name = a.Name;
    } );
}
