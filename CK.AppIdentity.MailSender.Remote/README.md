# CK.AppIdentity.MailSender.Remote

The Remote side of the "MailSender" [asymmetric feature](https://github.com/signature-opensource/CK-AppIdentity/blob/stable/CK.AppIdentity/Features/README.md#asymmetric-features):
a local party that doesn't allow "MailSender" sends its mails through its MailSender party, without any mail
configuration of its own (no SMTP server, no credentials). The code that sends mails doesn't change: it uses the
`IDefaultEmailSender` of [CK.Mailer](../CK.Mailer/README.md), whose "MailSender" sender sends the rendered mail to
the MailSender (see [CK.AppIdentity.MailSender.Local](../CK.AppIdentity.MailSender.Local/README.md)).

## Configuration

The "EmailSender" of the application's party has the "MailSender" sender; the MailSender is the remote that allows
"MailSenderRemote" (the `"Local"` doesn't allow "MailSender"):

```json
"CK-AppIdentity": {
  "FullName": "Signature/$Front",
  "Local": {
    "EmailSender": { "MailSender": true }
  },
  "Parties": [
    { "PartyName": "$Backend", "Address": "...", "AllowFeatures": "MailSenderRemote" }
  ]
}
```

More than one remote that allows "MailSenderRemote" is a start-up error; a remote added later can be the MailSender. The
MailSender must allow this party ("MailSenderRemote" on its side): otherwise it refuses the mails. It must announce
being a MailSender: a `$Backend` whose `"Local"` doesn't allow "MailSender" is an error when it announces its
description.

## How it works

| Type | |
|---|---|
| [`RemoteMailSenderFeatureDriver`](RemoteMailSenderFeatureDriver.cs) | The driver of the "MailSender" feature on this side: it gives a local party that doesn't allow "MailSender" and has a remote that allows "MailSenderRemote" its [`RemoteMailSender`](RemoteMailSender.cs) feature (the `RemoteMailSender.MailSender` remote party). |
| [`MailSenderEmailSenderFactory`](MailSenderEmailSenderFactory.cs) | The CK.Mailer factory of the "MailSender" sender: its sender sends the mails of the application's party as an `ISendMailCommand` of [CK.IO.MailSender](../CK.IO.MailSender/README.md) on the Cris channel to the MailSender, and waits for its `ISendMailResult` (30 seconds at most). |

The `From` of a mail is not sent: the MailSender always uses its own address. The attachments travel in the command
(their content is read when the mail is sent).

A local party that allows "MailSender" is ignored: CK.AppIdentity.MailSender.Local handles it, and the two packages can be
referenced by the same application.
