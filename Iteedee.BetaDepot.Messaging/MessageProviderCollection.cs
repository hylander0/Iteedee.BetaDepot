using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iteedee.BetaDepot.Messaging
{
    public class MessageProviderCollection : ProviderCollection
    {
        new public MessageProviderBase this[string name]
        {
            get { return (MessageProviderBase)base[name]; }
        }
    }
}
