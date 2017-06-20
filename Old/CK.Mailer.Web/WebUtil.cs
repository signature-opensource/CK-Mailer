using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CK.Mailer
{
    /// <summary>
    /// This static class contains helper methods that need System.Web to work.
    /// </summary>
    static class WebUtil
    {
        /// <summary>
        /// Removes diacritics from a string (converts them to their basic form after having filtered NonSpacingMark characters).
        /// From http://blogs.msdn.com/michkap/archive/2007/05/14/2629747.aspx
        /// </summary>
        /// <param name="input">The string to process.If null, <see cref="String.Empty"/> is returned.</param>
        /// <returns>The string from which diacritics are removed.</returns>
        public static string RemoveDiacritics( string input )
        {
            StringBuilder sharedBuffer = null;
            return RemoveDiacritics( ref sharedBuffer, input );
        }

        /// <summary>
        /// Removes diacritics from a string (converts them to their basic form after having filtered NonSpacingMark characters).
        /// From http://blogs.msdn.com/michkap/archive/2007/05/14/2629747.aspx
        /// </summary>
        /// <param name="sharedBuffer">A shared buffer that can be reused.</param>
        /// <param name="input">The string to process.If null, <see cref="String.Empty"/> is returned.</param>
        /// <returns>The string from which diacritics are removed.</returns>
        public static string RemoveDiacritics( ref StringBuilder sharedBuffer, string input )
        {
            if( input == null || input.Length == 0 ) return String.Empty;
            string stFormD = input.Normalize( NormalizationForm.FormD );
            int len = stFormD.Length;
            if( sharedBuffer == null ) sharedBuffer = new StringBuilder( len );
            else
            {
                sharedBuffer.EnsureCapacity( len );
                sharedBuffer.Length = 0;
            }
            for( int i = 0; i < len; i++ )
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory( stFormD, i );
                if( uc != UnicodeCategory.NonSpacingMark )
                {
                    sharedBuffer.Append( stFormD[i] );
                }
            }
            return sharedBuffer.ToString().Normalize( NormalizationForm.FormC );
        }


        static Regex _rHtmlToTextTag1 = new Regex( "\\s*(<(br\\s*/|/p|/div)\\s*>\\s*)+", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled );
        static Regex _rHtmlToTextTag2 = new Regex( "((<(.|\\n)+?>)|[\\s-[\\u00A0]]{2,})+", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled );
        static Regex _rHtmlToTextTag3 = new Regex( "( |\\xA0|\\t)+\\r|\\n( |\\xA0|\\t)+|( |\\t){2,}", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled );

        /// <summary>
        /// Converts a html string by removing all tags and transforming &lt;br/&gt;, &lt;/p&gt; and &lt;/div&gt; 
        /// into \r\n end of lines. &amp;nbsp; entity maps to either '\xA0' (dec. 160) (ANSI non breakable space) 
        /// character or to the '\x20' (dec. 32) if <paramref name="nbspToNormalWhiteSpace"/> is set to true.
        /// Consider using <see cref="HtmlDecodeWithBr"/> if html is very simple (contains only html entities and
        /// &lt;br /&gt; elements.
        /// </summary>
        /// <param name="str">The html string to obtain in text.</param>
        /// <param name="nbspToNormalWhiteSpace">True to map '\xA0' (dec. 160) (ANSI non breakable space) character to 
        /// the '\x20' (dec. 32) "normal" white space.</param>
        /// <returns>Plain text.</returns>
        static public string HtmlToText( string str, bool nbspToNormalWhiteSpace )
        {
            if( str == null || str.Length == 0 ) return String.Empty;
            StringBuilder sb = new StringBuilder( str.Length );
            // First replace <br>, end of <p> and <div> with markers.
            Match m = _rHtmlToTextTag1.Match( str );
            int from;
            if( m.Success )
            {
                from = 0;
                do
                {
                    sb.Append( str, from, m.Index - from );
                    from = m.Index + m.Length;
                    // Reserves 2 chars to avoid memory rellocation.
                    sb.Append( "\x01\x01" );
                    m = m.NextMatch();
                }
                while( m.Success );
                sb.Append( str, from, str.Length - from );
                str = sb.ToString();
                sb.Length = 0;
            }
            // Then skips all tags: replace them with a white space.
            m = _rHtmlToTextTag2.Match( str );
            if( m.Success )
            {
                from = 0;
                do
                {
                    sb.Append( str, from, m.Index - from );
                    from = m.Index + m.Length;
                    sb.Append( '\x20' );
                    m = m.NextMatch();
                }
                while( m.Success );
                sb.Append( str, from, str.Length - from );
                str = sb.ToString();
                sb.Length = 0;
            }
            StringWriter sw = new StringWriter( sb );
            HttpUtility.HtmlDecode( str, sw );
            if( nbspToNormalWhiteSpace ) sb.Replace( '\xA0', '\x20' );
            from = 0;
            while( from < sb.Length )
                if( "\x20\x01\xA0\t\f".IndexOf( sb[from] ) >= 0 ) ++from;
                else break;
            if( from == sb.Length ) return String.Empty;
            int to = sb.Length;
            while( "\x20\x01\xA0\t\f".IndexOf( sb[--to] ) >= 0 ) ;
            int len = to - from + 1;
            sb.Replace( "\x01\x01", "\r\n", from, len );
            str = sb.ToString( from, len );
            // Removes multiples white spaces.
            m = _rHtmlToTextTag3.Match( str );
            if( m.Success )
            {
                sb.Length = 0;
                from = 0;
                do
                {
                    sb.Append( str, from, m.Index - from );
                    from = m.Index + m.Length;
                    if( str[m.Index] == '\n' ) sb.Append( '\n' );
                    else if( str[from - 1] == '\r' ) sb.Append( '\r' );
                    else sb.Append( ' ' );
                    m = m.NextMatch();
                }
                while( m.Success );
                sb.Append( str, from, str.Length - from );
                str = sb.ToString();
            }

            return str;
        }
    }
}