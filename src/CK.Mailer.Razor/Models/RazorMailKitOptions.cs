using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.Razor.Models
{
    public class RazorMailKitOptions : MailKitOptions
    {
        /// <summary>
        /// Specified the physical file path for .cshtml template.
        /// </summary>
        /// <remarks>
        /// If null, the embedded resources is used
        /// </remarks>
        public string ViewsPhysicalPath { get; set; }
    }
}
