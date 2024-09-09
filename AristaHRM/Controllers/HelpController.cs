using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using AristaHRM.Models;

namespace AristaHRM.Controllers
{
    [RoutePrefix("Help")]
    public class HelpController : Controller
    {
        #region Daftar Variabel
        // variabel database

        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public SqlCommand cmd;
        public SqlDataReader sdr;

        #region Variabel Model
        // bagian variabel string
        public string NIK;
        public string Nama_Karyawan;
        public string Departemen;
        public string Privilege;

        // bagian variabel angka

        // bagian variabel tanggal

        #endregion

        // variabel lainnya
        HttpCookie record;

        #endregion

        /* BEGIN */

        #region FAQ Cuti
        // fungsi halaman 'frequently asked questions'
        public ActionResult FAQGuide()
        {
            SetPrivilege();
            return View();
        }
        public ActionResult FAQCuti()
        {
            SetPrivilege();
            return View();
        }
        #endregion

        #region User Guide
        // fungsi user guide
        public ActionResult UserGuide()
        {
            SetPrivilege();
            String Path = Server.MapPath("~/Files/Guide/UserGuide.pdf");
            ViewData["Path"] = Path;
            return View(ViewData);
        }
        #endregion

        #region Regional Guide
        public ActionResult RegionGuide()
        {
            SetPrivilege();
            return View();
        }
        #endregion

        #region About Us
        // fungsi company profile
        public ActionResult About()
        {
            SetPrivilege();
            return View();
        }
        #endregion

        #region Ketentuan Penggunaan
        // fungsi ketentuan penggunaan situs
        public ActionResult Terms()
        {
            SetPrivilege();
            return View();
        }
        #endregion

        #region Fungsi Lainnya
        public void SetPrivilege()
        {
            if (Request.IsAuthenticated)
            {
                // baca info cookie berisi NIK
                record = Request.Cookies["LoginID"];
                if (record != null)
                {
                    if (!string.IsNullOrEmpty(record.Values["NIK"]))
                    {
                        NIK = record.Values["NIK"].ToString().Trim();
                    }
                }

                // setting navigasi
                Nama_Karyawan = User.Identity.Name;
                using (var DB = new HRISContext())
                {
                    using (var conn = new SqlConnection(connstring))
                    {
                        Departemen = DB.TM_Karyawans.Where(x => x.NIK == NIK && x.Nama_Karyawan == Nama_Karyawan).Select(x => x.Departemen).FirstOrDefault();
                        Privilege = DB.TM_Karyawans.Where(x => x.NIK == NIK && x.Nama_Karyawan == Nama_Karyawan).Select(x => x.Privilege).FirstOrDefault();

                        if (String.IsNullOrEmpty(Privilege))
                        {
                            // gunakan koneksi langsung jika data context tidak menghasilkan apapun
                            cmd = new SqlCommand("SM_Karyawan", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                            cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";
                            try
                            {
                                if (conn.State != ConnectionState.Open) { conn.Open(); }
                                sdr = cmd.ExecuteReader();
                                while (sdr.Read())
                                {
                                    Departemen = sdr["Departemen"].ToString().Trim();
                                    Privilege = sdr["Privilege"].ToString().Trim();
                                }

                                ViewBag.Departemen = Departemen;
                                Session["Departemen"] = Departemen;
                                ViewBag.Privilege = Privilege;
                                Session["Privilege"] = Privilege;
                            }
                            catch (SqlException ex)
                            {
                                ViewData["ErrorMsg"] = "Terjadi kesalahan dalam proses data. Pesan: \"" + ex.Message + "\"";
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
                            }
                        }
                        else
                        {
                            ViewBag.Departemen = Departemen;
                            Session["Departemen"] = Departemen;

                            ViewBag.Privilege = Privilege;
                            Session["Privilege"] = Privilege;
                        }
                    }
                }

                HttpCookie auth = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (auth != null)
                {
                    FormsAuthenticationTicket TiketMasuk = null;
                    TiketMasuk = FormsAuthentication.Decrypt(auth.Value);

                    if (TiketMasuk != null && !TiketMasuk.Expired)
                    {
                        FormsAuthenticationTicket TiketBaru = TiketMasuk;
                        if (FormsAuthentication.SlidingExpiration)
                        {
                            TiketBaru = FormsAuthentication.RenewTicketIfOld(TiketMasuk);
                        }
                        string DataUser = TiketBaru.UserData;
                        string[] roles = DataUser.Split(',');

                        System.Web.HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(TiketBaru), roles);
                    }
                }
                FormsAuthentication.SetAuthCookie(Nama_Karyawan, createPersistentCookie: true);
            }
        }

        public static List<SelectListItem> GetListViewMode()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem() { Text = FileListView.Thumbnails.ToString(), Value = FileListView.Thumbnails.ToString(), Selected = true },
                new SelectListItem() { Text = FileListView.Details.ToString(), Value = FileListView.Details.ToString() }
            };
        }
        #endregion

        #region PDF Viewer
        public FileResult UserGuidePDF()
        {
            String Path = Server.MapPath("~/Files/Guide/UserGuide.pdf");
            byte[] PDFBytes = FileViewerHelper.GetBytesFromFile(Path);
            return File(PDFBytes, "application/pdf");
        }

        public FileResult AdminGuidePDF()
        {
            String Path = Server.MapPath("~/Files/Guide/AdminGuide.pdf");
            byte[] PDFBytes = FileViewerHelper.GetBytesFromFile(Path);
            return File(PDFBytes, "application/pdf");
        }
        #endregion
	}
}