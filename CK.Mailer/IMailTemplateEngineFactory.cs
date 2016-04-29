using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public interface IMailTemplateEngineFactory
    {
        IMailTemplateEngine<Q> GetEngine<Q>( Q key ) where Q : IMailTemplateKey;
        void AddEngine<Q>( IMailTemplateEngine<Q> engine ) where Q : IMailTemplateKey;
    }
}