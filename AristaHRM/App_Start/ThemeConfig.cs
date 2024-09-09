using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using DevExpress.Web.Internal;

namespace AristaHRM
{

    public static class Theme
    {
        const String CurrentKey = "theme",
                     DefaultTheme = "Aqua";

        // nama tema
        public static List<string> ThemesList = new List<string>()
        {
            "Aqua",
            "DevEx",
            "Glass",
            "Metropolis",
            "MetropolisBlue",
            "Moderno",
            "Mulberry",
            "Office2010Blue",
            "Office2010Silver",
            "PlasticBlue",
            "RedWine",
            "SoftOrange",
            "Youthful"
        };

        public static String[] ThemeName = ThemesList.ToArray();

        internal static HttpContext Context
        {
            get
            {
                return HttpContext.Current;
            }
        }

        internal static HttpRequest Request
        {
            get
            {
                return Context.Request;
            }
        }

        public static String SelectTheme
        {
            get
            {
                if (Request.Cookies[CurrentKey] != null)
                    return HttpUtility.UrlDecode(Request.Cookies[CurrentKey].Value);
                return DefaultTheme;
            }
        }

        public static String DefaultMobileTheme
        {
            get
            {
                return ThemeName[6].ToString();
            }
        }
    }
}