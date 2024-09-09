using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace AristaHRM
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquerymobile").Include(
                        "~/Scripts/jquery-1.*", "~/Scripts/jquery-ui-*", "~/Scripts/jquery.mobile-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/Mobile/css").Include(
                        "~/Content/Mobile.css"));

            bundles.Add(new StyleBundle("~/Content/jquerymobile/css").Include(
                        "~/Content/jquery.mobile-{version}.css"));

            bundles.Add(new ScriptBundle("~/bundles/jquerydate").Include(
                        "~/Scripts/jquery.datePicker*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/Site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/jquery-ui.css",
                        "~/Content/themes/base/jquery-ui.css",
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new ScriptBundle("~/bundles/devexpress").Include(
                "~/Scripts/dx.chartjs.js",
                "~/Scripts/dx.webappsjs.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/angularbase").Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-*"
                ));

            bundles.Add(new ScriptBundle("~/bundles/nprogress").Include(
                "~/Scripts/nprogress*"
                ));

            bundles.Add(new ScriptBundle("~/bundles/pdf").Include(
                "~/Scripts/pdf*"
                ));

            bundles.Add(new ScriptBundle("~/bundles/momentbase").Include(
                "~/Scripts/moment*",
                "~/Scripts/moment-*"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Scripts/HRIS.js"
                ));
        }
    }
}