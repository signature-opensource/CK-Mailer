using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public class Recipients
    {
        public Recipients()
        {
            From = new InternetAddressList();
            ResentFrom = new InternetAddressList();
            ReplyTo = new InternetAddressList();
            ResentReplyTo = new InternetAddressList();
            To = new InternetAddressList();
            ResentTo = new InternetAddressList();
            Cc = new InternetAddressList();
            ResentCc = new InternetAddressList();
            Bcc = new InternetAddressList();
            ResentBcc = new InternetAddressList();
        }

        //
        // Summary:
        //     Gets the list of addresses in the From header.
        //
        // Remarks:
        //     The "From" header specifies the author(s) of the message.
        //     If more than one MimeKit.MailboxAddress is added to the list of "From" addresses,
        //     the MimeKit.MimeMessage.Sender should be set to the single MimeKit.MailboxAddress
        //     of the personal actually sending the message.
        public InternetAddressList From { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Resent-From header.
        //
        // Remarks:
        //     The "Resent-From" header specifies the author(s) of the messagebeing resent.
        //     If more than one MimeKit.MailboxAddress is added to the list of "Resent-From"
        //     addresses, the MimeKit.MimeMessage.ResentSender should be set to the single MimeKit.MailboxAddress
        //     of the personal actually sending the message.
        public InternetAddressList ResentFrom { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Reply-To header.
        //
        // Remarks:
        //     When the list of addresses in the Reply-To header is not empty, it contains the
        //     address(es) where the author(s) of the message prefer that replies be sent.
        //     When the list of addresses in the Reply-To header is empty, replies should be
        //     sent to the mailbox(es) specified in the From header.
        public InternetAddressList ReplyTo { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Resent-Reply-To header.
        //
        // Remarks:
        //     When the list of addresses in the Resent-Reply-To header is not empty, it contains
        //     the address(es) where the author(s) of the resent message prefer that replies
        //     be sent.
        //     When the list of addresses in the Resent-Reply-To header is empty, replies should
        //     be sent to the mailbox(es) specified in the Resent-From header.
        public InternetAddressList ResentReplyTo { get; }
        //
        // Summary:
        //     Gets the list of addresses in the To header.
        //
        // Remarks:
        //     The addresses in the To header are the primary recipients of the message.
        public InternetAddressList To { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Resent-To header.
        //
        // Remarks:
        //     The addresses in the Resent-To header are the primary recipients of the message.
        public InternetAddressList ResentTo { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Cc header.
        //
        // Remarks:
        //     The addresses in the Cc header are secondary recipients of the message and are
        //     usually not the individuals being directly addressed in the content of the message.
        public InternetAddressList Cc { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Resent-Cc header.
        //
        // Remarks:
        //     The addresses in the Resent-Cc header are secondary recipients of the message
        //     and are usually not the individuals being directly addressed in the content of
        //     the message.
        public InternetAddressList ResentCc { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Bcc header.
        //
        // Remarks:
        //     Recipients in the Blind-Carpbon-Copy list will not be visible to the other recipients
        //     of the message.
        public InternetAddressList Bcc { get; }
        //
        // Summary:
        //     Gets the list of addresses in the Resent-Bcc header.
        //
        // Remarks:
        //     Recipients in the Resent-Bcc list will not be visible to the other recipients
        //     of the message.
        public InternetAddressList ResentBcc { get; }
    }
}
