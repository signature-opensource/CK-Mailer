# CK.IO.Mailer

The command that a remote party sends to its "MailSender" party, so that only the MailSender holds the mail
configuration (the SMTP server, its credentials, the `From` address). It is used by the two sides of the "MailSender"
feature: [CK.AppIdentity.Mailer](../CK.AppIdentity.Mailer/README.md) (the MailSender) and
[CK.AppIdentity.Mailer.Remote](../CK.AppIdentity.Mailer.Remote/README.md) (its remotes).

| Type | |
|---|---|
| [`ISendMailCommand`](ISendMailCommand.cs) | `[Internal]`, executed by the endpoint that receives it (`ICommandEndpointOnly`): the rendered mail (`To`, `Cc`, `Bcc`, `ReplyTo`, `Subject`, `HtmlBody`, `PlaintextBody`, `Priority`, `Headers`, `Attachments`). It has no `From`: the MailSender sends with its own address. |
| [`ISendMailResult`](ISendMailResult.cs) | The `MessageId` and the `ErrorMessages` (empty on success). |
| [`IMailAddress`](IMailAddress.cs) | An `Email` and its optional `Name`. |
| [`IMailAttachment`](IMailAttachment.cs) | `FileName`, `ContentType`, `ContentId`, `IsInline` and the `Data` bytes. |
| [`MailPriority`](MailPriority.cs) | `High`, `Normal` (the default), `Low`. |

The party that builds a mail renders it (its templates, its culture, its branding): the MailSender knows nothing about
what it sends.
