using System.Web.Mvc;

namespace AristaHRM.Areas.Mobile
{
    public class MobileAreaRegistration : AreaRegistration 
    {
        // @Override
        public override string AreaName 
        {
            get 
            {
                return "Mobile";
            }
        }

        // @Override
        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Mobile_default",
                "Mobile/{controller}/{action}/{id}",
                new { area = "Mobile", controller = "Home", action = "Dashboard", id = UrlParameter.Optional },
                namespaces: new string[] { "AristaHRM.Areas.Mobile.Controllers" }
            );
        }
    }
}