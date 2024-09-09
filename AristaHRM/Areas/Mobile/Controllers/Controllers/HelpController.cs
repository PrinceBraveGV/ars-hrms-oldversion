using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AristaHRM.Areas.Mobile.Controllers
{
    [RouteArea("Mobile")]
    [RoutePrefix("Help")]
    public class HelpController : Controller
    {
        public ViewResult MobileGuide()
        {
            return View("MobileGuide");
        }



	}
}