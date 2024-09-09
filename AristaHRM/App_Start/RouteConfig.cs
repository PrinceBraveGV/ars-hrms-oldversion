using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AristaHRM {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            // khusus untuk aplikasi yang menggunakan web forms
            // tanda komentar di bagian berikut ini boleh dihapus

            /* Begin Web Forms Area */
            /*
            routes.IgnoreRoute("{resource}.aspx/{*pathInfo}");
            
            routes.MapPageRoute(
                routeName: "Forms", // --> nama direktori rute
                routeUrl: "Forms/Page/{id}", // --> definisi URL rute ke page ASPX
                physicalFile: "~/Areas/Forms/PageName.aspx?id={id}" // --> virtual path ke page ASPX
            );
            */
            /* End Web Forms Area */

            routes.MapRoute(
                name: "Default", // Route name
                url: "{controller}/{action}/{id}/{id2}", // URL with parameters
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional }, // Parameter defaults
                namespaces: new string[] { "AristaHRM.Controllers" }
            );

            //routes.MapRoute(
            //    name: "Default", // Route name
            //    url: "{controller}/{action}/{id}", // URL with parameters
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
            //    namespaces: new string[] { "AristaHRM.Controllers" }
            //);

            //routes.MapRoute(
            //    name: "Admin",
            //    url: "Admin/{controller}/{action}/{id}/{id2}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional } // Parameter defaults
            //);

            //routes.MapRoute(
            //    name: "Master",
            //    url: "Master/{controller}/{action}/{id}/{id2}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional } // Parameter defaults
            //);

            //routes.MapRoute(
            //    name: "Home",
            //    url: "Home/{controller}/{action}/{id}/{id2}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional } // Parameter defaults
            //);

            //routes.MapRoute(
            //    name: "Supervisor",
            //    url: "Supervisor/{controller}/{action}/{id}/{id2}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional } // Parameter defaults
            //);

            //routes.MapRoute(
            //    name: "User",
            //    url: "User/{controller}/{action}/{id}/{id2}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional } // Parameter defaults
            //);
        }
    }
}