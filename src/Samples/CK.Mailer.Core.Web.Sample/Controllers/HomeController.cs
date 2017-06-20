using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CK.Core;

namespace CK.Mailer.Core.Web.Sample
{
    public class HomeController
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
