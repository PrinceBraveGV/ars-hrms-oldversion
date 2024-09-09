using System.Web.Mvc;

namespace AristaHRM.Areas.SPPD
{
    public class SPPDAreaRegistration : AreaRegistration 
    {
        // @Override
        public override string AreaName 
        {
            get 
            {
                return "SPPD";
            }
        }

        // @Override
        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SPPD_default",
                "SPPD/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "AristaHRM.Areas.SPPD.Controllers" }
            );
        }
    }
}