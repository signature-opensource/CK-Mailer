using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CK.Mailer
{
    [Serializable]
    public class RecipientModel
    {
        public RecipientModel()
        {
            Recipients = new List<Recipient>();
            CC = new List<Recipient>();
            BCC = new List<Recipient>();
        }

        public ICollection<Recipient> Recipients { get; private set; }
        public ICollection<Recipient> CC { get; private set; }
        public ICollection<Recipient> BCC { get; private set; }

        public override string ToString()
        {
            return string.Format( "A: {0}; CC: {1}; BCC: {2};",
                String.Join( ",", Recipients.Select( x => x.EmailAddress ) ),
                String.Join( ",", CC.Select( x => x.EmailAddress ) ),
                String.Join( ",", BCC.Select( x => x.EmailAddress ) ) );
        }
    }
}