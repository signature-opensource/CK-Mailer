using MimeKit;
using System.IO;
using System.Linq;

namespace CK.Mailer.Tests;

public static class PickupDirectory
{
    public static MimeMessage? GetTheLastSentEmail( string pickupDirectory )
    {
        if( !Directory.Exists( pickupDirectory ) ) return null;

        var lastFile = new DirectoryInfo( pickupDirectory )
            .GetFiles()
            .OrderByDescending( f => f.CreationTimeUtc )
            .FirstOrDefault();

        if( lastFile is null ) return null;

        using var stream = lastFile.OpenRead();
        return MimeMessage.Load( stream );
    }
}
