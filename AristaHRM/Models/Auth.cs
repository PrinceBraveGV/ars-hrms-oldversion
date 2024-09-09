using System;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AristaHRM.Models;

namespace AristaHRM.Models
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private HRISContext DB = new HRISContext();
        private String _role;

        public String Role
        {
            get { return _role; }
            set { _role = value; }
        }

        // @Override
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("Konteks database harus diisi.");
            }

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (HttpContext.Current.User.IsInRole("Admin"))
            {
                return true;
            }

            if (String.IsNullOrEmpty(Role))
            {
                return true;
            }

            String role = "";

            try
            {
                role = DB.TM_Karyawans.FirstOrDefault(x => x.Nama_Karyawan == HttpContext.Current.User.Identity.Name).Privilege;
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    role = "Privilege role harus terisi: '" + e.Message + "'";
                }
                else
                {
                    role = "Kesalahan pada privilege role: '" + e.Message + "'";
                }
            }
            return base.AuthorizeCore(httpContext);
        }

        // @Override
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("Konteks filter harus diisi.");
            }

            base.OnAuthorization(filterContext);

            var referrer = filterContext.HttpContext.Request.UrlReferrer ?? new Uri("/Home/UserSession");
            String link = referrer.OriginalString;

            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("/Home/UserSession");
            }

            if (filterContext.Result is HttpUnauthorizedResult && filterContext.HttpContext.Request.IsAjaxRequest())
            {
                HttpContext.Current.Session["Message"] = "Request timed out.";
                filterContext.Result = new RedirectResult(link);
            }

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                HttpContext.Current.Session["Message"] = "Akses tidak diizinkan.";
                filterContext.Result = new RedirectResult(link);
            }
        }

        protected override HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext)
        {
            return base.OnCacheAuthorization(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
        }
    }

}