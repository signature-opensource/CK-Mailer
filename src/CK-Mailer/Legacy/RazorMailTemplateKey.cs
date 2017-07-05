using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.Legacy
{
    public class RazorMailTemplateKey : IMailTemplateKey
    {
        public RazorMailTemplateKey( string mailKey, string areaName = null )
        {
            Key = mailKey;
            AreaName = areaName;
        }

        public string Key { get; set; }

        public string AreaName { get; set; }

        /// <summary>
        /// Gets the email virtual path.
        /// Pattern is : "~/Views/Email/{key}.cshtml or ~/Areas/{areaName}/Views/Email/{key}.cshtml 
        /// </summary>
        /// <returns>The mail content virtual path</returns>
        public virtual string GetVirtualPath()
        {
            if( String.IsNullOrEmpty( AreaName ) )
            {
                return "~/Views/Email/" + Key + ".cshtml";
            }
            else
            {
                return "~/Areas/" + AreaName + "/Views/Email/" + Key + ".cshtml";
            }

        }
    }
}
