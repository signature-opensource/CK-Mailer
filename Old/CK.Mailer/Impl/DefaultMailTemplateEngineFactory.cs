using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    /// <summary>
    /// Really simple engine factory for mailer
    /// </summary>
    public class DefaultMailTemplateEngineFactory : IMailTemplateEngineFactory
    {
        private Dictionary<Type, object> _dic;
        public DefaultMailTemplateEngineFactory()
        {
            _dic = new Dictionary<Type, object>();
        }

        public void AddEngine<Q>( IMailTemplateEngine<Q> engine ) where Q : IMailTemplateKey
        {
            if( engine == null ) throw new ArgumentNullException( "engine" );
            _dic.Add( typeof( Q ), engine );
        }

        public IMailTemplateEngine<Q> GetEngine<Q>( Q key ) where Q : IMailTemplateKey
        {
            if( key == null ) throw new ArgumentNullException( "key" );
            return (IMailTemplateEngine<Q>)_dic[key.GetType()];
        }
    }
}