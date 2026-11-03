using CK.AppIdentity.Cris;
using CK.Core;
using CK.Cris;
using CK.IO.Mailer;
using CK.Mailer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CK.AppIdentity.Mailer;

// The "MailSender" email sender: sends the mails to the MailSender of the root party (its RemoteMailSender feature).
sealed class RemoteEmailSender : IEmailSender
{
    // The maximal time to wait for the MailSender's answer.
    static readonly TimeSpan _timeout = TimeSpan.FromSeconds( 30 );

    readonly ApplicationIdentityService _identity;
    readonly PocoDirectory _pocos;

    internal RemoteEmailSender( ApplicationIdentityService identity, PocoDirectory pocos )
    {
        _identity = identity;
        _pocos = pocos;
    }

    public async Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
    {
        var response = new SendResponse();
        var mailSender = _identity.GetFeature<RemoteMailSender>()?.MailSender;
        if( mailSender == null )
        {
            response.ErrorMessages.Add( $"'{_identity}' has no MailSender: it must allow the 'MailSenderRemote' feature and one of its remotes 'MailSender'." );
            monitor.Error( response.ErrorMessages[0] );
            return response;
        }
        var channel = mailSender.GetFeature<CrisChannelFeature>();
        if( channel == null )
        {
            response.ErrorMessages.Add( $"The MailSender '{mailSender}' has no Cris channel." );
            monitor.Error( response.ErrorMessages[0] );
            return response;
        }
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
            response.ErrorMessages.Add( $"Unable to send the mail to the MailSender '{mailSender}': {ex.Message}" );
            monitor.Error( response.ErrorMessages[^1], ex );
        }
        return response;
    }

    // The From is not sent: the MailSender always uses its own address.
    ISendMailCommand ToCommand( SimpleEmail email )
    {
        return _pocos.Create<ISendMailCommand>( c =>
        {
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
