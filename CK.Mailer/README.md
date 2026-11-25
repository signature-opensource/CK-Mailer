# CK.Mailer

How the parties of an application send their mails. It brings the "MailSender" feature of
[CK-AppIdentity](https://github.com/signature-opensource/CK-AppIdentity/blob/stable/CK.AppIdentity/Features/README.md):
each local party (the root and its tenant domains) has its own named senders, configured in its "MailSender" section.
This package defines the senders' contract; the senders themselves come from other packages:
[CK.Mailer.MailKit](../CK.Mailer.MailKit/README.md) (the "MailKit" senders: an SMTP server or a pickup directory) and
[CK.AppIdentity.MailRelay.Remote](../CK.AppIdentity.MailRelay.Remote/README.md) (a relay party that sends the mails for
this one).

## Configuration

The "MailSender" section of a local party holds its senders, grouped by kind (an option of the feature: "MailKit"), each
one named; `"Default"` names the default sender:

```jsonc
"CK-AppIdentity": {
  "Local": {
    "MailSender": {
      "MailKit": {
        "Senders": {
          "NoReply": { "Host": "smtp.acme.com", "User": "...", "Password": { "$SECRET": "Smtp:Password" }, "From": "noreply@acme.com" },
          "Support": { "Host": "smtp.acme.com", "User": "...", "Password": { "$SECRET": "Smtp:Password" }, "From": "support@acme.com" }
        }
      },
      "Default": "NoReply"
    }
  }
}
```

- **A sender name** is unique in the party across the kinds: an identifier (letters, digits, `_`, `-`, `.`). A relay is a
  sender named after its remote party (`"$CorpRelay"`).
- **The default sender:** with a single sender, it is the default; with several, `"Default"` is required.
- **A mail goes through one sender.**
- **Enabled by default:** a party without "MailSender" section has no sender, sending a mail fails (the
  `SendResponse.ErrorMessages` say why); `"MailSender": false` turns the feature off. A remote has no "MailSender"
  (declaring it on a remote is an error).
- An invalid configuration (an unknown key of a sender, a missing default) fails the start of the application.

## Usage

`IDefaultEmailSender` is the default sender of the root party:

```csharp
public async Task SendEmailAsync( IActivityMonitor monitor, IDefaultEmailSender sender )
{
    var email = new SimpleEmail { Subject = "Hello World!" }.To( "user@recipient.com" );
    var result = await sender.SendAsync( monitor, email );
}
```

The senders of any local party are its `MailSenders` feature; `MailSenderExtensions.GetMailSender( party, name )` gets one
(the default one when `name` is null):

```csharp
var support = tenant.GetMailSender( "Support" );
var result = await support.SendAsync( monitor, email );
```

A sender is resolved when a mail is sent: a missing sender (or party without the feature) fails the sending, it doesn't
throw. A package that sends mails on behalf of a party and needs a specific sender names it in its own configuration
(as the `"Sender"` of the "PasswordLost" option of the AuthCenter).

## Types

| Type | |
|---|---|
| [`MailSenderFeatureDriver`](MailSenderFeatureDriver.cs) | The driver of the "MailSender" feature: it gives each local party its `MailSenders`. |
| [`MailSenders`](MailSenders.cs) | The senders of a local party: `Names`, `Find`, `Default`, `GetSender( name )`; `TryAdd` and `Remove` for the packages that add senders at run time (the relays). |
| [`MailSenderOption`](MailSenderOption.cs) | The base class of a sender kind: an option of the feature whose configuration model holds named senders. |
| [`MailSenderConfiguration`](MailSenderFeatureDriver.cs) | The configuration model of the feature: its `Default`. |
| [`IEmailSender`](IEmailSender.cs), [`IDefaultEmailSender`](IDefaultEmailSender.cs) | A sender, and the default sender of the root party (a singleton service). |
| [`SimpleEmail`](Models/SimpleEmail.cs), [`SendResponse`](Models/SendResponse.cs) | The mail (properties or a fluent builder) and the result of its sending (`MessageId`, `ErrorMessages`). |

## A sender kind

A new kind of sender is an option of the "MailSender" feature: a class that specializes `MailSenderOption`, decorated with
its name, its activation and its configuration model, whose senders are a dictionary by name (`"Senders"`: a model is a
class, the named entries are one of its properties). Its constructor receives the checked model (the errors are the
reader's and the model's `Validate`), the secrets are declared for the builder:

```csharp
[PartyFeatureOption( "MailKit", TypeActivation.Optional )]
[Configuration<MailKitOptionConfiguration>]
[ConfigurationSecret( "Party:Senders:*:Password" )]
public sealed class MailKitOption : MailSenderOption
{
    public MailKitOption( MailKitOptionConfiguration configuration )
    {
        _options = configuration.Senders.ToDictionary( kv => kv.Key, kv => kv.Value.CreateOptions(), StringComparer.OrdinalIgnoreCase );
        _senders = _options.ToDictionary( kv => kv.Key, kv => (IEmailSender)new MailKitSender( kv.Value ), StringComparer.OrdinalIgnoreCase );
    }

    public override IReadOnlyDictionary<string, IEmailSender> Senders => _senders;
}
```

See the [options of a feature](https://github.com/signature-opensource/CK-AppIdentity/blob/stable/CK.AppIdentity/Features/README.md).
