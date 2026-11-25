# CK.Mailer.MailKit

The "MailKit" senders of the "MailSender" feature of [CK.Mailer](../CK.Mailer/README.md): mails sent by an SMTP server
through the [MailKit](https://github.com/jstedfast/MailKit) client, or written in a pickup directory. It brings the
"MailKit" option of the feature ([`MailKitOption`](MailKitOption.cs)): its `"Senders"` are the named senders of a party.

## Configuration

The configuration is a model ([`MailKitOptionConfiguration`](MailKitOption.cs)): the senders by name, the settings of a
sender are MailKit's ([`MailKitSenderConfiguration`](MailKitOption.cs)). All of them, with their default values:
```jsonc
"CK-AppIdentity": {
  "Local": {
    "MailSender": {
      "MailKit": {
        "Senders": {
          "NoReply": {
            "UsePickupDirectory": false,
            "PickupDirectory": null,
            "SendEmail": true,
            "Host": null,
            "Port": 587,
            "RequiresAuthentication": true,
            "User": null,
            "Password": { "$SECRET": "Smtp:Password" },  // A secret: a handle, never the value.
            "SocketOptions": "Auto",
            "From": null
          }
        }
      }
    }
  }
}
```
An unknown key or an invalid value fails the start of the application (the errors of the model). The option needs at
least one sender.

The sender can store the message on disk, send it, do both, or neither: `UsePickupDirectory`, `SendEmail` and
`RequiresAuthentication` enable or disable these behaviors. `From` is the address of the sender.

### Pickup directory

To only use the pickup directory you can use the following configuration:
```json
{
  "UsePickupDirectory": true,
  "PickupDirectory": "[...]",
  "SendEmail": false
}
```

### Send email

To send an email you need at least these properties:
```json
{
  "Host": "[...]",
  "User": "[...]",
  "Password": "[...]"
}
```
Or if the email server requires no authentication:
```json
{
  "Host": "[...]",
  "RequiresAuthentication": false
}
```

### Authentication

If you don't use authentication, you don't need to use the User and Password properties.
```json
{
  "RequiresAuthentication": false
}
```
Otherwise, if you need them, then you MUST use them. (And not need to use RequiresAuthentication.)
```json
{
  "User": "[...]",
  "Password": "[...]"
}
```

### SocketOptions

To configure the SocketOptions, uses the string format for `MailKit.Security.SecureSocketOptions` values.
```json
{
  "SocketOptions": "StartTlsWhenAvailable"
}
```

Available values are `None`, `Auto` (the default: MailKit chooses from the port), `SslOnConnect`, `StartTls` and
`StartTlsWhenAvailable`.
For more information see the [SecureSocketOptions](https://github.com/jstedfast/MailKit/blob/master/MailKit/Security/SecureSocketOptions.cs).

## MailKitSender

This class implements the `IEmailSender`.

When you call `MailKitSender.SaveToPickupDirectoryAsync` or `MailKitSender.SendAsync` (with UsePickupDirectory set to true), you can obtain the file name of the email `SendResponse.MessageId` property.

If you send an email, the MessageId will contains the MessageId from the `MimeMessage.MessageId`.

If you send both, both ids are in the MessageId, separated by a comma.

## Other

If you want to send your `SimpleEmail` with MailKit but without the MailKitSender, you can obtain a `MimeMessage` from the email with the `SimpleEmail.GetMimeMessage()` extension method.

You can also obtain a `SimpleEmail` from a `MimeMessage` with the `MimeMessage.GetSimpleEmail()` extension method.
