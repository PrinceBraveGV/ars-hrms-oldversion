using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using AristaHRM.Models;

namespace AristaHRM.Controllers
{
    [RoutePrefix("Files")]
    public class FilesController : Controller
    {
        #region Daftar Variabel
        /* Variabel database */
        // variabel database SQL Server
        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public SqlCommand cmd;
        public SqlDataAdapter sda;
        public SqlDataReader sdr;
        public SqlBulkCopy sbc, sbc1, sbc2;
        public DataSet ds;
        public DataTable dt;

        // variabel database OLEDB
        public OleDbConnection oleconn;
        public OleDbCommand olecmd, olecmd2;
        public OleDbDataAdapter oleda;
        public OleDbDataReader oledr;
        public string oleconnstring;
        public string olecmdstring;

        #region Variabel Model

        /* Variabel string */
        public string NIK, No_Induk;
        public string Password;
        public string Nama_Karyawan;
        public string Departemen;
        public string Privilege;
        public string Pass_File;
        public string PathFile;
        public string Email, Email_Perusahaan;
        public string Nama_Atasan, Nama_Supervisor;

        #endregion

        /* Variabel objek */
        public HttpCookie record;
        public List<string> ListKaryawan;
        public string[] DaftarKaryawan;
        public string Nama_Tabel;
        public string Nama_Sumber;
        public string Param_File;
        public HttpPostedFileBase UploadCtrl;

        #endregion

        #region File Manager Interface
        public ActionResult FileManager()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    Session["ListViewMode"] = FileListView.Thumbnails;
                    return View(FileManagerHelper.RootDirectory);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        public ActionResult FileManager(FileListView ListViewMode)
        {
            SetPrivilege();
            Session["ListViewMode"] = ListViewMode;
            return View(FileManagerHelper.RootDirectory);
        }

        [ValidateInput(false)]
        public ActionResult FileManagerPartial()
        {
            return PartialView("_FileManager", FileManagerHelper.RootDirectory);
        }

        public ActionResult DownloadFiles()
        {
            return FileManagerExtension.DownloadFiles(FileManagerDownloadSettings.CreateDownloadSettings(), (string)FileManagerDownloadSettings.FileModel);
        }

        public ActionResult DownloadPictures()
        {
            return FileManagerExtension.DownloadFiles(FileManagerDownloadSettings.CreateDownloadSettings(), (string)FileManagerDownloadSettings.ImageModel);
        }
        #endregion

        #region Unduhan Berkas

        [AllowAnonymous]
        public ActionResult Download()
        {
            SetPrivilege();
            Session["ListViewMode"] = FileListView.Thumbnails;
            return View(FileManagerHelper.RootDirectory);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Download(FileListView ListViewMode)
        {
            SetPrivilege();
            Session["ListViewMode"] = ListViewMode;
            return View(FileManagerHelper.RootDirectory);
        }

        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult DownloadPartial()
        {
            return PartialView("_Download", FileManagerHelper.RootDirectory);
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
        #endregion
	}

    #region Download Helper
    public class FileManagerDownloadSettings
    {
        public static string RootDirectory = @"~/Files";
        public static string ImagesRootDirectory = @"~/Files/Images";
        public static string FileModel
        {
            get
            {
                return RootDirectory;
            }
        }

        public static string ImageModel
        {
            get
            {
                return ImagesRootDirectory;
            }
        }

        public static DevExpress.Web.Mvc.FileManagerSettings CreateDownloadSettings()
        {
            var settings = new DevExpress.Web.Mvc.FileManagerSettings();

            settings.SettingsEditing.AllowDownload = true;
            settings.Name = "fileManager";
            return settings;
        }
    }
    #endregion
}