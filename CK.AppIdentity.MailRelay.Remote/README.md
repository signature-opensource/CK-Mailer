# CK.AppIdentity.MailRelay.Remote

The Remote side of the "MailRelay" [asymmetric feature](https://github.com/signature-opensource/CK-AppIdentity/blob/stable/CK.AppIdentity/Features/README.md#asymmetric-features):
a local party sends mails through its relay parties, without any mail configuration of its own (no SMTP server, no
credentials). Each remote that declares "MailRelayRemote" is a sender of its local party (see the "MailSender" feature of
[CK.Mailer](../CK.Mailer/README.md)), named after the remote: the code that sends mails doesn't change, it uses a sender
(`IDefaultEmailSender`, `MailSenderExtensions.GetMailSender`) whose mails are sent to the relay (see
[CK.AppIdentity.MailRelay.Local](../CK.AppIdentity.MailRelay.Local/README.md)).

## Configuration

```jsonc
"CK-AppIdentity": {
  "FullName": "Signature/$Front",
  "Local": {
    "MailSender": { "Default": "$CorpRelay" }   // Required when the party has several senders.
  },
  "Parties": [
    { "PartyName": "$CorpRelay", "Address": "...", "MailRelayRemote": { "Sender": "Support" } },   // The relay's "Support" sender.
    { "PartyName": "$PartnerRelay", "Address": "...", "MailRelayRemote": true }                    // The relay chooses.
  ]
}
```

- A local party can have several relays; a remote added later adds a sender, a destroyed one removes it.
- A `"Sender"` names a sender of the relay: it is checked by the relay when a mail is sent (an unknown name is an error of
  the sending).
- The relay must accept this party (it declares "MailRelayRemote" on its side): otherwise it refuses the mails.
- The local party must have the "MailSender" feature (it is enabled by default): a relay remote of a party whose
  "MailSender" is off is a start-up error.

## How it works

| Type | |
|---|---|
| [`RemoteMailRelayFeatureDriver`](RemoteMailRelayFeatureDriver.cs) | The driver of the "MailRelay" feature on this side: it adds a sender, named after the remote, to the `MailSenders` of the local party of each remote that declares "MailRelayRemote". |
| [`RelayEmailSender`](RelayEmailSender.cs) | The sender of a relay (internal): it sends a mail as an `ISendMailCommand` of [CK.IO.MailRelay](../CK.IO.MailRelay/README.md) on the Cris channel to the relay, with the `"Sender"` of the remote, and waits for its `ISendMailResult` (30 seconds at most). |

The `From` of a mail is not sent: the relay uses the address of its sender. The attachments travel in the command (their
content is read when the mail is sent).

A local party that declares "MailRelay" (a relay) is ignored: its "MailRelayRemote" remotes are its callers
(CK.AppIdentity.MailRelay.Local handles them), and the two packages can be referenced by the same application.
