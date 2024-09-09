using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using AristaHRM.Models;

namespace AristaHRM.Areas.Mobile.Controllers
{
    [RouteArea("Mobile")]
    [RoutePrefix("Notif")]
    public class NotifController : Controller
    {
        #region Daftar Variabel

        /* Variabel database */
        // variabel database SQL Server
        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public SqlCommand cmd;
        public SqlDataAdapter sda;
        public SqlDataReader sdr;
        public SqlBulkCopy sbc;
        public DataSet ds;
        public DataTable dt;

        // variabel database OLEDB
        public OleDbConnection oleconn;
        public OleDbCommand olecmd;
        public OleDbDataAdapter oleda;
        public OleDbDataReader oledr;
        public string oleconnstring;
        public string Nama_Tabel;

        // variabel konteks - langsung dideklarasikan
        public HRISContext DB = new HRISContext();

        #region Variabel Model

        /* Variabel string */
        public string NIK;
        public string Nama_Karyawan;
        public string Password, Pass_Lama, Pass_Baru, Pass_Hash, Konfirmasi;
        public string Jenis_Kelamin;
        public string Tempat_Lahir;
        public string Email_Perusahaan, Email_Atasan, Email_Supervisor, Email_Advisor, Email_User;
        public string Alamat;
        public string Agama;
        public string Status_Nikah;
        public string Email, Email_Temp;
        public string Perusahaan, Cabang, Jabatan, Departemen, Nama_Atasan, Nama_Supervisor;
        public string Pembuat, Privilege, Privilege_S, Privilege_At;
        public string Notes, NotesBaru;

        // model khusus cuti
        public string ID_Cuti;
        public string Jenis_Cuti;
        public string Pemberi, Keperluan;
        public string Status_Cuti, Keterangan, No_Kontak;
        public string Nomor_Kontak;

        // model khusus perusahaan & cabang
        public string Kode_Perusahaan, Kode_Cabang;
        public string Nama_Perusahaan, Nama_Cabang;
        public string Lokasi, Kode_Singkat;

        // model khusus mutasi
        public string ID_Mutasi, ID_Pesan, Perusahaan_Baru, Cabang_Baru, Jabatan_Baru, Departemen_Baru, Atasan_Baru, Supervisor_Baru;
        public string Atasan_Ganti, Atasan_Asal, Area_Kerja, Staf_Ganti;

        // model khusus resign
        public string Alasan;

        // model string lainnya
        public string Reset_Token;
        public string Parameter;
        public string PhotoDir;
        public string Jenis_Anno;
        public string LastData;

        // model array
        public string[] ArrayData;
        public string[] ResultData;

        /* Variabel angka */
        public long ID_Baru;
        public int Masa_Cuti;
        public int Libur_Awal, Libur_Akhir, Sisa_Tahunan;
        public int Jatah_Cuti, Total_Cuti, Cuti_Tahunan, Cuti_Massal, Sisa_Cuti;
        public int Counter, Total_Count;
        public double Selisih_Awal, Selisih_Akhir;

        /* Variabel tanggal */
        public DateTime Tgl_Lahir;
        public DateTime Tgl_Masuk;
        public DateTime Periode_Awal, Periode_Akhir;
        public DateTime Tgl_Pengajuan, Tgl_Setuju;
        public DateTime Tgl_Mulai, Tgl_Selesai, Tgl_Awal, Tgl_Akhir;
        public DateTime Tgl_Mutasi;
        public DateTime Tgl_Resign;

        /* Variabel boolean */
        public bool AutoApprove;
        #endregion

        /* Variabel objek */
        public HttpCookie record;
        public StringBuilder Msg, Token;
        public MailMessage EmailRequest;
        public SmtpClient SMTPServer;
        public HttpPostedFileBase UploadCtrl;

        #endregion

        [AllowAnonymous]
        public ActionResult RegisterSukses()
        {
            ViewData["Nama_Karyawan"] = Session["Nama_Karyawan"].ToString().Trim();
            return View(ViewData);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult RegisterSukses(string returnurl)
        {
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ResetSukses()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetSukses(string returnurl)
        {
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ResetBatal()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetBatal(string returnurl)
        {
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult KonfirmasiSetting()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult KonfirmasiSetting(string returnurl)
        {
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult KonfirmasiReset()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult KonfirmasiReset(string returnurl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult MultiApprovalSukses()
        {
            return View("MultiApprovalSukses");
        }

        public ActionResult MultiApprovalError()
        {
            return View("MultiApprovalError");
        }

        public ActionResult AddSukses()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddSukses(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditSukses()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditSukses(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult BatalCutiSukses()
        {
            ViewData["ID_Cuti"] = Session["ID_Cuti"].ToString().Trim();
            return View(ViewData);
        }

        [HttpPost]
        public ActionResult BatalCutiSukses(string returnurl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult MutasiError()
        {
            return View();
        }

        public ActionResult MutasiSukses()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MutasiSukses(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ResignError()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResignError(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ResignSukses()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResignSukses(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ResignBatal()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResignBatal(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        #region Tampilan Layar Hasil Proses

        public ActionResult PengajuanSukses()
        {
            ViewData["ID_Cuti"] = Session["ID_Cuti"].ToString().Trim();
            return View(ViewData);
        }

        [HttpPost]
        public ActionResult PengajuanSukses(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult PengajuanError()
        {
            ViewData["ID_Cuti"] = Session["ID_Cuti"].ToString().Trim();
            return View(ViewData);
        }

        [HttpPost]
        public ActionResult PengajuanError(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ApprovalError()
        {
            Status_Cuti = Session["Status_Cuti"].ToString();
            if (Status_Cuti == "Disetujui")
            {
                return RedirectToAction("ApprovalSukses", "Notif");
            }
            else
            {
                return RedirectToAction("ApprovalReject", "Notif");
            }
        }

        public ActionResult ApprovalSukses()
        {
            ViewData["Jenis_Cuti"] = Session["Jenis_Cuti"].ToString().Trim();
            ViewData["ID_Cuti"] = Session["ID_Cuti"].ToString().Trim();
            ViewData["Nama_Karyawan"] = Session["Nama_Karyawan"].ToString().Trim();
            return View(ViewData);
        }

        [HttpPost]
        public ActionResult ApprovalSukses(string returnUrl)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ApprovalReject()
        {
            SetPrivilege();
            var model = new CutiModel();
            ViewData["Jenis_Cuti"] = Session["Jenis_Cuti"].ToString().Trim();
            ViewData["ID_Cuti"] = Session["ID_Cuti"].ToString().Trim();
            ViewData["NIK"] = Session["NIK"].ToString().Trim();
            ViewData["Nama_Karyawan"] = Session["Nama_Karyawan"].ToString().Trim();
            return View(model);
        }

        [HttpPost]
        public ActionResult ApprovalReject(CutiModel model)
        {
            SqlCommand cmd;
            // alasan penolakan harus diisi
            if (!string.IsNullOrEmpty(model.Alasan))
            {
                using (var conn = new SqlConnection(connstring))
                {
                    // masukkan alasan penolakan cuti
                    ID_Cuti = Session["ID_Cuti"].ToString().Trim();
                    NIK = Session["NIK"].ToString().Trim();
                    Nama_Karyawan = Session["Nama_Karyawan"].ToString().Trim();
                    Jenis_Cuti = Session["Jenis_Cuti"].ToString().Trim();

                    Alasan = model.Alasan.ToString().Trim();

                    // bentuk stored procedure
                    if (Jenis_Cuti == "Khusus")
                    {
                        cmd = new SqlCommand("SP_ApproveKhusus", conn);
                    }
                    else
                    {
                        cmd = new SqlCommand("SP_ApproveTahunan", conn);
                    }
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Alasan", SqlDbType.NVarChar).Value = Alasan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Alasan";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // tahan sesi
                ViewData["ErrorMsg"] = "Harap mengisi alasan penolakan cuti sebelum melanjutkan proses berikutnya.";
                ViewData["Jenis_Cuti"] = Session["Jenis_Cuti"].ToString().Trim();
                ViewData["ID_Cuti"] = Session["ID_Cuti"].ToString().Trim();
                ViewData["NIK"] = Session["NIK"].ToString().Trim();
                ViewData["Nama_Karyawan"] = Session["Nama_Karyawan"].ToString().Trim();
                return View(model);
            }
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