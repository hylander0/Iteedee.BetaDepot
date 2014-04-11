using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iteedee.BetaDepot.Messaging
{
    public class MessageProviderConfiguration : ConfigurationSection
    {

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection)base["providers"];
            }
        }

        [ConfigurationProperty("default", DefaultValue = "EmailProvider")]
        public string DefaultProviderName
        {
            get
            {
                return base["default"] as string;
            }
        }

    }
}
