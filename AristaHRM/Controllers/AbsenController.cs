using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using AristaHRM.Models;
using AristaHRM.Models.Absensi;

namespace AristaHRM.Controllers
{
    [RoutePrefix("Absen")]
    public class AbsenController : Controller
    {
        #region Daftar Variabel
        public static String connstring = ConfigurationManager.ConnectionStrings["AbsenConnection"].ConnectionString;

        public SqlCommand cmd;
        public SqlDataAdapter sda;
        public SqlDataReader sdr;
        public DataSet ds;
        public DataTable dt;

        #region Variabel Model

        /* Variabel string */
        public string NIK;
        public string Nama_Karyawan;
        public string Privilege;

        #endregion

        /* Variabel objek */
        public HttpCookie record;

        #endregion

        #region Daftar Kehadiran
        public ActionResult DaftarHadir()
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
        #endregion

        #region Daftar Ketidakhadiran
        public ActionResult DaftarTidakHadir()
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
        #endregion

        #region Daftar Grup Shift
        public ActionResult GrupShift()
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

        public PartialViewResult GrupShiftPartial()
        {
            try
            {
                var model = Providers.GetGrupShift();

                if (model != null && model.Count() > 0)
                {
                    return PartialView("_GrupShift", model);
                }
                else
                {
                    return PartialView("_GrupShift");
                }
            }
            catch (Exception e)
            {
                ViewData["GridError"] = "Kesalahan pada proses data: '" + e.Message + "'";
                return PartialView("_GrupShift");
            }
        }
        #endregion

        #region Daftar Shift Kerja
        public ActionResult ShiftKerja()
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

        public PartialViewResult ShiftKerjaPartial()
        {
            try
            {
                var model = Providers.GetShift();

                if (model != null && model.Count() > 0)
                {
                    

                    return PartialView("_ShiftKerja", model);
                }
                else
                {
                    return PartialView("_ShiftKerja");
                }
            }
            catch (Exception e)
            {
                ViewData["GridError"] = "Kesalahan pada proses data: '" + e.Message + "'"; 
                return PartialView("_ShiftKerja");
            }
        }

        // edit shift kerja
        public PartialViewResult EditShift()
        {
            return PartialView("_ShiftKerja");
        }

        // hapus shift kerja
        public PartialViewResult HapusShift()
        {


            return PartialView("_ShiftKerja");
        }
        #endregion

        #region Shift Kerja Tetap
        public ActionResult ShiftTetap()
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

        public PartialViewResult ShiftTetapPartial()
        {
            try
            {
                var model = Providers.GetShiftTetap();
                if (model != null && model.Count() > 0)
                {
                    return PartialView("_ShiftTetap", model);
                }
                else
                {
                    return PartialView("_ShiftTetap");
                }
            }
            catch (Exception e)
            {
                ViewData["GridError"] = "Kesalahan pada proses data: '" + e.Message + "'";
                return PartialView("_ShiftTetap");
            }
        }

        // edit shift tetap
        public PartialViewResult EditShiftTetap()
        {


            return PartialView("_ShiftTetap");
        }

        // hapus shift tetap
        public PartialViewResult DelShiftTetap()
        {

            return PartialView("_ShiftTetap");
        }
        #endregion

        #region Shift Kerja Tidak Tetap
        public ActionResult ShiftTidakTetap()
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

        public PartialViewResult ShiftTDTPartial()
        {
            try
            {
                var model = Providers.GetShiftTdkTetap();
                if (model != null && model.Count() > 0)
                {
                    return PartialView("_ShiftTidakTetap", model);
                }
                else
                {
                    return PartialView("_ShiftTidakTetap");
                }
            }
            catch (Exception e)
            {
                ViewData["GridError"] = "Kesalahan pada proses data: '" + e.Message + "'";
                return PartialView("_ShiftTidakTetap");
            }
        }

        // edit shift tidak tetap
        public PartialViewResult EditShiftTidakTetap()
        {


            return PartialView("_ShiftTidakTetap");
        }

        // hapus shift tidak tetap
        public PartialViewResult DelShiftTidakTetap()
        {


            return PartialView("_ShiftTidakTetap");
        }
        #endregion

        #region Proses Time Sheet
        public ActionResult TimeSheet()
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
        #endregion

        #region Daftar Absensi
        public ActionResult ListAbsen()
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

        public PartialViewResult ListAbsenPartial()
        {


            return PartialView("_ListAbsen");
        }

        // edit jam absensi (jam masuk & pulang)
        public PartialViewResult EditAbsen()
        {



            return PartialView("_ListAbsen");
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
                                    Privilege = sdr["Privilege"].ToString().Trim();
                                }
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
        #endregion
    }
}