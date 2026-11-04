# CK-Mailer

This solution contains:
- [CK.Mailer](./CK.Mailer/README.md): The base project to use mailer with [CK-AppIdentity](https://github.com/signature-opensource/CK-AppIdentity).
- [CK.Mailer.MailKit](./CK.Mailer.MailKit/README.md): An implementation for CK-Mailer with [Mailkit](https://github.com/jstedfast/MailKit).
- [CK.IO.MailSender](./CK.IO.MailSender/README.md): The `[Internal]` command that a remote party sends to its "MailSender" party.
- [CK.AppIdentity.MailSender.Local](./CK.AppIdentity.MailSender.Local/README.md): The "MailSender" asymmetric feature, Local side: a local party sends the mails of its remote parties.
- [CK.AppIdentity.MailSender.Remote](./CK.AppIdentity.MailSender.Remote/README.md): The "MailSender" asymmetric feature, Remote side: a local party sends its mails through its MailSender party.
