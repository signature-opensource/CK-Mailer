using CK.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer;

public class EmailSenderFeature : IEmailSender
{
    readonly IEnumerable<IEmailSender> _senders;

    internal EmailSenderFeature( IEnumerable<IEmailSender> senders )
    {
        _senders = senders;
    }

    public async Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
    {
        var response = new SendResponse();

        foreach( var sender in _senders )
        {
            var r = await sender.SendAsync( monitor, email, token );

            if( r.MessageId is not null )
            {
                response.MessageId = response.MessageId is not null
                    ? $"{response.MessageId},{r.MessageId}"
                    : r.MessageId;
            }
            response.ErrorMessages.AddRange( r.ErrorMessages );
        }
        return response;
    }
}
