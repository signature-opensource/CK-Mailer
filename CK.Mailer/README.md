# CK.Mailer

The purpose of this package is to provide the basis for the implementation of mailer packages, compatible with configuration via [CK-AppIdentity](https://github.com/signature-opensource/CK-AppIdentity). This package is a host for the plug-in approach to easily manage different email senders.

You cannot directly use this package to send an email, you sould use an implementation, like [CK.Mailer.MailKit](./../CK.Mailer.MailKit/README.md).


## Configuration

To configure the mailer service you need to use `EmailSender` section in the root of a party and/or local.
```json
{
  "CK-AppIdentity": {
    "Local": {
      "EmailSender": {
        [...]
      }
    },
    "Parties": [
      {
        "EmailSender": {
          [...]
        }
      }
    ]
  }
}
```

### Features

An email sender implementation is a CK-AppIdentity feature, see [The Features](https://github.com/signature-opensource/CK-AppIdentity/blob/develop/CK.AppIdentity/Features/README.md). To use this feature you can add and configure it in the `EmailSender` section. For that you need to refere the sender name as a key in the EmailSender configuration. The value can be an objet that contains the configuration of one sender or an array of object if you want to use this feature with many differents configurations.

- Using one SampleSender on the party:
```json
{
  "EmailSender": {
    "SampleSender": {
      // Configuration of the SampleSender...
    },
    "OtherSender": {
      // Configuration of the OtherSender...
    }
  }
}
```
- Using multiple senders in the party:
```json
{
  "EmailSender": {
    "SampleSender": [
      {
        // Configuration of one SampleSender...
      },
      {
        // Configuration of another SampleSender...
      }
    ]
  }
}
```

## Usage

To use CK.Mailer in you application, do not forget to Add CK-AppIdentity service configuration.

```csharp
var builder = Host.CreateApplicationBuilder();
builder.AddApplicationIdentityServiceConfiguration();
```

Then, you can inject the default sender in you code:
```csharp
public async Task SendEmailAsync( IActivityMonitor, monitor, IDefaultEmailSender sender )
{
    var email = new SimpleEmail()
      .From( "no-reply@sender.com" )
      .To( "user@recipent.com" )
      .Subject( "Hello World!" );

    var result = await sender.SendAsync( monitor, email );
}
```

Note: When an email is send by the [EmailSenderFeature](./EmailSenderFeature.cs) or the [DefaultEmailSender](./DefaultEmailSender.cs) derived class, it send the email to all senders of the party.

## SimpleEmail

The [SimpleEmail](./Models/SimpleEmail.cs) class represents the informations in an email. This is the object you pass to `IEmailSender.SendAsync`.

You can configure it with passing the values in the properties or use the builder pattern.

## How to implements an email sender

To make your own email sender you need to create a class that implements [IEmailSender](./IEmailSender.cs).

You also need to create a class that extends the [EmailSenderFactory](./EmailSenderFactory.cs) abstract class. The class name MUST ends with "EmailSenderFactory" to be validated by the abstract class constructor. The start of the name will be exposed as the `EmailSenderFactory.SenderName` and will be used to match the factory with the feature name in the configuration.

If the TryCreateEmailSender can create a sender, then it will returns true and instantiate your email sender and returns it with the out parameter.
