# CK.Mailer.MailKit

This package is an implementation of [CK.Mailer](./../CK.Mailer/README.md) via [Mailkit](https://github.com/jstedfast/MailKit) SMTP email client.

## Configuration

The configuration of this mailer is very flexible and does not require all fields, depending on the expected behavior.

Here is the all configuration properties in an `appsettings.json` with default values:
```json
{
  "CK-AppIdentity": {
    "Local": {
      "EmailSender": {
        "Smtp": {
          "UsePickupDirectory": false,
          "PickupDirectory": null,
          "SendEmail": true,
          "Host": null,
          "Port": 587,
          "RequiredAuthentication": true,
          "User": null,
          "Password": null,
          "SocketOptions": null,
          "UseSsl": true
        }
      }
    }
  }
}
```
- You can find the defaults in the [MailKitSenderOptions](./MailKitSenderOptions.cs).
- You can find the configuration parsing in [SmtpEmailSenderFactory](./SmtpEmailSenderFactory.cs).

The sender can store the message on disk, send it, do both, or neither.

To enable or disabled features, you can use the `UsePickupDirectory`, `SendEmail` and `RequiresAuthentication` properties.

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
  "UserName": "[...]",
  "Password": "[...]"
}
```
Or if the email server not required anthentication:
```json
{
  "Host": "[...]",
  "RequiredAuthentication": false
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

### SockupOptions

To configure the SocketOptions, uses the string format for `MailKit.Security.SecureSocketOptions` values.
```json
{
  "SocketOptions": "StartTlsWhenAvailable"
}
```

Available values are `None`, `Auto`, `SslOnConnect`, `StartTls` and `StartTlsWhenAvailable`.
For more inforamtions see the [SecureSocketOptions](https://github.com/jstedfast/MailKit/blob/master/MailKit/Security/SecureSocketOptions.cs).

When the SockupOptions property is used, the UseSsl is not used.

## MailKitSender

This class implements the `IEmailSender`.

When you call `MailKitSender.SaveToPickupDirectoryAsync` or `MailKitSender.SendAsync` (with UsePickupDirectory set to true), you can obtain the path where the email was saved in the `SendResponse.MessageId` property.

## Other

If you want to send your `SimpleEmail` with MailKit but without the MailKitSender, you can obtain a `MimeMessage` from the email with the `SimpleEmail.GetMimeMessage()` extension method.
