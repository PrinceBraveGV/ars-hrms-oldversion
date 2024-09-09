using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AristaHRM.Areas.SPPD.Controllers
{
    [RouteArea("SPPD")]
    [RoutePrefix("Home")]
    public class HomeController : Controller
    {
        // Controller Area Khusus SPPD
        // Mengingat di area ini menggunakan web forms, semua rute di sini hanya melakukan redirect ke page web forms

        public ActionResult Index()
        {
            return Redirect("~/Areas/SPPD/Form/Home.aspx");
        }

        public ActionResult PengajuanSPD()
        {
            return RedirectToAction("UnderConstruction", "Notif", new { area = "" });
            // return Redirect("~/Areas/SPPD/Form/spd1.aspx");
        }

        public ActionResult LaporanSPD()
        {
            return RedirectToAction("UnderConstruction", "Notif", new { area = "" });
            // return Redirect("~/Areas/SPPD/Form/spd2.aspx");
        }

	}
}