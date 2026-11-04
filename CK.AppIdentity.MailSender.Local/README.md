# CK.AppIdentity.MailSender.Local

The Local side of the "MailSender" [asymmetric feature](https://github.com/signature-opensource/CK-AppIdentity/blob/stable/CK.AppIdentity/Features/README.md#asymmetric-features):
a local party that allows "MailSender" sends the mails of its remote parties with its own mail configuration. Only the
MailSender holds the SMTP server, its credentials and the `From` address; its remotes send it their rendered mails (see [CK.AppIdentity.MailSender.Remote](../CK.AppIdentity.MailSender.Remote/README.md)).

An application that sends its own mails needs none of this: its "EmailSender" configuration (see
[CK.Mailer](../CK.Mailer/README.md)) is enough.

## Configuration

The application's party allows "MailSender" and has an "EmailSender" configuration; the remotes that may send their mails
through it allow "MailSenderRemote" (the feature is opt-in):

```json
"CK-AppIdentity": {
  "FullName": "Signature/$Backend",
  "Local": {
    "AllowFeatures": "MailSender",
    "EmailSender": { "Smtp": { "Host": "smtp.example.com", "User": "...", "Password": "...", "From": "no-reply@example.com" } }
  },
  "Parties": [
    { "PartyName": "$Front", "AllowFeatures": "MailSenderRemote" }
  ]
}
```

A party that allows "MailSender" without "EmailSender" configuration is a start-up error. The mails are sent with the
`IDefaultEmailSender` of the application: a tenant domain cannot be a MailSender.

## How it works

| Type | |
|---|---|
| [`LocalMailSenderFeatureDriver`](LocalMailSenderFeatureDriver.cs) | The driver of the "MailSender" feature on this side. `LocalMailSenderFeatureDriver.IsAllowedCaller` tells whether a remote may send its mails here. |
| [`MailSenderHandler`](MailSenderHandler.cs) | Handles the `ISendMailCommand` of [CK.IO.MailSender](../CK.IO.MailSender/README.md): it refuses a caller that is not allowed (the `ISendMailResult` carries the error), and sends the mail with the `IDefaultEmailSender`. |

The command has no `From`: the mail is sent with the MailSender's own address. The response is synchronous: the
`ISendMailResult` carries the `MessageId` and the `ErrorMessages` of the senders.

A local party that doesn't allow "MailSender" is ignored: CK.AppIdentity.MailSender.Remote handles it, and the two
packages can be referenced by the same application. Unlike the AuthCenter, the MailSender is not a singleton: there can be
several MailSenders in a tenant (each remote party uses at most one). The description of a MailSender announces it (its
roles): a remote party checks that the party it relays its mails to is a MailSender.
