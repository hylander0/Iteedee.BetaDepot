﻿using System.Web;
using System.Web.Optimization;

namespace Iteedee.BetaDepot
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-{version}.js")
                .Include("~/Scripts/jquery.ui.widget.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryFileUpload")
                    .Include("~/Scripts/vendor/jquery.iframe-transport.js")
                    .Include("~/Scripts/vendor/jquery.fileupload.js")
                    );

            bundles.Add(new ScriptBundle("~/bundles/bootstrapValidator")
                    .Include("~/Scripts/vendor/bootstrapValidator.min.js")
                    );

            bundles.Add(new ScriptBundle("~/bundles/app")
                    .Include("~/Scripts/app.js")
                    );

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/site.css"));
        }
    }
}
