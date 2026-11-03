# CK.AppIdentity.Mailer.Remote

The Remote side of the "MailSender" feature: this application sends its mails through its MailSender party, without
any mail configuration of its own (no SMTP server, no credentials). The code that sends mails doesn't change: it uses
the `IDefaultEmailSender` of [CK.Mailer](../CK.Mailer/README.md), whose "MailSender" sender sends the rendered mail to
the MailSender (see [CK.AppIdentity.Mailer](../CK.AppIdentity.Mailer/README.md)).

## Configuration

The application's party allows "MailSenderRemote" and its "EmailSender" has the "MailSender" sender; the MailSender is
the remote that allows "MailSender":

```json
"CK-AppIdentity": {
  "FullName": "Signature/$Front",
  "Local": {
    "AllowFeatures": "MailSenderRemote",
    "EmailSender": { "MailSender": true }
  },
  "Parties": [
    { "PartyName": "$Backend", "Address": "...", "AllowFeatures": "MailSender" }
  ]
}
```

None, or more than one, remote that allows "MailSender" is a start-up error, and a remote added later cannot be the
MailSender. The MailSender must allow this party ("MailSenderRemote" on its side): otherwise it refuses the mails.

## How it works

| Type | |
|---|---|
| [`RemoteMailSenderFeatureDriver`](RemoteMailSenderFeatureDriver.cs) | The driver of the "MailSender" feature on this side: it gives a local party that allows "MailSenderRemote" its [`RemoteMailSender`](RemoteMailSender.cs) feature (the `RemoteMailSender.MailSender` remote party). |
| [`MailSenderEmailSenderFactory`](MailSenderEmailSenderFactory.cs) | The CK.Mailer factory of the "MailSender" sender: its sender sends the mails of the application's party as an `ISendMailCommand` of [CK.IO.Mailer](../CK.IO.Mailer/README.md) on the Cris channel to the MailSender, and waits for its `ISendMailResult` (30 seconds at most). |

The `From` of a mail is not sent: the MailSender always uses its own address. The attachments travel in the command
(their content is read when the mail is sent).

An application has one role: it references this package (it is a remote of a MailSender) or CK.AppIdentity.Mailer (it
is a MailSender), never both.
