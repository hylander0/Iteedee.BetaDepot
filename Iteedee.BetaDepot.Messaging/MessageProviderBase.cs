using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iteedee.BetaDepot.Messaging
{
    public enum MessageType
    {
        Email,
        SMS
    }

    public abstract class MessageProviderBase : System.Configuration.Provider.ProviderBase
    {
        public virtual void SetParameters(NameValueCollection config)
        {

        }

        public abstract void SendMessage(MessageType msgType, string receiver, string subject, string message);
    }
}
