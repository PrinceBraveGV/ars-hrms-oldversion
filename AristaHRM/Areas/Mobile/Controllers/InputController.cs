using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using AristaHRM.Models;

namespace AristaHRM.Areas.Mobile.Controllers
{
    [RouteArea("Mobile")]
    [RoutePrefix("Input")]
    public class InputController : Controller
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
        // model karyawan
        public string NIK;
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
        public string Kepala_Cabang;

        // model perusahaan & cabang
        public string Kode_Perusahaan, Kode_Cabang;
        public string Nama_Perusahaan, Nama_Cabang;
        public string Kode_Singkat;
        public string Lokasi;
        public string Status_Cuti, Keterangan;

        public string Jenis_Training;
        public string Nama_Training;
        public string ServerIP = Providers.GetServerIP();

        /* Variabel angka */
        public long ID_Baru;
        public int Cuti_Massal;
        public int Libur_Weekend;
        public int Counter, Total_Count;

        /* Variabel tanggal */
        public DateTime Tgl_Lahir;
        public DateTime Tgl_Masuk;
        public DateTime Tgl_Resign;
        public DateTime Tgl_Mulai, Tgl_Selesai;
        public DateTime Tgl_Cek;
        public DateTime Tgl_Awal, Tgl_Akhir;
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
        public string Reset_Token;
        public string Parameter, Parameter2;

        #endregion

        /* BEGIN */

        #region Menu Input Data
        public ActionResult InputData()
        {
            Session["ViewMode"] = "Mobile";
            return View("MobileInputData");
        }
        #endregion

        #region Perusahaan

        #region Input Data Karyawan Baru
        public ActionResult InputKaryawan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    string Nama_Perusahaan = (Request.Params["Perusahaan"] ?? string.Empty).ToString();
                    string Nama_Cabang = (Request.Params["Cabang"] ?? string.Empty).ToString();
                    return View("MobileInputKaryawan", new KaryawanModel { Perusahaan = Nama_Perusahaan, Cabang = Nama_Cabang });
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult CabangPartial()
        {
            string Nama_Perusahaan = (Request.Params["Perusahaan"] ?? string.Empty).ToString();
            return PartialView("_MobileCabang", new KaryawanModel { Perusahaan = Nama_Perusahaan });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InputKaryawan(KaryawanModel model)
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
                            if (sdr.HasRows)
                            {
                                Parameter = sdr["Nama_Karyawan"].ToString().Trim();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        return View("MobileInputKaryawan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View("MobileInputKaryawan", model);
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
                        return View("MobileInputKaryawan", model);
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
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputKaryawan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputKaryawan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
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
                        ID_Baru = Convert.ToInt32(NIK_Baru);
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
                                Parameter = sdr["NIK"].ToString().Trim();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputKaryawan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputKaryawan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                // tahap 3: susun stored procedure jika user yang dicari memang baru mendaftar
                if (Parameter == null)
                {
                    // tahap 3-A: transfer model ke variabel kendali
                    Jenis_Kelamin = model.Jenis_Kelamin.ToString().Trim();
                    Tempat_Lahir = model.Tempat_Lahir.ToString().Trim();
                    Tgl_Lahir = model.Tgl_Lahir;
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
                    Status_Karyawan = model.Status_Karyawan.ToString().Trim();
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
                            SetPrivilege();
                            return View("MobileInputKaryawan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses registrasi. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileInputKaryawan", model);
                        }
                        finally
                        {
                            conn.Close();
                        }

                        Periode_Awal = DateTime.Now;
                        Periode_Akhir = DateTime.Now.AddYears(1);

                        // dapatkan nilai ID dari tabel sisa cuti
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
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi saat memproses data registrasi. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobileInputKaryawan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses logout. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileInputKaryawan", model);
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
                        cmd.Parameters.Add("@Cuti_Massal", SqlDbType.Int).Value = 1; // default hanya cuti massal Lebaran
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
                            SetPrivilege();
                            return View("MobileInputKaryawan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileInputKaryawan", model);
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
                        Msg.Append("<a href=\"" + string.Format("http://" + ServerIP + "/Home/SettingPass/{0}", NIK) + "\">" + string.Format("http://" + ServerIP + "/Home/SettingPass/{0}", NIK) + "</a>"); // => ubah localhost dengan alamat situs remote SMTP server
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
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam proses pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator terkait untuk melaporkan masalah yang terjadi. Kode pesan: \"" + smtpex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputKaryawan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator terkait untuk melaporkan masalah. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputKaryawan", model);
                    }

                    return RedirectToAction("KonfirmasiSetting", "Notif"); // untuk versi final diubah ke return RedirectToAction("ConfirmRequest");
                }
                else
                {
                    // tahap 3-B: user ybs sudah terdaftar sebelumnya pada database
                    ViewData["ErrorMsg"] = "NIK yang bersangkutan sudah terdaftar pada sistem. Silakan hubungi administrator terkait untuk dukungan teknis registrasi.";
                    SetPrivilege();
                    return View("MobileInputKaryawan", model);
                }
            }
            else
            {
                SetPrivilege();
                string Nama_Perusahaan = (Request.Params["Perusahaan"] ?? string.Empty).ToString();
                string Nama_Cabang = (Request.Params["Cabang"] ?? string.Empty).ToString();
                return View("MobileInputKaryawan", new KaryawanModel { Perusahaan = Nama_Perusahaan, Cabang = Nama_Cabang });
            }
        }

        #region Setting Password
        public ActionResult InputPassword(PasswordModel model)
        {
            model.NIK = Session["NIK"].ToString().Trim();
            SetPrivilege();
            return View("MobileInputPassword", model);
        }

        // jika user menekan tombol proses
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult InputPassword(PasswordModel model, string returnUrl)
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
                            return RedirectToAction("RegisterSukses", "Notif");
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            model.NIK = NIK;
                            SetPrivilege();
                            return View("MobileInputPassword", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            model.NIK = NIK;
                            SetPrivilege();
                            return View("MobileInputPassword", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                    else
                    {
                        // tahap 3-B: jika password saling tidak cocok
                        ViewData["ErrorMsg"] = "Password dan konfirmasinya tidak sesuai. Silakan periksa kembali form isian dan ulangi proses setting.";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobileInputPassword", model);
                    }
                }
                else
                {
                    // tahap 2-B: jika salah satu isian password belum diisi
                    ViewData["ErrorMsg"] = "Anda belum mengisi password dan/atau konfirmasinya. Periksa kembali form isian dan ulangi proses setting.";
                    SetPrivilege();
                    return View("MobileInputPassword", model);
                }
            }
        }
        #endregion
        #endregion

        #region Input Data Perusahaan Baru
        public ActionResult InputPerusahaan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    var model = new PerusahaanModel();
                    model.Nama_Perusahaan = "PT. ";
                    return View("MobileInputPerusahaan", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InputPerusahaan(PerusahaanModel model)
        {
            if (ModelState.IsValid)
            {
                // tahap 1: transfer model ke variabel kendali
                if (!string.IsNullOrEmpty(model.Nama_Perusahaan))
                {
                    Nama_Perusahaan = model.Nama_Perusahaan.ToString().Trim();
                }
                else
                {
                    ViewData["ErrorMsg"] = "Nama perusahaan tidak boleh kosong.";
                    SetPrivilege();
                    return View("MobileInputPerusahaan", model);
                }
                if (!string.IsNullOrEmpty(model.Kode_Singkat))
                {
                    Kode_Singkat = model.Kode_Singkat.ToString().Trim();
                }
                else
                {
                    ViewData["ErrorMsg"] = "Kode singkatan tidak boleh kosong.";
                    SetPrivilege();
                    return View("MobileInputPerusahaan", model);
                }

                // periksa keadaan perusahaan duplikat
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_CheckDupe", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Perusahaan";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            if (!Convert.IsDBNull(sdr["Nama_Perusahaan"]))
                            {
                                Parameter = sdr["Nama_Perusahaan"].ToString().Trim();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputPerusahaan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputPerusahaan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    if (!string.IsNullOrEmpty(Parameter))
                    {
                        ViewData["ErrorMsg"] = "Perusahaan yang anda masukkan telah terdaftar pada sistem. Silakan input perusahaan lain atau edit data yang telah ada.";
                        SetPrivilege();
                        return View("MobileInputPerusahaan", model);
                    }

                    // proses pembuatan kode perusahaan
                    cmd = new SqlCommand("SM_Perusahaan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Max";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            Parameter = sdr["Kode"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputPerusahaan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputPerusahaan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    if (!string.IsNullOrEmpty(Parameter))
                    {
                        ID_Baru = Convert.ToInt32(Parameter);
                    }
                    else
                    {
                        ID_Baru = 0;
                    }

                    ID_Baru++;
                    Kode_Perusahaan = ID_Baru.ToString();

                    // tahap 2: susun stored procedure
                    cmd = new SqlCommand("SM_Perusahaan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Kode_Perusahaan", SqlDbType.NVarChar).Value = Kode_Perusahaan;
                    cmd.Parameters.Add("@Nama_Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Kode_Singkat", SqlDbType.NVarChar).Value = Kode_Singkat;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("AddSukses", "Notif");
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputPerusahaan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputPerusahaan", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Proses pengisian data perusahaan belum lengkap. Harap periksa kembali form isian anda.";
                SetPrivilege();
                return View("MobileInputPerusahaan", model);
            }
        }
        #endregion

        #region Input Data Cabang Baru
        public ActionResult InputCabang()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InputCabang(CabangModel model)
        {
            if (ModelState.IsValid)
            {
                // tahap 1: transfer model ke variabel kendali
                Nama_Perusahaan = model.Nama_Perusahaan.ToString().Trim();
                Nama_Cabang = model.Nama_Cabang.ToString().Trim();
                Kode_Singkat = model.Kode_Singkat.ToString().Trim();
                Lokasi = model.Lokasi.ToString().Trim();
                Kepala_Cabang = model.Kepala_Cabang.ToString().Trim();

                // periksa keadaan cabang duplikat
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_CheckDupe", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Cabang";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            Parameter = sdr["Nama_Perusahaan"].ToString().Trim();
                            Parameter2 = sdr["Nama_Cabang"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    if (!string.IsNullOrEmpty(Parameter) && !string.IsNullOrEmpty(Parameter2))
                    {
                        ViewData["ErrorMsg"] = "Nama cabang yang anda masukkan telah terdaftar pada sistem. Silakan input cabang lain atau edit data yang telah ada terlebih dahulu.";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }

                    // permintaan kode singkat perusahaan
                    cmd = new SqlCommand("SM_Perusahaan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Nama_Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Perusahaan";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            // menyimpan kode singkat perusahaan
                            Kode_Perusahaan = sdr["Kode_Singkat"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // proses pembuatan kode cabang
                    cmd = new SqlCommand("SM_Cabang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Nama_Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Max";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            Parameter = sdr["Kode"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                for (int i = 0; i < Parameter.Length; i++)
                {
                    if (Char.IsDigit(Parameter[i]))
                    {
                        Proses += Parameter[i];
                    }
                }

                if (Proses.Length > 0)
                {
                    ID_Baru = Convert.ToInt32(Proses);
                }
                else
                {
                    ID_Baru = 0; // hasil proses pasti selain 0 karena 00 adalah kode khusus kantor pusat/HO
                }

                ID_Baru++;

                // tahap 2: assembly kode cabang yang baru, kode singkatan perusahaan + kode nomor urut cabang baru
                Kode_Cabang = Kode_Perusahaan + ID_Baru.ToString("00");

                // tahap 3: susun stored procedure
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Cabang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Kode_Cabang", SqlDbType.NVarChar).Value = Kode_Cabang;
                    cmd.Parameters.Add("@Nama_Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Nama_Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
                    cmd.Parameters.Add("@Kode_Singkat", SqlDbType.NVarChar).Value = Kode_Singkat;
                    cmd.Parameters.Add("@Lokasi", SqlDbType.NVarChar).Value = Lokasi;
                    cmd.Parameters.Add("@Kepala_Cabang", SqlDbType.NVarChar).Value = Kepala_Cabang;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("AddSukses", "Notif");
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputCabang", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Proses pengisian data cabang belum lengkap. Harap periksa kembali form isian anda.";
                SetPrivilege();
                return View("MobileInputCabang", model);
            }
        }
        #endregion

        #endregion

        #region Pengaturan Cuti

        #region Cuti Massal Kantor Pusat
        public ActionResult InputCutiMassalPusat()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                var model = new CutiMassalModel();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    Session["Bagian"] = "Pusat";
                    return View("MobileCutiMassalPusat", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InputCutiMassalPusat(CutiMassalModel model)
        {
            // pengecekan keadaan model
            if (ModelState.IsValid)
            {
                // tahap 1: transfer model ke variabel kendali
                model.ID_Daftar = "P" + string.Format("{0:yyMMdd}", model.Tgl_Cuti);
                ID_Cuti = model.ID_Daftar.ToString().Trim();
                Tgl_Mulai = model.Tgl_Cuti;
                Keterangan = model.Keterangan.ToString();
                Status_Cuti = "Massal";
                Cuti_Massal = 1;

                // cek apakah ada cuti massal dalam tanggal yang sama
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Cuti_Massal_P", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Daftar", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Tgl_Cuti", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Tgl";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            if (sdr["ID_Daftar"] != null)
                            {
                                Parameter = sdr["ID_Daftar"].ToString().Trim();
                                Tgl_Cek = Convert.ToDateTime(sdr["Tgl_Cuti"]);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalPusat", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalPusat", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // tahap 2: periksa adanya duplikasi tanggal yang sama untuk cuti massal tertentu

                    if (Parameter != null && Tgl_Mulai == Tgl_Cek)
                    {
                        // tahap 2-B: tanggal cuti yang dimaksud sudah ditempati cuti massal lainnya
                        ViewData["ErrorMsg"] = "Sistem mendeteksi adanya tanggal cuti massal pada hari yang diajukan. Silakan lakukan edit data pada tanggal yang ditentukan, atau tentukan hari lainnya.";
                        SetPrivilege();
                        return View("MobileCutiMassalPusat", model);
                    }

                    // tahap 3: susun stored procedure
                    cmd = new SqlCommand("SM_Cuti_Massal_P", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Daftar", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Tgl_Cuti", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Jenis_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    cmd.Parameters.Add("@Cuti_Massal", SqlDbType.Int).Value = Cuti_Massal;
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
                        SetPrivilege();
                        return View("MobileCutiMassalPusat", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalPusat", model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    // tahap 4: cuti massal di HO berlaku juga untuk cabang
                    model.ID_Daftar = "C" + string.Format("{0:yyMMdd}", model.Tgl_Cuti);
                    ID_Cuti = model.ID_Daftar.ToString().Trim();
                    Tgl_Mulai = model.Tgl_Cuti;
                    Keterangan = model.Keterangan.ToString();
                    Status_Cuti = "Massal";
                    Cuti_Massal = 1;

                    // tahap 5: susun stored procedure
                    cmd = new SqlCommand("SM_Cuti_Massal_C", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Daftar", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Tgl_Cuti", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Jenis_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    cmd.Parameters.Add("@Cuti_Massal", SqlDbType.Int).Value = Cuti_Massal;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("AddSukses", "Notif");
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalPusat", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalPusat", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Proses pengisian data cuti massal belum lengkap. Harap periksa kembali form isian anda.";
                SetPrivilege();
                return View("MobileCutiMassalPusat", model);
            }
        }

        public ActionResult CutiMassalPusatPartial()
        {
            var model = DB.TM_List_Cutis;
            return PartialView("_MobileCutiMassalPusat", model.ToList());
        }

        // proses update dan delete cuti massal
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateCM(TM_List_Cuti items)
        {
            Keterangan = Session["Bagian"].ToString();
            var model = DB.TM_List_Cutis;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelitem = model.FirstOrDefault(it => it.ID_Daftar == items.ID_Daftar);
                    if (modelitem != null)
                    {
                        this.UpdateModel(modelitem);
                        DB.SaveChanges();
                        ViewData["EditSuccess"] = "Data cuti massal sudah di-update";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan selama proses pemutakhiran sebagai berikut: \"" + e.Message + "\"";
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Silakan periksa kembali form isian anda.";
            }
            if (Session["Bagian"] != null && Keterangan == "Cabang")
            {
                return PartialView("_MobileCutiMassalCabang", model.ToList());
            }
            else
            {
                return PartialView("_MobileCutiMassalPusat", model.ToList());
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HapusCM(string ID_Daftar)
        {
            var model = DB.TM_List_Cutis;
            if (ID_Daftar != null)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.ID_Daftar == ID_Daftar);
                    if (item != null)
                    {
                        model.Remove(item);
                        DB.SaveChanges();
                        ViewData["EditSuccess"] = "Data cuti massal sudah dihapus";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan selama proses penghapusan sebagai berikut: \"" + e.Message + "\"";
                }
            }
            if (Session["Bagian"] != null && Keterangan == "Cabang")
            {
                return PartialView("_MobileCutiMassalCabang", model.ToList());
            }
            else
            {
                return PartialView("_MobileCutiMassalPusat", model.ToList());
            }
        }
        #endregion

        #region Cuti Massal Kantor Cabang
        public ActionResult InputCutiMassalCabang()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                var model = new CutiMassalModel();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    Session["Bagian"] = "Cabang";
                    return View("MobileCutiMassalCabang", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InputCutiMassalCabang(CutiMassalModel model)
        {
            // pengecekan keadaan model
            if (ModelState.IsValid)
            {
                // tahap 1: transfer model ke variabel kendali
                model.ID_Daftar = "C" + string.Format("{0:yyMMdd}", model.Tgl_Cuti);
                ID_Cuti = model.ID_Daftar.ToString().Trim();
                Tgl_Mulai = model.Tgl_Cuti;
                Keterangan = model.Keterangan.ToString();
                Status_Cuti = "Massal";
                Cuti_Massal = 1;

                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Cuti_Massal_P", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Tgl_Cuti", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Tgl";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            if (sdr["Tgl_Cuti"] != null)
                            {
                                Tgl_Cek = Convert.ToDateTime(sdr["Tgl_Cuti"]);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalCabang", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalCabang", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // tahap 2: periksa tanggal cuti massal, pastikan tidak tumpang-tindih
                    if (Tgl_Cek == Tgl_Mulai)
                    {
                        ViewData["ErrorMsg"] = "Sistem mendeteksi adanya tanggal cuti massal pada hari yang diajukan. Silakan lakukan edit data pada tanggal yang ditentukan, atau tentukan hari lainnya.";
                        SetPrivilege();
                        return View("MobileCutiMassalCabang", model);
                    }

                    // tahap 3: susun stored procedure
                    cmd = new SqlCommand("SM_Cuti_Massal_C", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Daftar", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Tgl_Cuti", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Jenis_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    cmd.Parameters.Add("@Cuti_Massal", SqlDbType.Int).Value = Cuti_Massal;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("AddSukses", "Notif");
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalCabang", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileCutiMassalCabang", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Proses pengisian data cuti massal belum lengkap. Harap periksa kembali form isian anda.";
                SetPrivilege();
                return View("MobileCutiMassalCabang", model);
            }
        }

        public ActionResult CutiMassalCabangPartial()
        {
            var model = DB.TM_List_Cutis;
            return PartialView("_MobileCutiMassalCabang", model.ToList());
        }
        #endregion

        #region Hari Libur
        public ActionResult InputLiburan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin")
                {
                    var model = new LiburModel();
                    return View("MobileInputLiburan", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult InputLiburPartial()
        {
            var model = DB.TM_List_Liburs;
            return PartialView("_MobileLiburan", model.ToList());
        }

        [HttpPost]
        public ActionResult InputLiburan(LiburModel model)
        {
            // cek keadaan model
            if (ModelState.IsValid)
            {
                // tahap 1: transfer model ke variabel kendali
                model.ID_Daftar = "M" + string.Format("{0:yyMMdd}", model.Tgl_Libur);
                ID_Cuti = model.ID_Daftar.ToString().Trim();
                Tgl_Mulai = model.Tgl_Libur;
                Keterangan = model.Keterangan.ToString();

                // pengecekan keadaan tanggal libur
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_List_Libur", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Tgl_Libur", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            if (sdr["ID_Cuti"] != null)
                            {
                                Parameter = sdr["ID_Cuti"].ToString().Trim();
                                Tgl_Cek = Convert.ToDateTime(sdr["Tgl_Cuti"]);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputLiburan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputLiburan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // tahap 2: periksa adanya duplikasi tanggal yang sama untuk hari libur tertentu

                    if (Parameter != null && Tgl_Cek == Tgl_Mulai)
                    {
                        // tahap 2-B: tanggal yang dimaksud sudah ditempati jenis liburan lainnya
                        ViewData["ErrorMsg"] = "Sistem mendeteksi adanya tanggal libur duplikat pada data yang diberikan. Silakan lakukan edit data pada tanggal yang ditentukan, atau tentukan hari lainnya.";
                        SetPrivilege();
                        return View("MobileInputLiburan", model);
                    }

                    // mengolah keterangan tanggal libur yang dipilih
                    if (Tgl_Mulai.DayOfWeek == DayOfWeek.Saturday || Tgl_Mulai.DayOfWeek == DayOfWeek.Sunday)
                    {
                        Libur_Weekend = 1;
                    }
                    else
                    {
                        Libur_Weekend = 0;
                    }

                    // tahap 3: susun stored procedure
                    cmd = new SqlCommand("SM_List_Libur", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Daftar", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Tgl_Libur", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    cmd.Parameters.Add("@Libur_Weekend", SqlDbType.Int).Value = Libur_Weekend;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("AddSukses", "Notif");
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputLiburan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileInputLiburan", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Proses pengisian data hari libur belum lengkap. Harap periksa kembali form isian anda.";
                SetPrivilege();
                return View("MobileInputLiburan", model);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateLibur(TM_List_Libur items)
        {
            var model = DB.TM_List_Liburs;
            if (ModelState.IsValid)
            {
                try
                {
                    var modelitem = model.FirstOrDefault(it => it.ID_Daftar == items.ID_Daftar);
                    if (modelitem != null)
                    {
                        this.UpdateModel(modelitem);
                        DB.SaveChanges();
                        ViewData["EditSuccess"] = "Data hari libur sudah di-update";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan selama proses pemutakhiran sebagai berikut: \"" + e.Message + "\"";
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Silakan periksa kembali form isian anda.";
            }
            return PartialView("_MobileLiburan", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HapusLibur(string ID_Daftar)
        {
            var model = DB.TM_List_Liburs;
            if (ID_Daftar != null)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.ID_Daftar == ID_Daftar);
                    if (item != null)
                    {
                        model.Remove(item);
                        DB.SaveChanges();
                        ViewData["EditSuccess"] = "Data hari libur sudah dihapus";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan selama proses penghapusan sebagai berikut: \"" + e.Message + "\"";
                }
            }
            return PartialView("_MobileLiburan", model.ToList());
        }
        #endregion

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