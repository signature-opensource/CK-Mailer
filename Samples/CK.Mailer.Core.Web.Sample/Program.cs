using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CK.Mailer.Core.Web.Sample
{
    public class Program
    {
        public static void Main( string[] args )
        {
            var host = Host.CreateDefaultBuilder( args )
                .UseCKMonitoring()
                .ConfigureWebHostDefaults( webBuilder =>
                {
                    webBuilder.UseKestrel()
                    .UseIISIntegration()
                    .UseStartup<Startup>();
                } )
                .Build();

            host.Run();
        }
    }
}
