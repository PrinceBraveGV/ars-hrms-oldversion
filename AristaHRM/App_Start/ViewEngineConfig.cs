using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AristaHRM
{
    public class ViewEngineConfig
    {
        public ViewEngineConfig()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ViewEngines.Engines.Add(new WebFormViewEngine());
            ViewEngines.Engines.Add(new CustomRazorEngine());
            ViewEngines.Engines.Add(new CustomWebFormEngine());
        }
    }

    // setting utama: Razor view
    /// <summary>
    /// Kelas khusus untuk mencari Razor view sesuai dengan definisi direktori yang ditentukan.
    /// </summary>
    public class CustomRazorEngine : RazorViewEngine
    {
        public CustomRazorEngine()
        {
            /*
             * Catatan khusus untuk pengubah lokasi:
             * {0} = nama action method
             * {1} = nama controller
             * {2} = nama area
             * {3} = nama feature
             * 
             * Rute untuk area:
             * Contoh => /Areas/Mobile/Views/Home/MobileIndex.cshtml
             * Register rute => /Areas/{2}/Views/{1}/{0}.cshtml
             */

            AreaMasterLocationFormats = new[] 
            {
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };

            AreaViewLocationFormats = new[] 
            {
                "~/Areas/{2}/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Home/{0}.cshtml",
                "~/Areas/{2}/Views/Admin/{0}.cshtml",
                "~/Areas/{2}/Views/Manager/{0}.cshtml",
                "~/Areas/{2}/Views/Input/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Supervisor/{0}.cshtml",
                "~/Areas/{2}/Views/User/{0}.cshtml"
            };

            AreaPartialViewLocationFormats = AreaViewLocationFormats;

            MasterLocationFormats = new[]
            {
                "~/Views/Shared/{0}.cshtml"
            };

            ViewLocationFormats = new[]
            {
                "~/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.cshtml",
                "~/Views/Home/{0}.cshtml",
                "~/Views/Admin/{0}.cshtml",
                "~/Views/Manager/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Supervisor/{0}.cshtml",
                "~/Views/User/{0}.cshtml"
            };
            PartialViewLocationFormats = ViewLocationFormats;

            FileExtensions = new[]
            {
                "cshtml",
                "vbhtml"
            };
        }
    }

    // mempertahankan web forms ASPX jika ada
    /// <summary>
    /// Kelas khusus untuk mencari ASPX view sesuai dengan definisi direktori yang ditentukan.
    /// </summary>
    public class CustomWebFormEngine : WebFormViewEngine
    {
        public CustomWebFormEngine()
        {
            AreaMasterLocationFormats = new[] {
                "~/Areas/{2}/{1}/{0}.master",
                "~/Areas/{2}/Views/{1}/{0}.master",
                "~/Areas/{2}/Views/Shared/{0}.master"
            };

            AreaViewLocationFormats = new[] {
                "~/Areas/{2}/{1}/{0}.aspx",
                "~/Areas/{2}/{1}/{0}.ascx",
                "~/Areas/{2}/Views/{1}/{0}.aspx",
                "~/Areas/{2}/Views/{1}/{0}.ascx",
                "~/Areas/{2}/Views/Home/{0}.aspx",
                "~/Areas/{2}/Views/Home/{0}.ascx",
                "~/Areas/{2}/Views/Shared/{0}.aspx",
                "~/Areas/{2}/Views/Shared/{0}.ascx",
                "~/Areas/{2}/Views/Admin/{0}.aspx",
                "~/Areas/{2}/Views/Admin/{0}.ascx",
                "~/Areas/{2}/Views/User/{0}.aspx",
                "~/Areas/{2}/Views/User/{0}.ascx"
            };

            AreaPartialViewLocationFormats = AreaViewLocationFormats;

            MasterLocationFormats = new[]
            {
                "~/{1}/{0}.master",
                "~/Views/{1}/{0}.master",
                "~/Views/Shared/{0}.master"
            };

            ViewLocationFormats = new[]
            {
                "~/{1}/{0}.aspx",
                "~/{1}/{0}.ascx",
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Home/{0}.aspx",
                "~/Views/Home/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx",
                "~/Views/Admin/{0}.aspx",
                "~/Views/Admin/{0}.ascx",
                "~/Views/User/{0}.aspx",
                "~/Views/User/{0}.ascx"
            };
            PartialViewLocationFormats = ViewLocationFormats;

            FileExtensions = new[]
            {
                "aspx",
                "ascx",
                "master"
            };
        }
    }
}