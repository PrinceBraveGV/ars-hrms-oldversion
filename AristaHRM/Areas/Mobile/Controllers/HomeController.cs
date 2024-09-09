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

namespace AristaHRM.Areas.Mobile.Controllers
{
    [RouteArea("Mobile")]
    [RoutePrefix("Home")]
    public class HomeController : Controller
    {
        #region Daftar Variabel
        /* Variabel database */
        // variabel database SQL Server
        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public SqlCommand cmd;
        public SqlDataAdapter sda;
        public SqlDataReader sdr;
        public DataSet ds;
        public DataTable dt;

        // variabel konteks - langsung dideklarasikan
        public HRISContext DB = new HRISContext();

        #region Variabel Model

        /* Variabel string */
        public string NIK, No_Induk;
        public string Nama_Karyawan;
        public string Password, Pass_Hash, Pass_Baru, Konfirmasi;
        public string Jenis_Kelamin;
        public string Tempat_Lahir;
        public string Alamat;
        public string Agama;
        public string Status_Nikah;
        public string Email, Email_Temp;
        public string Email_Perusahaan, Provinsi, Kota;
        public string Perusahaan, Cabang, Jabatan, Departemen;
        public string Nama_Atasan, Nama_Supervisor;
        public string Pembuat, Privilege, Status_Karyawan, Status_Kerja, Area_Kerja;
        public string ID_Cuti, NIK_Baru, Proses;
        public string Reset_Token;
        public string Parameter, Parameter2;
        public string ServerIP;

        /* Variabel angka */
        public int AktifLogin;
        public long ID_Baru;
        public int Coba;
        public int Email_Valid;
        public int Tanggal, Bulan, Tahun;
        public int Counter, Total_Count;

        /* Variabel tanggal */
        public DateTime Tgl_Lahir;
        public DateTime Tgl_Masuk;
        public DateTime Tgl_Resign;
        public DateTime Tgl_Awal;
        public DateTime Periode_Awal;
        public DateTime Periode_Akhir;
        public DateTime Last_Session = DateTime.Now;
        #endregion

        /* Variabel objek */
        public HttpCookie record;
        public StringBuilder Msg, Token;
        public MailMessage EmailRequest;
        public SmtpClient SMTPServer;
        public bool Remember;

        // helper
        public EmailHelper MailHelper = new EmailHelper();
        #endregion

        public ActionResult Dashboard()
        {
            SetPrivilege();

            if (!Request.IsAuthenticated) {
                return View("MobileIndex");
            }

            if (ViewBag.Privilege == "Admin")
            {
                return RedirectToAction("AdminMenu", "Admin", new { area = "Mobile" });
            }
            else if (ViewBag.Privilege == "Manager")
            {
                return RedirectToAction("ManagerMenu", "Manager", new { area = "Mobile" });
            }
            else if (ViewBag.Privilege == "Supervisor") {
                return RedirectToAction("SuperMenu", "Supervisor", new { area = "Mobile" });
            }
            else
            {
                return RedirectToAction("UserMenu", "User", new { area = "Mobile" });
            }
        }

        public ActionResult Login()
        {
            return View("MobileLogin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (!string.IsNullOrEmpty(model.NIK))
            {
                NIK = model.NIK.ToString().Trim();
            }
            else
            {
                return View("MobileLogin", model);
            }

            if (Session["NIK"] != null)
            {
                Parameter = Session["NIK"].ToString().Trim();
            }

            // periksa dahulu apakah user sudah 3x salah mengisi data login
            if (Parameter == NIK && Session["Coba"] != null && Convert.ToInt32(Session["Coba"]) >= 3)
            {
                Tgl_Awal = Convert.ToDateTime(Session["Tgl_Awal"]);
                if (DateTime.Now.Subtract(Tgl_Awal).Minutes >= 3)
                {
                    Coba = 0;
                    Session["Coba"] = Coba;
                }
                else
                {
                    ViewData["ErrorMsg"] = "Anda sudah tiga kali salah memasukkan informasi login. Saat ini login anda dalam keadaan terproteksi, mohon mencoba lagi setelah 3 menit.";
                }
                SetPrivilege();
                return View("MobileLogin", model);
            }

            if (!string.IsNullOrEmpty(model.NIK) && !string.IsNullOrEmpty(model.Password))
            {
                // tahap 1: transfer model ke variabel kendali
                Password = FormsAuthentication.HashPasswordForStoringInConfigFile(model.Password.ToString(), "SHA1");
                Remember = (bool)model.Remember;

                // tahap 2: penyusunan stored procedure
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Password;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Login";

                    // tahap 3: eksekusi stored procedure langsung ke SQL Server DB
                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            // informasi yang dibutuhkan untuk proses berikutnya
                            No_Induk = sdr["NIK"].ToString().Trim();
                            Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            Pass_Hash = sdr["Password"].ToString().Trim();
                            Jabatan = sdr["Jabatan"].ToString().Trim();
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Email_Valid = Convert.ToInt32(sdr["Email_Valid"]);
                            Status_Kerja = sdr["Status_Kerja"].ToString().Trim();
                            Privilege = sdr["Privilege"].ToString().Trim();
                            if (!Convert.IsDBNull(sdr["Tgl_Resign"]))
                            {
                                Parameter = sdr["Tgl_Resign"].ToString();
                                if (!string.IsNullOrEmpty(Parameter))
                                {
                                    Tgl_Resign = Convert.ToDateTime(Parameter);
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah dalam proses login ke database. Jika masalah berlanjut harap segera menghubungi admin/support. Pesan: \"" + ex.Message + "\"";
                        return View("MobileLogin", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses login. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View("MobileLogin", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                // tahap 4: periksa kebenaran input
                if (NIK == No_Induk && Password == Pass_Hash)
                {
                    // 4-A: user ID & password benar

                    // cek apakah ybs sudah resign dengan persetujuan HRD
                    if ((Status_Kerja == "Tidak Aktif" || Status_Kerja == "Resign") && (Tgl_Resign != null && Tgl_Resign != DateTime.MinValue && DateTime.Now.Date >= Tgl_Resign))
                    {
                        ViewData["ErrorMsg"] = "NIK yang digunakan untuk login sudah resign dari perusahaan. Mohon periksa kembali status NIK anda.";
                        return View("MobileLogin", model);
                    }

                    if (Session["User"] != null)
                    {
                        Parameter = Session["User"].ToString().Trim();
                    }

                    /*
                    // mencegah user yang sama login kembali ke sistem jika user ybs sudah login
                    if (Parameter == NIK)
                    {
                        ViewData["ErrorMsg"] = "Mohon maaf, saat ini NIK yang bersangkutan sedang login pada sistem. Segera hubungi admin jika anda tidak sedang login dengan NIK ybs.";
                        return View(model);
                    }
                    */

                    TimeSpan SessionTimeOut = new TimeSpan(0, 0, System.Web.HttpContext.Current.Session.Timeout, 0, 0);
                    string SessionKey = NIK;
                    string SessionUser = Convert.ToString(System.Web.HttpContext.Current.Cache[SessionKey]);

                    /*
                    if (!string.IsNullOrEmpty(SessionUser))
                    {
                        ViewData["ErrorMsg"] = "Mohon maaf, saat ini NIK yang bersangkutan sedang login pada sistem. Segera hubungi admin jika anda tidak sedang login dengan NIK ybs.";
                        return View(model);
                    }
                    */

                    // apabila sesi dalam keadaan kosong, set cookie baru
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

                    record = new HttpCookie("LoginID");
                    record["NIK"] = NIK;
                    record["Nama_Karyawan"] = Nama_Karyawan;
                    record["Jabatan"] = Jabatan;
                    record["Privilege"] = Privilege;
                    record["Remember"] = Remember.ToString();
                    record["Departemen"] = Departemen;
                    record["Email_Valid"] = Email_Valid.ToString();
                    Session["User"] = NIK;
                    Session["Privilege"] = Privilege;
                    Session["ViewMode"] = "Desktop";

                    if (Email_Valid == 0)
                    {
                        Session["Profil"] = "Profil";
                    }
                    else
                    {
                        Session["Profil"] = null;
                    }

                    System.Web.HttpContext.Current.Cache.Insert(SessionKey, SessionKey, null, DateTime.MaxValue, SessionTimeOut, System.Web.Caching.CacheItemPriority.NotRemovable, null);

                    if (Remember == true)
                    {
                        record.Expires = DateTime.Now.AddDays(30d);
                    }
                    else
                    {
                        record.Expires = DateTime.Now.AddDays(1d);
                    }
                    Response.Cookies.Add(record);

                    if (!Request.IsAuthenticated)
                    {
                        AktifLogin = 1;
                    }
                    else
                    {
                        AktifLogin = 0;
                    }

                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SM_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = No_Induk;
                        cmd.Parameters.Add("@Last_Login", SqlDbType.DateTime).Value = Last_Session.Date;
                        cmd.Parameters.Add("@Last_Session", SqlDbType.DateTime).Value = Last_Session;
                        cmd.Parameters.Add("@Aktif_Login", SqlDbType.Int).Value = AktifLogin; // mencegah login
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Login Update";
                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah dalam proses login ke database. Jika masalah berlanjut harap segera menghubungi admin/support. Pesan: \"" + ex.Message + "\"";
                            return View("MobileLogin", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses login. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            return View("MobileLogin", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }

                    // tahap terakhir: redirect ke halaman indeks dengan status user terotentikasi
                    ViewBag.Privilege = Privilege;
                    Session["Privilege"] = Privilege;
                    Session["Email_Valid"] = Email_Valid;
                    return RedirectToAction("LoginSukses", "Home", new { area = "Mobile" });

                    /* akhir prosedur 4-A */
                }
                else
                {
                    // 4-B: user ID atau password salah
                    ViewData["ErrorMsg"] = "Data login atau password yang anda masukkan salah atau keliru. Silakan mencoba kembali.";
                    if (Session["Coba"] != null && Convert.ToInt32(Session["Coba"]) != 0)
                    {
                        Coba = Convert.ToInt32(Session["Coba"]);
                        Coba++;
                        Tgl_Awal = DateTime.Now;
                        Session["Tgl_Awal"] = Tgl_Awal;

                        if (Coba >= 3 && Session["NIK"].ToString() == NIK)
                        {
                            Session["Coba"] = Coba;
                            ViewData["ErrorMsg"] = "Anda sudah tiga kali salah memasukkan informasi login. Saat ini login anda dalam keadaan terproteksi, mohon mencoba lagi setelah 3 menit.";
                        }
                        else if (Session["NIK"].ToString() != NIK)
                        {
                            NIK = model.NIK.ToString().Trim();
                            No_Induk = NIK;
                            Session["NIK"] = NIK;

                            Coba = 0;
                            Coba++;
                            Session["Coba"] = Coba;
                            Tgl_Awal = DateTime.Now;
                            Session["Tgl_Awal"] = Tgl_Awal;
                        }
                        else
                        {
                            Session["Coba"] = Coba;
                            Tgl_Awal = DateTime.Now;
                            Session["Tgl_Awal"] = Tgl_Awal;
                        }
                    }
                    else
                    {
                        NIK = model.NIK.ToString().Trim();
                        No_Induk = NIK;
                        Session["NIK"] = NIK;

                        Coba = 0;
                        Coba++;
                        Session["Coba"] = Coba;
                        Tgl_Awal = DateTime.Now;
                        Session["Tgl_Awal"] = Tgl_Awal;
                    }
                    return View("MobileLogin", model);

                    /* akhir prosedur 4-B */
                }
            }

            return View("MobileLogin", model);
        }

        #region Logout
        [AllowAnonymous]
        public ActionResult Logout()
        {
            // fungsi logout
            // menghapus semua cookie dan sesi untuk user yang sedang login

            record = Request.Cookies["LoginID"];
            if (record != null)
            {
                if (!string.IsNullOrEmpty(record.Values["NIK"]))
                {
                    NIK = record.Values["NIK"].ToString();
                }
            }

            // login tidak aktif
            AktifLogin = 0;

            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SM_Karyawan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = No_Induk;
                cmd.Parameters.Add("@Last_Login", SqlDbType.DateTime).Value = Last_Session.Date;
                cmd.Parameters.Add("@Last_Session", SqlDbType.DateTime).Value = DateTime.Now.ToString();
                cmd.Parameters.Add("@Aktif_Login", SqlDbType.Int).Value = AktifLogin; // mencegah login
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Login Update";
                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan saat memproses permintaan logout. Pesan: \"" + ex.Message + "\"";
                    return View("MobileLogout", ViewData);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses logout. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    return View("MobileLogout", ViewData);
                }
                finally
                {
                    conn.Close();
                }
            }

            // hapus semua informasi user yang sedang login
            FormsAuthentication.SignOut();

            TimeSpan SessionTimeOut = new TimeSpan(0, 0, System.Web.HttpContext.Current.Session.Timeout, 0, 0);
            string SessionKey = NIK;

            if (SessionKey != null)
            {
                System.Web.HttpContext.Current.Cache.Insert(SessionKey, string.Empty, null, DateTime.MaxValue, SessionTimeOut, System.Web.Caching.CacheItemPriority.NotRemovable, null);
            }

            Session.Abandon();
            Response.Clear();
            Response.Cache.SetExpires(DateTime.Now.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return View("MobileLogout");
        }
        #endregion

        #region Registrasi Karyawan
        [AllowAnonymous]
        public ActionResult Register()
        {
            string Nama_Perusahaan = (Request.Params["Perusahaan"] ?? string.Empty).ToString();
            string Nama_Cabang = (Request.Params["Cabang"] ?? string.Empty).ToString();
            return View(new KaryawanModel { Perusahaan = Nama_Perusahaan, Cabang = Nama_Cabang });
        }

        [AllowAnonymous]
        public ActionResult CabangPartial()
        {
            string Nama_Perusahaan = (Request.Params["Perusahaan"] ?? string.Empty).ToString();
            return PartialView("_Cabang", new KaryawanModel { Perusahaan = Nama_Perusahaan });
        }

        [AllowAnonymous]
        public ActionResult AtasanPartial()
        {
            string Departemen = (Request.Params["Departemen"] ?? string.Empty).ToString();
            return PartialView("_Atasan", new KaryawanModel { Departemen = Departemen });
        }

        [AllowAnonymous]
        public ActionResult SupervisorPartial()
        {
            string Departemen = (Request.Params["Departemen"] ?? string.Empty).ToString();
            return PartialView("_Supervisor", new KaryawanModel { Departemen = Departemen });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(KaryawanModel model)
        {
            if (ModelState.IsValid)
            {
                // tahap 1: transfer model ke variabel kendali
                Nama_Karyawan = model.Nama_Karyawan.ToString().Trim();
                Status_Karyawan = model.Status_Karyawan.ToString().Trim();

                // bersihkan keadaan parameter
                Parameter = null;
                Parameter2 = null;

                // periksa keadaan karyawan duplikat
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_CheckDupe", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Parameter", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Karyawan";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            if (sdr.HasRows == true)
                            {
                                Parameter = sdr["Nama_Karyawan"].ToString().Trim();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        return View("MobileRegister", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View("MobileRegister", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    if (!string.IsNullOrEmpty(Parameter))
                    {
                        ViewData["ErrorMsg"] = "Nama karyawan yang anda masukkan telah terdaftar pada sistem. Silakan input karyawan lain atau edit data yang telah ada.";
                        SetPrivilege();
                        return View("MobileRegister", model);
                    }

                    // buat NIK baru berdasarkan posisi NIK terakhir
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Status_Karyawan", SqlDbType.NVarChar).Value = Status_Karyawan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "NIK Baru";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            NIK_Baru = sdr["NIK_Baru"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan saat memproses data registrasi. Pesan: \"" + ex.Message + "\"";
                        return View("MobileRegister", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses registrasi. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View("MobileRegister", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                if (string.IsNullOrEmpty(NIK_Baru))
                {
                    NIK_Baru = "00000";
                }

                for (int i = 0; i < NIK_Baru.Length; i++)
                {
                    if (Char.IsDigit(NIK_Baru[i]))
                        Proses += NIK_Baru[i];
                }

                if (Proses.Length > 0)
                {
                    ID_Baru = Convert.ToInt32(Proses);
                }
                else
                {
                    ID_Baru = 1;
                }

                ID_Baru++;

                NIK = ID_Baru.ToString("00000").Trim();

                // --> buat password standar = NIK karyawan yang mendaftar
                Password = FormsAuthentication.HashPasswordForStoringInConfigFile(NIK, "SHA1");

                // tahap 2: cek keberadaan user dengan nama yang sama pada database (karena NIK baru dikeluarkan secara otomatis oleh sistem)
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            if (sdr["NIK"] != null)
                            {
                                No_Induk = sdr["NIK"].ToString().Trim();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan saat memproses data registrasi. Pesan: \"" + ex.Message + "\"";
                        return View("MobileRegister", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses registrasi. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View("MobileRegister", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                // tahap 3: susun stored procedure jika user yang dicari memang baru mendaftar
                if (No_Induk == null)
                {
                    // tahap 3-A: transfer model ke variabel kendali
                    Jenis_Kelamin = model.Jenis_Kelamin.ToString().Trim();
                    Tempat_Lahir = model.Tempat_Lahir.ToString().Trim();
                    Tgl_Lahir = (DateTime)model.Tgl_Lahir;
                    Provinsi = model.Provinsi.ToString().Trim();
                    Kota = model.Kota.ToString().Trim();
                    Alamat = model.Alamat.ToString().Trim();
                    Agama = model.Agama.ToString().Trim();
                    Status_Nikah = model.Status_Nikah.ToString().Trim();
                    Email = model.Email.ToString().Trim();
                    if (!string.IsNullOrEmpty(model.Email_Perusahaan))
                    {
                        Email_Perusahaan = model.Email_Perusahaan.ToString().Trim();
                    }
                    Perusahaan = model.Perusahaan.ToString().Trim();
                    Cabang = model.Cabang.ToString().Trim();
                    Jabatan = model.Jabatan.ToString().Trim();
                    Departemen = model.Departemen.ToString().Trim();
                    Nama_Atasan = model.Nama_Atasan.ToString().Trim();
                    if (!string.IsNullOrEmpty(model.Nama_Supervisor))
                    {
                        Nama_Supervisor = model.Nama_Supervisor.ToString().Trim();
                    }
                    Status_Karyawan = model.Status_Karyawan.ToString();
                    Tgl_Masuk = model.Tgl_Masuk.Date;
                    Pembuat = User.Identity.Name.ToString();
                    Area_Kerja = model.Area_Kerja.ToString().Trim();

                    // seleksi level user
                    // dapat diubah sesuai ketentuan perusahaan melalui hard code
                    switch (Jabatan)
                    {
                        case "President Director":
                        case "Komisaris":
                        case "Board of Directors":
                        case "Operation Director":
                        case "Managing Director":
                        case "Marketing Director":
                            Privilege = "Admin";
                            break;
                        case "Operation Manager":
                        case "General Manager":
                        case "Deputy General Manager":
                        case "Deputy Operation Manager":
                        case "Manager":
                        case "Sales Manager":
                        case "Service Manager":
                        case "Marketing Manager":
                        case "Kepala Cabang":
                            Privilege = "Manager";
                            break;
                        case "Supervisor":
                        case "Supervisor Admin Area":
                        case "Supervisor Service Area":
                        case "Service Advisor":
                        case "Service Supervisor":
                        case "Kepala Bengkel":
                            Privilege = "Supervisor";
                            break;
                        default:
                            Privilege = "Staff";
                            break;
                    }

                    // tahap 4-A: susun stored procedure untuk database
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SM_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Password; // password standar awal
                        cmd.Parameters.Add("@Jenis_Kelamin", SqlDbType.NVarChar).Value = Jenis_Kelamin;
                        cmd.Parameters.Add("@Tempat_Lahir", SqlDbType.NVarChar).Value = Tempat_Lahir;
                        cmd.Parameters.Add("@Tgl_Lahir", SqlDbType.Date).Value = Tgl_Lahir;
                        cmd.Parameters.Add("@Provinsi", SqlDbType.NVarChar).Value = Provinsi;
                        cmd.Parameters.Add("@Kota", SqlDbType.NVarChar).Value = Kota;
                        cmd.Parameters.Add("@Alamat", SqlDbType.NVarChar).Value = Alamat;
                        cmd.Parameters.Add("@Agama", SqlDbType.NVarChar).Value = Agama;
                        cmd.Parameters.Add("@Status_Nikah", SqlDbType.NVarChar).Value = Status_Nikah;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = Email;
                        if (!string.IsNullOrEmpty(Email_Perusahaan))
                        {
                            cmd.Parameters.Add("@Email_Perusahaan", SqlDbType.NVarChar).Value = Email_Perusahaan;
                        }
                        cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                        cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                        cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                        cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                        cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                        if (!string.IsNullOrEmpty(Nama_Supervisor))
                        {
                            cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
                        }
                        cmd.Parameters.Add("@Status_Karyawan", SqlDbType.NVarChar).Value = Status_Karyawan;
                        cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.Date).Value = Tgl_Masuk;
                        cmd.Parameters.Add("@Pembuat", SqlDbType.NVarChar).Value = Pembuat;
                        cmd.Parameters.Add("@Privilege", SqlDbType.NVarChar).Value = Privilege;
                        cmd.Parameters.Add("@Email_Valid", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Area_Kerja", SqlDbType.NVarChar).Value = Area_Kerja;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                        // --> uji koneksi
                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            ViewBag.NIK = NIK;
                            Session["NIK"] = NIK;
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan saat memproses data registrasi. Pesan: \"" + ex.Message + "\"";
                            return View("MobileRegister", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses registrasi. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            return View("MobileRegister", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }

                    Periode_Awal = DateTime.Now;
                    Periode_Akhir = DateTime.Now.AddYears(1);

                    // dapatkan nilai ID dari tabel sisa cuti
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Sisa_Cuti", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Max";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            sdr = cmd.ExecuteReader();
                            while (sdr.Read())
                            {
                                ID_Cuti = sdr[0].ToString().Trim();
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data registrasi. Pesan: \"" + ex.Message + "\"";
                            return View("MobileRegister", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengolahan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            return View("MobileRegister", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }

                        if (!string.IsNullOrEmpty(ID_Cuti))
                        {
                            ID_Baru = Convert.ToInt32(ID_Cuti);
                        }
                        else
                        {
                            ID_Baru = 0;
                        }

                        ID_Baru++;
                        ID_Cuti = ID_Baru.ToString("0000000000000000");

                        // tahap 5-A: isi status cuti dan sisa cuti dengan angka default user, diambil dari tabel setting cuti
                        cmd = new SqlCommand("SP_Sisa_Cuti", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                        cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                        cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                        cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                        cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                        cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                        cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.Date).Value = Tgl_Masuk;
                        cmd.Parameters.Add("@Periode_Awal", SqlDbType.DateTime).Value = Periode_Awal;
                        cmd.Parameters.Add("@Periode_Akhir", SqlDbType.DateTime).Value = Periode_Akhir;
                        cmd.Parameters.Add("@Jatah_Cuti", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Total_Cuti", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Cuti_Pribadi", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Cuti_Khusus", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Cuti_Massal", SqlDbType.Int).Value = 4; // perubahan untuk tahun berjalan
                        cmd.Parameters.Add("@Cuti_Hangus", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Tahun_Hangus", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Sisa_Cuti", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                            return View("MobileRegister", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            return View("MobileRegister", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }

                    // tahap 6-A: redirect ke rutin set password via email

                    // setting SMTP server
                    try
                    {
                        // konstruksi isi email
                        Msg = new StringBuilder();
                        Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                        Msg.Append("<br />");
                        Msg.Append("Yth. " + Nama_Karyawan + ",");
                        Msg.Append("<br /><br />");
                        Msg.Append("Anda baru saja terdaftar pada sistem kami.<br />");
                        Msg.Append("Untuk mengamankan akun anda, anda disarankan untuk melakukan pengaturan password secara mandiri.");
                        Msg.Append("<br /><br />");
                        Msg.Append("Password yang telah diisi dapat langsung digunakan untuk login.");
                        Msg.Append("<br /><br />");
                        Msg.Append("Silakan klik link berikut ini untuk melakukan setting password: ");
                        Msg.Append("<br /><br />");
                        Msg.Append("<a href=\"" + string.Format("http://" + ServerIP + "/Home/InputPassword/{0}", NIK) + "\">" + string.Format("http://" + ServerIP + "/Home/InputPassword/{0}", NIK) + "</a>"); // => ubah localhost dengan alamat situs remote SMTP server
                        Msg.Append("<br /><br />");
                        Msg.Append("Apabila anda tidak melakukan prosedur setting password ini, sistem otomatis akan memilih NIK anda sebagai password default untuk login.<br /><br />");
                        Msg.Append("Atas perhatiannya kami mengucapkan terima kasih.");
                        Msg.Append("<br /><br /><br /><br />");
                        Msg.Append("Jakarta, " + string.Format("{0:dd MMMM yyyy}", DateTime.Now));
                        Msg.Append("<br /><br />");
                        Msg.Append("Hormat kami,");
                        Msg.Append("<br /><br />");
                        Msg.Append("<br /><br />");
                        Msg.Append("<u>Human Resources Department ARISTA Group</u>");
                        Msg.Append("</span>");

                        // tentukan email mana yang valid
                        if (!string.IsNullOrEmpty(Email_Perusahaan))
                        {
                            Email_Temp = Email_Perusahaan;
                        }
                        else
                        {
                            Email_Temp = Email;
                        }

                        Helpers.SendSettingPassword(Msg.ToString(), Email_Temp, Email_Perusahaan, Email);
                    }
                    catch (SmtpException smtpex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam proses pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator terkait untuk melaporkan masalah yang terjadi. Pesan: \"" + smtpex.Message + "\"";
                        return View("MobileRegister", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator terkait untuk melaporkan masalah. Pesan: \"" + e.Message + "\"";
                        return View("MobileRegister", model);
                    }

                    // return RedirectToAction("InputPassword", "Home"); // versi final harus diganti dengan baris sbb:
                    return RedirectToAction("KonfirmasiSetting", "Notif", new { area = "Mobile" });
                }
                else
                {
                    // tahap 3-B: user ybs sudah terdaftar sebelumnya pada database
                    ViewData["ErrorMsg"] = "NIK anda sudah terdaftar pada sistem. Silakan hubungi administrator terkait untuk dukungan teknis registrasi.";
                    return View("MobileRegister", model);
                }
            }
            else
            {
                string Nama_Perusahaan = (Request.Params["Perusahaan"] ?? string.Empty).ToString();
                string Nama_Cabang = (Request.Params["Cabang"] ?? string.Empty).ToString();
                return View(new KaryawanModel { Perusahaan = Nama_Perusahaan, Cabang = Nama_Cabang });
            }
        }
        #endregion

        #region Setting Password
        [AllowAnonymous]
        public ActionResult InputPassword(string id, PasswordModel model)
        {
            SetPrivilege();
            if (Session["NIK"] != null)
            {
                model.NIK = Session["NIK"].ToString().Trim();
            }

            // jika menerima parameter ID berupa NIK
            if (!string.IsNullOrEmpty(id))
            {
                model.NIK = id.ToString();
            }

            return View("MobileInputPassword", model);
        }

        // jika user menekan tombol proses
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult InputPassword(PasswordModel model, string returnUrl)
        {
            SetPrivilege();
            // mendapatkan nilai NIK dari sesi maupun model
            if (model.NIK == null)
            {
                NIK = Session["NIK"].ToString().Trim();
            }
            else
            {
                NIK = model.NIK.ToString().Trim();
            }

            // temukan nama karyawan
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SM_Karyawan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data registrasi. Pesan: \"" + ex.Message + "\"";
                    return View("MobileInputPassword", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses setting password. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    return View("MobileInputPassword", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            Session["Nama_Karyawan"] = Nama_Karyawan;

            // tahap 1: pengecekan input password
            if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.Konfirmasi))
            {
                // tahap 2-A: jika ada password dalam text input
                Pass_Baru = model.Password.ToString().Trim();
                Konfirmasi = model.Konfirmasi.ToString().Trim();

                // samakan kedua isian password
                if (Konfirmasi == Pass_Baru)
                {
                    // tahap 3-A: susun stored procedure
                    Password = FormsAuthentication.HashPasswordForStoringInConfigFile(Pass_Baru, "SHA1");
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SM_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Pass_Hash;
                        cmd.Parameters.Add("@Email_Valid", SqlDbType.NVarChar).Value = 1;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Request Password";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            if (!string.IsNullOrEmpty(Session["Profil"].ToString()))
                            {
                                return RedirectToAction("ProfilSukses", "Notif");
                            }
                            else
                            {
                                return RedirectToAction("RegisterSukses", "Notif");
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileInputPassword", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileInputPassword", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
                else
                {
                    // tahap 3-B: jika password saling tidak cocok
                    ViewData["ErrorMsg"] = "Password dan konfirmasinya tidak sesuai. Silakan periksa kembali form isian dan ulangi proses setting.";
                    model.NIK = NIK;
                    return View("MobileInputPassword", model);
                }
            }
            else
            {
                // tahap 2-B: jika salah satu isian password belum diisi
                ViewData["ErrorMsg"] = "Anda belum mengisi password dan/atau konfirmasinya. Periksa kembali form isian dan ulangi proses setting.";
                return View("MobileInputPassword", model);
            }
        }
        #endregion

        #region Inisialisasi Reset Password
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(PasswordModel model)
        {
            // tahap 1: periksa baris NIK pada form isian
            if (!string.IsNullOrEmpty(model.NIK))
            {
                // tahap 2-A: periksa keadaan NIK hasil input pada DB
                Parameter = model.NIK.ToString().Trim();
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = Parameter;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            NIK = sdr["NIK"].ToString().Trim();
                            Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            Email = sdr["Email"].ToString().Trim();
                            Email_Perusahaan = sdr["Email_Perusahaan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terjadi gangguan pada koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah pada proses pembacaan data dari database. Pesan: \"" + ex.Message + "\"";
                        return View("MobileResetPassword", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View("MobileResetPassword", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                // tahap 3-A: jika NIK dari database dikenali
                if (!string.IsNullOrEmpty(NIK) && !string.IsNullOrEmpty(Nama_Karyawan))
                {
                    // menyimpan sesi nomor induk
                    ViewBag.NIK = NIK;
                    ViewData["NIK"] = NIK;
                    Session["NIK"] = NIK;

                    // setting SMTP server untuk melakukan prosedur reset password
                    Reset_Token = FormsAuthentication.HashPasswordForStoringInConfigFile(NIK + DateTime.Now.ToString(), "SHA1");
                    Reset_Token = Reset_Token.Substring(0, 20);

                    try
                    {
                        // konstruksi isi email
                        Msg = new StringBuilder();
                        Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                        Msg.Append("<br /><br />");
                        Msg.Append("Yth. " + Nama_Karyawan + ",");
                        Msg.Append("<br /><br />");
                        Msg.Append("Anda telah mengirim permintaan untuk proses reset password akun anda.<br />");
                        Msg.Append("Jika anda merasa tidak melakukan permintaan demikian, abaikan pesan email ini.");
                        Msg.Append("<br /><br />");
                        Msg.Append("<b><u>Perhatian</u></b>: Link token reset yang disertakan pada email ini akan kadaluarsa secara otomatis setelah dipakai atau melebihi batas waktu 7 hari yang ditentukan oleh sistem.");
                        Msg.Append("<br /><br />");
                        Msg.Append("Silakan klik link token berikut ini untuk melakukan reset password: ");
                        Msg.Append("<br /><br />");
                        Msg.Append("<a href=\"" + string.Format("http://" + ServerIP + "/Home/EditAkun/{0}/{1}", NIK, Reset_Token) + "\">" + string.Format("http://" + ServerIP + "/Home/EditAkun/{0}/{1}", NIK, Reset_Token) + "</a>"); // => ubah localhost dengan alamat situs remote SMTP server
                        Msg.Append("<br /><br />");
                        Msg.Append("Atas perhatiannya kami mengucapkan terima kasih.");
                        Msg.Append("<br /><br /><br /><br />");
                        Msg.Append("Jakarta, " + string.Format("{0:dd MMMM yyyy}", DateTime.Now));
                        Msg.Append("<br /><br />");
                        Msg.Append("Hormat kami,");
                        Msg.Append("<br /><br />");
                        Msg.Append("<br /><br />");
                        Msg.Append("<u>Human Resources Department ARISTA Group</u>");
                        Msg.Append("</span>");

                        // tentukan email mana yang valid
                        if (!string.IsNullOrEmpty(Email_Perusahaan))
                        {
                            Email_Temp = Email_Perusahaan;
                        }
                        else
                        {
                            Email_Temp = Email;
                        }

                        Helpers.SendReset(Msg.ToString(), Email_Temp, Email_Perusahaan, Email);

                        // membuat pesan email dengan body pesan terdefinisi s.d.a

                    }
                    catch (SmtpException smtpex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam proses pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator untuk melaporkan masalah. Pesan: \"" + smtpex.Message + "\"";
                        return View("MobileResetPassword", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator terkait untuk melaporkan masalah. Pesan: \"" + e.Message + "\"";
                        return View("MobileResetPassword", model);
                    }

                    // tulis token reset ke dalam database melalui stored procedure
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Reset_Token", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID_Token", SqlDbType.NVarChar).Value = Reset_Token;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Token", SqlDbType.NVarChar).Value = Reset_Token;
                        cmd.Parameters.Add("@Tgl_Buat", SqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@Tgl_Akhir", SqlDbType.DateTime).Value = DateTime.Now.AddDays(7d);
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            return RedirectToAction("KonfirmasiReset", "Notif");
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileResetPassword", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileResetPassword", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
                else
                {
                    ViewData["ErrorMsg"] = "NIK yang anda masukkan tidak terdaftar pada sistem. Silakan hubungi admin untuk mengetahui NIK anda.";
                    return View("MobileResetPassword", model);
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Anda belum mengisi kolom NIK untuk syarat reset password. Pastikan kotak isian NIK telah terisi dengan benar.";
                return View("MobileResetPassword", model);
            }
        }
        #endregion

        #region Reset Password
        [AllowAnonymous]
        public ActionResult EditAkun(string id, string id2, PasswordModel model)
        {
            if (Session["NIK"] != null)
            {
                model.NIK = Session["NIK"].ToString().Trim();
            }

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(id2))
            {
                model.NIK = id.ToString();
                Reset_Token = id2.ToString();

                // cari informasi token reset
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_Reset_Token", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Token", SqlDbType.NVarChar).Value = Reset_Token;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Token";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            Parameter = sdr["NIK"].ToString().Trim();
                            Periode_Akhir = (DateTime)Convert.ToDateTime(sdr["Tgl_Akhir"]);
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan saat memproses data akun anda. Pesan kesalahan: " + ex.Message;
                        return View("MobileEditAkun", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses data akun. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View("MobileEditAkun", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                if (Parameter != model.NIK.ToString().Trim() || Periode_Akhir == DateTime.MinValue || DateTime.Now.Date > Periode_Akhir.Date)
                {
                    return RedirectToAction("ResetBatal", "Notif", new { area = "Mobile" });
                }
                else
                {
                    // update 151222: token baru dihapus setelah penggantian password selesai, atau melebihi batas 7 hari pemakaian
                    return View("MobileEditAkun", model);
                }
            }
            else
            {
                return RedirectToAction("ResetBatal", "Notif", new { area = "Mobile" });
            }
        }

        // jika user menekan tombol proses
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditAkun(PasswordModel model, string returnUrl)
        {
            // mendapatkan nilai NIK dari sesi maupun model
            if (model.NIK == null)
            {
                NIK = Session["NIK"].ToString().Trim();
            }
            else
            {
                NIK = model.NIK.ToString().Trim();
            }

            // tahap 1: pengecekan input password
            if (!string.IsNullOrEmpty(model.Pass_Baru) && !string.IsNullOrEmpty(model.Konfirmasi))
            {
                // tahap 2-A: jika ada password dalam text input
                Pass_Baru = model.Pass_Baru.ToString().Trim();
                Konfirmasi = model.Konfirmasi.ToString().Trim();

                // samakan kedua isian password
                if (Konfirmasi == Pass_Baru)
                {
                    // tahap 3-A: susun stored procedure
                    Password = FormsAuthentication.HashPasswordForStoringInConfigFile(Pass_Baru, "SHA1");
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SM_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Password;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Password";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileEditAkun", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileEditAkun", model);
                        }
                        finally
                        {
                            conn.Close();
                        }

                        // update 151222: token hanya dihapus jika proses penggantian password SUDAH dimulai
                        // fungsi penghapusan token karena penggantian password sudah berjalan
                        cmd = new SqlCommand("SP_Reset_Token", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Token", SqlDbType.NVarChar).Value = Reset_Token;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Delete";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileEditAkun", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileEditAkun", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }

                    return RedirectToAction("ResetSukses", "Notif", new { area = "Mobile" });
                }
                else
                {
                    // tahap 3-B: jika password saling tidak cocok
                    ViewData["ErrorMsg"] = "Password dan konfirmasinya tidak sesuai. Silakan periksa kembali form isian dan ulangi proses setting.";
                    model.NIK = NIK;
                    return View("MobileEditAkun", model);
                }
            }
            else
            {
                // tahap 2-B: jika salah satu isian password belum diisi
                ViewData["ErrorMsg"] = "Anda belum mengisi password dan/atau konfirmasinya. Periksa kembali form isian dan ulangi proses setting.";
                return View("MobileEditAkun", model);
            }
        }
        #endregion

        // profil karyawan yang ditampilkan apabila belum mendapatkan email yang valid

        #region Profil Karyawan
        public ActionResult Profil()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile"});
            }
            else
            {
                SetPrivilege();
                var model = new KaryawanModel();
                record = Request.Cookies["LoginID"];
                if (record != null)
                {
                    if (!string.IsNullOrEmpty(record.Values["NIK"]))
                    {
                        NIK = record.Values["NIK"].ToString().Trim();
                    }
                }

                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            model.NIK = sdr["NIK"].ToString().Trim();
                            model.Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            model.Jenis_Kelamin = sdr["Jenis_Kelamin"].ToString().Trim();
                            model.Tempat_Lahir = sdr["Tempat_Lahir"].ToString().Trim();
                            model.Tgl_Lahir = Convert.ToDateTime(sdr["Tgl_Lahir"].ToString().Trim());
                            model.Provinsi = sdr["Provinsi"].ToString().Trim();
                            model.Kota = sdr["Kota"].ToString().Trim();
                            model.Alamat = sdr["Alamat"].ToString().Trim();
                            model.Agama = sdr["Agama"].ToString().Trim();
                            model.Status_Nikah = sdr["Status_Nikah"].ToString().Trim();
                            model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            model.Cabang = sdr["Cabang"].ToString().Trim();
                            model.Jabatan = sdr["Jabatan"].ToString().Trim();
                            model.Departemen = sdr["Departemen"].ToString().Trim();
                            model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            model.Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"].ToString().Trim());
                            model.Status_Karyawan = sdr["Status_Karyawan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses permintaan data profil. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileProfil", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses permintaan data profil. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileProfil", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                return View("MobileProfil", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profil(KaryawanModel model)
        {
            // tahap 1: cek seluruh input
            if (ModelState.IsValid)
            {
                // prosedur serupa dengan registrasi
                // parameter SP yang digunakan: "update karyawan"
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = model.Nama_Karyawan.ToString().Trim();
                    cmd.Parameters.Add("@Jenis_Kelamin", SqlDbType.NVarChar).Value = model.Jenis_Kelamin.ToString().Trim();
                    cmd.Parameters.Add("@Tempat_Lahir", SqlDbType.NVarChar).Value = model.Tempat_Lahir.ToString().Trim();
                    cmd.Parameters.Add("@Tgl_Lahir", SqlDbType.NVarChar).Value = model.Tgl_Lahir.ToString().Trim();
                    cmd.Parameters.Add("@Provinsi", SqlDbType.NVarChar).Value = model.Provinsi.ToString().Trim();
                    cmd.Parameters.Add("@Kota", SqlDbType.NVarChar).Value = model.Kota.ToString().Trim();
                    cmd.Parameters.Add("@Alamat", SqlDbType.NVarChar).Value = model.Alamat.ToString().Trim();
                    cmd.Parameters.Add("@Agama", SqlDbType.NVarChar).Value = model.Agama.ToString().Trim();
                    cmd.Parameters.Add("@Status_Nikah", SqlDbType.NVarChar).Value = model.Status_Nikah.ToString().Trim();
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email.ToString().Trim();
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = model.Perusahaan.ToString().Trim();
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = model.Cabang.ToString().Trim();
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = model.Jabatan.ToString().Trim();
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = model.Departemen.ToString().Trim();
                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = model.Nama_Atasan.ToString().Trim();
                    cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = model.Nama_Supervisor.ToString().Trim();
                    cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.DateTime).Value = model.Tgl_Masuk;
                    cmd.Parameters.Add("@Status_Karyawan", SqlDbType.NVarChar).Value = model.Status_Karyawan;
                    if (!string.IsNullOrEmpty(model.Email.ToString()) || !string.IsNullOrEmpty(model.Email_Perusahaan.ToString()))
                        cmd.Parameters.Add("@Email_Valid", SqlDbType.Int).Value = 1;
                    else
                        cmd.Parameters.Add("@Email_Valid", SqlDbType.NVarChar).Value = 0;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update"; // parameter update karyawan di sini

                    Session["NIK"] = model.NIK.ToString();
                    Session["Profil"] = "Profil";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("InputPassword", "Home", new { id = NIK, area = "Mobile" });
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data karyawan yang telah diedit.";
                        SetPrivilege();
                        return View("MobileProfil", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pemutakhiran data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileProfil", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Mohon isi keseluruhan data dengan lengkap untuk melanjutkan ke proses berikutnya.";
                SetPrivilege();
                return View("MobileProfil", model);
            }
        }
        #endregion

        #region Tampilan Layar Hasil Proses
        [AllowAnonymous]
        public ActionResult UserSession(String ReturnUrl)
        {
            if (!String.IsNullOrEmpty(ReturnUrl))
            {
                if (ReturnUrl.Contains("Partial"))
                {
                    ReturnUrl = ReturnUrl.Replace("Partial", String.Empty);
                }
                else if (ReturnUrl.Contains("_"))
                {
                    ReturnUrl = ReturnUrl.Replace("_", String.Empty);
                }
                else if (ReturnUrl.Contains("Logout"))
                {
                    return RedirectToAction("Dashboard", "Home", new { area = "Mobile" });
                }
                else if (ReturnUrl.Contains("GetKaryawan"))
                {
                    ReturnUrl = ReturnUrl.Replace("GetKaryawan", "Pengajuan");
                }

                Session["Return"] = ReturnUrl;
            }
            else
            {
                Session["Return"] = String.Empty;
            }
            return View("MobileUserSession");
        }

        public ActionResult Success()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Logout(string returnurl)
        {
            return RedirectToAction("Dashboard", "Home", new { area = "Mobile" });
        }

        public ActionResult LoginSukses()
        {
            return View("MobileSuccess");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LoginSukses(string returnurl)
        {
            if (Convert.ToInt32(Session["Email_Valid"]) == 0)
                return RedirectToAction("Profil", "Home", new { area = "Mobile" });
            else
                return RedirectToAction("Dashboard", "Home", new { area = "Mobile" });
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

        // -- Khusus sistem mobile
        // -- Memperoleh data melalui AJAX & hasil pencarian diteruskan melalui JSON

        /// <summary>
        /// Mendapatkan list cabang-cabang untuk sistem mobile.
        /// </summary>
        /// <param name="Nama_Perusahaan"></param>
        /// <returns></returns>
        public JsonResult GetCabangList(string Nama_Perusahaan)
        {
            List<SelectListItem> ListCabang = Providers.GetCabangByPerusahaan(Nama_Perusahaan) as List<SelectListItem>;
            return Json(new SelectList(ListCabang, "Value", "Text"));
        }

        /// <summary>
        /// Mendapatkan list atasan utama untuk sistem mobile.
        /// </summary>
        /// <param name="Departemen">Departemen kerja</param>
        /// <returns></returns>
        public JsonResult GetAtasanList(string Departemen)
        {
            List<SelectListItem> ListAtasan = Providers.GetAtasanByDepartemen(Departemen) as List<SelectListItem>;
            return Json(new SelectList(ListAtasan, "Value", "Text"));
        }

        /// <summary>
        /// Mendapatkan list atasan kedua untuk sistem mobile.
        /// </summary>
        /// <param name="Departemen">Departemen kerja</param>
        /// <returns></returns>
        public JsonResult GetSupervisorList(string Departemen)
        {
            List<SelectListItem> ListSupervisor = Providers.GetSupervisorByDepartemen(Departemen) as List<SelectListItem>;
            return Json(new SelectList(ListSupervisor, "Value", "Text"));
        }
	}
}