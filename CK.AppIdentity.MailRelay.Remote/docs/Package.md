The Remote side of the CK-AppIdentity "MailRelay" feature: each remote that declares "MailRelayRemote" is a sender of its
local party (CK.Mailer "MailSender"), named after it: the local party sends mails through its relays without any mail
configuration of its own (`RemoteMailRelayFeatureDriver`). See
https://github.com/signature-opensource/CK-Mailer/blob/stable/CK.AppIdentity.MailRelay.Remote/README.md.
