using Microsoft.AspNetCore.Mvc;

namespace CK.Mailer.Core.Web.Sample
{
    public class HomeController : Controller
    {
        private readonly IMailerService _mailService;

        public HomeController( IMailerService mailService )
        {
            _mailService = mailService;
        }

        public IActionResult Index()
        {
            return new VirtualFileResult( "~/index.html", "text/html" );
        }

        public IActionResult Login()
        {
            return new VirtualFileResult( "~/index.html", "text/html" );
        }
    }
}
