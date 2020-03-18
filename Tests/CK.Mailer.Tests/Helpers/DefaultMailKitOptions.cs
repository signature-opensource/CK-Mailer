namespace CK.Mailer.Tests
{
    public static class DefaultMailKitOptions
    {
        public static MailKitOptions Default
        {
            get
            {
                return new MailKitOptions()
                {
                    DefaultSenderEmail = "default@ckmailer.org",
                    DefaultSenderName = "ckmailer",
                    SendMails = false,
                    UsePickupDirectory = true,
                    PickupDirectoryPath = "./PickupDirectory"
                };
            }
        }
    }
}
