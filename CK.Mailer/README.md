# CK.Mailer

The purpose of this package is to provide the basis for the implementation of mailer packages, compatible with configuration via [CK-AppIdentity](https://github.com/signature-opensource/CK-AppIdentity).

You cannot directly use this package to send an email, you sould use an implementation, like [CK.Mailer.MailKit](../CK.Mailer.MailKit).

## Configuration

The configuration CK.Mailer you need to use "EmailSender" section in the root of a party and/or local.
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

In the EmailSender section, you can register the sender(s) you want to use.

## Usage

## SimpleEmail

## How to make a new email sender

