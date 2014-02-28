using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Iteedee.BetaDepot.Startup))]
namespace Iteedee.BetaDepot
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
