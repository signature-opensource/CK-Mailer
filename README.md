# CK-Mailer

This solution contains:
- [CK.Mailer](./CK.Mailer/README.md): The base project to use mailer with [CK-AppIdentity](https://github.com/signature-opensource/CK-AppIdentity).
- [CK.Mailer.MailKit](./CK.Mailer.MailKit/README.md): An implementation for CK-Mailer with [Mailkit](https://github.com/jstedfast/MailKit).
- [CK.IO.Mailer](./CK.IO.Mailer/README.md): The `[Internal]` command that a remote party sends to its "MailSender" party.
- [CK.AppIdentity.Mailer](./CK.AppIdentity.Mailer/README.md): The "MailSender" feature, Local side: this application sends the mails of its remote parties.
- [CK.AppIdentity.Mailer.Remote](./CK.AppIdentity.Mailer.Remote/README.md): The "MailSender" feature, Remote side: this application sends its mails through its MailSender party.
