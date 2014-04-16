using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Iteedee.BetaDepot
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Platform",
                url: "{controller}/{action}/{platform}/{id}",
                defaults: new { controller = "Platform", action = "Index", platform = UrlParameter.Optional, id = UrlParameter.Optional }
            );

            // Add our route registration for MvcSiteMapProvider sitemaps
            //MvcSiteMapProvider.Web.Mvc.XmlSiteMapController.RegisterRoutes(routes);

        }
    }
}
