using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.YodiiScript
{
    public class YodiiScriptHtmlHelpers
    {
        public YodiiScriptHtmlHelpers()
        {
        }

        public bool IsNullOrEmpty(object obj)
        {
            if( obj == null ) return true;

            if( obj is string )
            {
                return (string)obj == String.Empty;
            }

            return false;
        }
    }
}
