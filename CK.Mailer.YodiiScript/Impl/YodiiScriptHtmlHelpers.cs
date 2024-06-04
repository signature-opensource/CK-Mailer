namespace CK.Mailer.YodiiScript
{
    public class YodiiScriptHtmlHelpers
    {
        public YodiiScriptHtmlHelpers()
        {
        }

        public bool IsNullOrEmpty(object obj) => obj == null || (obj is string s && s.Length == 0);

    }
}
