using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AristaHRM.Models;
using System.Web.Script.Serialization;

namespace AristaHRM.Controllers
{
    public class SettingsController : Controller
    {
        #region Daftar Variabel
        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public SqlCommand cmd;
        public SqlDataAdapter sda;
        public SqlDataReader sdr;
        public DataSet ds;
        public DataTable dt;

        // variabel konteks - langsung dideklarasikan
        public HRISContext DB = new HRISContext();

        #region Variabel Model
        // variabel string
        public string NIK;
        public string Nama_Karyawan;
        public string Departemen;
        public string Privilege;
        public HttpCookie record;

        #endregion

        #endregion

        public ActionResult QueryViewer()
        {

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin")
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult QueryViewerPartial(QueryModel model, String Alamat_Server = null, String Nama_DB = null, String Query_String = null)
        {
            if (!String.IsNullOrEmpty(Alamat_Server) && !String.IsNullOrEmpty(Nama_DB) && !String.IsNullOrEmpty(Query_String))
            {
                // mencegah perintah asing selain dari SELECT
                if (Query_String.Contains("INSERT") || Query_String.Contains("DELETE") || Query_String.Contains("ALTER") || Query_String.Contains("CREATE") || Query_String.Contains("DROP"))
                {
                    ViewData["ErrorMsg"] = "Anda hanya dapat menjalankan query SELECT dalam form ini.";
                    return PartialView("_QueryViewer");
                }

                if (Alamat_Server.Contains("73"))
                {
                    model.User_Id = "sa1";
                    model.Password = "sqlserver";
                }
                else
                {
                    model.User_Id = "sa";
                    model.Password = "pass@word1";
                }

                String connection = Import.GenerateSqlConnection(Alamat_Server, Nama_DB, model.User_Id, model.Password);

                using (var conn = new SqlConnection(connection))
                {
                    try
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(Query_String, conn))
                        {
                            cmd.CommandType = CommandType.Text;

                            var da = new SqlDataAdapter();
                            var dt = new DataTable();

                            var sdr = cmd.ExecuteReader();
                            dt.Load(sdr); // muatan hasil query reader ke datatable

                            return PartialView("_QueryViewer", dt);
                        }
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Kesalahan pada proses pengambilan data: '" + e.Message + "'";
                        return PartialView("_QueryViewer");
                    }
                }
            }
            else
            {
                return PartialView("_QueryViewer");
            }
        }

        public async Task<ActionResult> RunAsync()
        {
            Task<ViewResult> task = Task.Run(() =>
            {
                // jalankan proses asynchronous di sini
                // terutama semua metode yang berakhiran "Async"

                // tentukan jenis return objek (ViewResult, FileResult, JsonResult dll)
                return View();
            });

            return await task;
        }

        public async Task<ActionResult> RunRedirectAsync()
        {
            Task<RedirectResult> task = Task.Run(() =>
            {
                var url = new UrlHelper(Request.RequestContext);

                return Redirect(url.Action("", "", new { id = "" }));
            });

            return await task;
        }

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

        #region Test Only
        public ActionResult DateConverter()
        {
            return View();
        }

        public ActionResult ConvertToHijri(string date)
        {
            var dat = ConvertDateCalendar(date, "Hijri", "en-US");
            return Json(dat, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConvertToGregorian(string date)
        {
            var dat = ConvertDateCalendar(date, "Gregorian", "en-US");

            return Json(dat, JsonRequestBehavior.AllowGet);
        }

        public static string ConvertDateCalendar(string DateConv, string Calendar, string DateLangCulture)
        {
            DateTimeFormatInfo DTFormat;
            var result = new DateTime();

            if (Calendar == "Hijri")
            {
                DateLangCulture = "ar-SA";
            }
            else
            {
                DateLangCulture = "en-US";
            }

            DTFormat = new System.Globalization.CultureInfo(DateLangCulture, false).DateTimeFormat;

            switch (Calendar)
            {
                case "Hijri":
                    DTFormat.Calendar = new System.Globalization.HijriCalendar();
                    DateLangCulture = "ar-SA";
                    result = DateTime.Parse(DateConv);
                    break;

                case "Gregorian":
                    DTFormat.Calendar = new System.Globalization.GregorianCalendar();
                    DateLangCulture = "en-US";
                    result = DateTime.ParseExact(DateConv, "dd/MM/yyyy", new CultureInfo("ar-SA"));
                    break;

                default:
                    return "";
            }

            return result.ToString("dd/MM/yyyy", DTFormat);
        }

        public ActionResult DataTableTest()
        {
            return View();
        }

        public JsonResult GetJson(string NIK)
        {
            using (var DB = new HRISContext())
            {
                var karyawan = (from kar in DB.TM_Karyawans
                                where kar.NIK == NIK
                                select new KaryawanModel()
                                {
                                    NIK = kar.NIK,
                                    Nama_Karyawan = kar.Nama_Karyawan,
                                    Perusahaan = kar.Perusahaan,
                                    Cabang = kar.Cabang,
                                    Jabatan = kar.Jabatan
                                });

                return Json(karyawan, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult DataTableTestPartial(string NIK)
        {
            var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK);

            var model = new KaryawanModel()
            {
                NIK = karyawan.NIK,
                Nama_Karyawan = karyawan.Nama_Karyawan,
                Perusahaan = karyawan.Perusahaan,
                Cabang = karyawan.Cabang,
                Jabatan = karyawan.Jabatan
            };

            return PartialView("_DataTableTest", model);
        }
        #endregion

    }
}