using MimeKit;
using System.IO;
using System.Linq;

namespace CK.Mailer.Tests
{
    public class PickupDirectory
    {
        public static MimeMessage GetTheLastSentEmail( MailKitOptions options )
        {
            if( !Directory.Exists( options.PickupDirectoryPath ) ) return null;

            DirectoryInfo dir = new DirectoryInfo( options.PickupDirectoryPath );
            FileInfo lastFile = dir.GetFiles()
                .OrderByDescending( f => f.CreationTimeUtc )
                .First();
            
            using( var stream = lastFile.OpenRead() )
            {
                return MimeMessage.Load( stream );
            }
        }
    }
}
