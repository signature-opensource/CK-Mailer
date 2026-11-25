# CK.AppIdentity.MailRelay.Local

The Local side of the "MailRelay" [asymmetric feature](https://github.com/signature-opensource/CK-AppIdentity/blob/stable/CK.AppIdentity/Features/README.md#asymmetric-features):
a local party that declares "MailRelay" sends the mails of its remote parties with its own senders (its "MailSender", see
[CK.Mailer](../CK.Mailer/README.md)). Only the relay holds the SMTP servers, their credentials and the `From` addresses;
its remotes send it their rendered mails (see [CK.AppIdentity.MailRelay.Remote](../CK.AppIdentity.MailRelay.Remote/README.md)).

An application that sends its own mails needs none of this: its "MailSender" is enough.

## Configuration

The relay declares "MailRelay" and has senders; the remotes that may send their mails through it declare "MailRelayRemote"
(the feature is opt-in):

```jsonc
"CK-AppIdentity": {
  "FullName": "Signature/$Backend",
  "Local": {
    "MailRelay": { "Sender": "Relayed" },   // Optional (or "MailRelay": true): the sender used when the caller names none.
    "MailSender": {
      "MailKit": {
        "Senders": {
          "NoReply": { "Host": "smtp.example.com", "User": "...", "Password": { "$SECRET": "Smtp:Password" }, "From": "no-reply@example.com" },
          "Relayed": { "Host": "smtp.example.com", "User": "...", "Password": { "$SECRET": "Smtp:Password" }, "From": "relay@example.com" }
        }
      },
      "Default": "NoReply"
    }
  },
  "Parties": [
    { "PartyName": "$Front", "MailRelayRemote": true }
  ]
}
```

A mail is sent with the sender that the caller names (`"MailRelayRemote": { "Sender": "..." }` on its side), else the
`"MailRelay": { "Sender" }` of the relay, else its default sender. A relay without sender, or whose `"Sender"` is not one of
its senders, is a start-up error.

## How it works

| Type | |
|---|---|
| [`LocalMailRelayFeatureDriver`](LocalMailRelayFeatureDriver.cs) | The driver of the "MailRelay" feature on this side. `LocalMailRelayFeatureDriver.IsAllowedCaller` tells whether a remote may send its mails here, `LocalMailRelayFeatureDriver.GetConfiguredSender` gives the relay's `"Sender"`. |
| [`MailRelayHandler`](MailRelayHandler.cs) | Handles the `ISendMailCommand` of [CK.IO.MailRelay](../CK.IO.MailRelay/README.md): it refuses a caller that is not allowed, an unknown sender and a relay sender (the `ISendMailResult` carries the error), and sends the mail. |

The command has no `From`: the mail is sent with the address of the relay's sender. The response is synchronous: the
`ISendMailResult` carries the `MessageId` and the `ErrorMessages` of the sender.

A relay has no relay sender (its "MailRelayRemote" remotes are its callers): a mail is never relayed twice. A local party
that doesn't declare "MailRelay" is ignored: CK.AppIdentity.MailRelay.Remote handles it, and the two packages can be
referenced by the same application. There can be several relays in a tenant.
