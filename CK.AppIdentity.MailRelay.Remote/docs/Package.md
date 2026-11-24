The Remote side of the CK-AppIdentity "MailRelay" feature: each remote that declares "MailRelayRemote" is a sender of
its local party, which sends mails through its relays without any mail configuration of its own.

`RemoteMailRelayFeatureDriver` adds these senders to the CK.Mailer "MailSender" feature, each one named after its remote.

See the [README](https://github.com/signature-opensource/CK-Mailer/blob/stable/CK.AppIdentity.MailRelay.Remote/README.md).
