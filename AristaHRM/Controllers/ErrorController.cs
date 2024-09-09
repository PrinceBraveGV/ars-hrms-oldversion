using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AristaHRM.Controllers
{
    [RoutePrefix("Error")]
    public class ErrorController : Controller
    {
        public ViewResult Index()
        {
            return View("Error");
        }

        public ViewResult Forbidden()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = 403;
            return View("Error403");
        }

        public ViewResult PageNotFound()
        {
            return View("Error404");
        }

        public ViewResult InternalError()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = 500;
            return View("Error500");
        }

        public ViewResult CSRFError()
        {
            return View("ErrorCSRF");
        }

        /* General error */
        /// <summary>
        /// Menangani error level aplikasi, seperti pengajuan, persetujuan dll.
        /// </summary>
        /// <param name="src">Sumber kesalahan</param>
        /// <returns></returns>
        public ActionResult GeneralError(string src)
        {
            switch (src)
            {
                case "Pengajuan":
                    return RedirectToAction("PengajuanError");
                case "Resign":
                    return RedirectToAction("ResignError");
                default:
                    return View("Error");
            }
        }
	}
}