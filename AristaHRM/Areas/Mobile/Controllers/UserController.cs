using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
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
    [RoutePrefix("User")]
    public class UserController : Controller
    {
        #region Daftar Variabel
        // variabel database
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
        public string Nama_Karyawan, Nama_Tabel;
        public string Password, Pass_Lama, Pass_Baru, Pass_Hash, Konfirmasi;
        public string Jenis_Kelamin;
        public string Tempat_Lahir;
        public string Alamat;
        public string Agama;
        public string Status_Nikah;
        public string Email, Email_Perusahaan, Email_Atasan, Email_Supervisor, Email_Temp;
        public string Perusahaan, Cabang, Jabatan, Departemen, Nama_Atasan, Nama_Supervisor;
        public string Pembuat, Privilege, Privilege_At;
        public string ID_Daftar, Nama_Event, Jenis_Event;
        public string Notes;

        // model khusus cuti
        public string ID_Cuti;
        public string Jenis_Cuti;
        public string Pemberi, Keperluan;
        public string Status_Cuti, Keterangan, No_Kontak;
        public string Nomor_Kontak;
        public string Lokasi, Area_Kerja;

        // model khusus resign
        public string Alasan;
        public long ID_Baru;
        public int Masa_Cuti;
        public int Libur_Awal, Libur_Akhir, Sisa_Tahunan;
        public int Jatah_Cuti, Total_Cuti, Cuti_Tahunan, Cuti_Massal, Sisa_Cuti;
        public int Total_CM;
        public int Counter, Total_Count;

        // model feedback
        public string ID_Pesan, Subjek, Isi_Pesan;
        public string ServerIP = Providers.GetServerIP();

        // model array
        public string[] ArrayData;
        public string[] ResultData;

        // variabel angka
        public double Selisih_Awal, Selisih_Akhir;

        // variabel tanggal
        public DateTime Tgl_Lahir;
        public DateTime Tgl_Masuk;
        public DateTime Periode_Awal, Periode_Akhir;
        public DateTime Tgl_Pengajuan, Tgl_Setuju;
        public DateTime Tgl_Mulai, Tgl_Selesai, Tgl_Awal, Tgl_Akhir;
        public DateTime Tgl_Resign;
        public DateTime Tgl_Cuti_Massal;

        #endregion

        // variabel lainnya
        public HttpCookie record;
        public StringBuilder Msg, Token;
        public MailMessage EmailRequest;
        public SmtpClient SMTPServer;
        public string Parameter;

        public string PhotoDir;

        #endregion

        /* BEGIN */

        #region Menu User
        public ActionResult UserMenu()
        {
            Session["ViewMode"] = "Mobile";
            return View("MobileUserMenu");
        }
        #endregion

        #region Pengaturan Karyawan

        #region Profil User
        public ActionResult Profil(KaryawanModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return Redirect(Url.Action("UserSession", "Home"));
            }
            else
            {
                Session["ViewMode"] = "Desktop";

                SetPrivilege();

                // kosongkan notes
                Notes = null;

                record = Request.Cookies["LoginID"];
                if (record != null)
                {
                    if (!string.IsNullOrEmpty(record.Values["NIK"]))
                    {
                        NIK = record.Values["NIK"].ToString();
                    }
                }

                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            model.NIK = NIK;
                            model.Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            model.Jenis_Kelamin = sdr["Jenis_Kelamin"].ToString().Trim();
                            model.Tempat_Lahir = sdr["Tempat_Lahir"].ToString().Trim();
                            model.Tgl_Lahir = Convert.ToDateTime(sdr["Tgl_Lahir"].ToString().Trim());
                            model.Provinsi = sdr["Provinsi"].ToString().Trim();
                            model.Kota = sdr["Kota"].ToString().Trim();
                            model.Alamat = sdr["Alamat"].ToString().Trim();
                            model.Agama = sdr["Agama"].ToString().Trim();
                            model.Status_Nikah = sdr["Status_Nikah"].ToString().Trim();
                            model.Email = sdr["Email"].ToString().Trim();
                            model.Email_Perusahaan = sdr["Email_Perusahaan"].ToString().Trim();
                            model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            model.Cabang = sdr["Cabang"].ToString().Trim();
                            model.Jabatan = sdr["Jabatan"].ToString().Trim();
                            model.Departemen = sdr["Departemen"].ToString().Trim();
                            model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            model.Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"].ToString().Trim());
                            model.Status_Karyawan = sdr["Status_Karyawan"].ToString().Trim();
                            model.Notes = sdr["Notes"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses permintaan data profil. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses permintaan data profil. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // hitung sisa cuti ybs di database
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
                            Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            Cabang = sdr["Cabang"].ToString().Trim();
                            Jabatan = sdr["Jabatan"].ToString().Trim();
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"]);
                            if (!string.IsNullOrEmpty(sdr["Tgl_Resign"].ToString()))
                            {
                                Tgl_Resign = Convert.ToDateTime(sdr["Tgl_Resign"]);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    Tgl_Awal = Tgl_Masuk;
                    if (Tgl_Masuk.Month < DateTime.Now.Month)
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year + 1));
                    }
                    else if (Tgl_Masuk.Day > DateTime.Now.Day && Tgl_Masuk.Month <= DateTime.Now.Month)
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year));
                    }
                    else if (Tgl_Masuk.Day <= DateTime.Now.Day && Tgl_Masuk.Month <= DateTime.Now.Month)
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year + 1));
                    }
                    else
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year));
                    }

                    // interupsi jika karyawan punya tanggal resign
                    if (Tgl_Resign != null && Tgl_Resign != DateTime.MinValue)
                    {
                        Tgl_Akhir = Tgl_Resign;
                    }

                    Selisih_Awal = (Tgl_Akhir - Tgl_Awal).TotalDays;

                    // susun stored procedure
                    // masukkan parameter sesuai dengan prosedur sisa cuti

                    cmd = new SqlCommand("SP_Hitung_Cuti_Hangus", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                    if (!string.IsNullOrEmpty(Nama_Supervisor))
                    {
                        cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
                    }
                    cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.DateTime).Value = Tgl_Masuk;
                    cmd.Parameters.Add("@Tgl_Akhir", SqlDbType.DateTime).Value = Tgl_Akhir;
                    cmd.Parameters.Add("@Temp1", SqlDbType.NVarChar).Value = Selisih_Awal.ToString(); // ambil dari combo box model
                    cmd.Parameters.Add("@Temp2", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@Temp3", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@Temp4", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@Temp5", SqlDbType.Int).Value = Tgl_Akhir.Year - Tgl_Masuk.Year;

                    // eksekusi parameter query di balik layar
                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        sda = new SqlDataAdapter(cmd);
                        ds = new DataSet();
                        sda.Fill(ds);
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    if (model.Tgl_Masuk.Month >= DateTime.Now.Month && DateTime.Now.Day < Tgl_Akhir.Day)
                    {
                        ID_Baru = DateTime.Now.Year - 1;
                    }
                    else if (model.Tgl_Masuk.Month >= DateTime.Now.Month && DateTime.Now.Day >= Tgl_Akhir.Day)
                    {
                        if (Tgl_Akhir.Year <= DateTime.Now.Year)
                        {
                            ID_Baru = DateTime.Now.Year - 1;
                        }
                        else if (Tgl_Akhir.Year > DateTime.Now.Year)
                        {
                            ID_Baru = DateTime.Now.Year;
                        }
                    }
                    else
                    {
                        ID_Baru = DateTime.Now.Year;
                    }

                    // temukan informasi sisa cuti karyawan ybs
                    cmd = new SqlCommand("SP_Sisa_Cuti", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = Convert.ToInt32(ID_Baru);
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Sisa";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        if (sdr.HasRows == false)
                        {
                            ViewData["SisaCuti"] = 0;
                            ViewData["TotalCuti"] = 0;
                        }
                        else
                        {
                            while (sdr.Read())
                            {
                                if (sdr["NIK"] != null && sdr["Tahun_Cuti"] != null)
                                {
                                    ViewData["SisaCuti"] = sdr["Sisa_Cuti"].ToString().Trim();
                                    if (Convert.ToInt32(sdr["Total_Cuti"].ToString().Trim()) > 24)
                                    {
                                        ViewData["TotalCuti"] = 24;
                                    }
                                    else
                                    {
                                        ViewData["TotalCuti"] = sdr["Total_Cuti"].ToString().Trim();
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses permintaan data profil. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses permintaan data profil. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
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

        // jika admin mengirim permintaan tampilan profil dengan NIK tertentu
        [HttpPost]
        public ActionResult Profil(KaryawanModel model, string Length)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                Session["ViewMode"] = "Desktop";

                SetPrivilege();

                // kosongkan notes
                Notes = null;

                NIK = model.NIK.ToString().Trim();

                // tampilkan profil user

                NIK = model.NIK.ToString().Trim();

                // keluarkan output profil karyawan sesuai NIK      
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (string.IsNullOrEmpty(model.NIK))
                    {
                        cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                    }
                    else
                    {
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    }
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
                            model.Email = sdr["Email"].ToString().Trim();
                            model.Email_Perusahaan = sdr["Email_Perusahaan"].ToString().Trim();
                            model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            model.Cabang = sdr["Cabang"].ToString().Trim();
                            model.Jabatan = sdr["Jabatan"].ToString().Trim();
                            model.Departemen = sdr["Departemen"].ToString().Trim();
                            model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            model.Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"].ToString().Trim());
                            model.Status_Karyawan = sdr["Status_Karyawan"].ToString().Trim();
                            if (sdr["Notes"] != null && sdr["Notes"] != Convert.DBNull)
                                Notes = sdr["Notes"].ToString().Trim();
                            else
                                Notes = null;
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses permintaan data profil. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses permintaan data profil. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // hitung sisa cuti ybs di database                    
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            Cabang = sdr["Cabang"].ToString().Trim();
                            Jabatan = sdr["Jabatan"].ToString().Trim();
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"]);
                            if (!string.IsNullOrEmpty(sdr["Tgl_Resign"].ToString()))
                            {
                                Tgl_Resign = Convert.ToDateTime(sdr["Tgl_Resign"]);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    Tgl_Awal = Tgl_Masuk;
                    if (Tgl_Masuk.Month < DateTime.Now.Month)
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year + 1));
                    }
                    else if (Tgl_Masuk.Day > DateTime.Now.Day && Tgl_Masuk.Month <= DateTime.Now.Month)
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year));
                    }
                    else if (Tgl_Masuk.Day <= DateTime.Now.Day && Tgl_Masuk.Month <= DateTime.Now.Month)
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year + 1));
                    }
                    else
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year));
                    }

                    // interupsi jika karyawan punya tanggal resign
                    if (Tgl_Resign != null && Tgl_Resign != DateTime.MinValue)
                    {
                        Tgl_Akhir = Tgl_Resign;
                    }

                    Selisih_Awal = (Tgl_Akhir - Tgl_Awal).TotalDays;

                    // susun stored procedure
                    // masukkan parameter sesuai dengan prosedur sisa cuti

                    cmd = new SqlCommand("SP_Hitung_Cuti_Hangus", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                    if (!string.IsNullOrEmpty(Nama_Supervisor))
                    {
                        cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
                    }
                    cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.DateTime).Value = Tgl_Masuk;
                    cmd.Parameters.Add("@Tgl_Akhir", SqlDbType.DateTime).Value = Tgl_Akhir;
                    cmd.Parameters.Add("@Temp1", SqlDbType.NVarChar).Value = Selisih_Awal.ToString(); // ambil dari combo box model
                    cmd.Parameters.Add("@Temp2", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@Temp3", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@Temp4", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@Temp5", SqlDbType.Int).Value = Tgl_Akhir.Year - Tgl_Masuk.Year;

                    // eksekusi parameter query di balik layar
                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        sda = new SqlDataAdapter(cmd);
                        ds = new DataSet();
                        sda.Fill(ds);
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    if (model.Tgl_Masuk.Month >= DateTime.Now.Month && DateTime.Now.Day < Tgl_Akhir.Day)
                    {
                        ID_Baru = DateTime.Now.Year - 1;
                    }
                    else if (model.Tgl_Masuk.Month >= DateTime.Now.Month && DateTime.Now.Day >= Tgl_Akhir.Day)
                    {
                        if (Tgl_Akhir.Year <= DateTime.Now.Year)
                        {
                            ID_Baru = DateTime.Now.Year - 1;
                        }
                        else if (Tgl_Akhir.Year > DateTime.Now.Year)
                        {
                            ID_Baru = DateTime.Now.Year;
                        }
                    }
                    else
                    {
                        ID_Baru = DateTime.Now.Year;
                    }

                    // temukan informasi sisa cuti karyawan ybs
                    cmd = new SqlCommand("SP_Sisa_Cuti", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = Convert.ToInt32(ID_Baru);
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Sisa";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        if (sdr.HasRows == false)
                        {
                            ViewData["SisaCuti"] = 0;
                            ViewData["TotalCuti"] = 0;
                        }
                        else
                        {
                            while (sdr.Read())
                            {
                                if (sdr["NIK"] != null && sdr["Tahun_Cuti"] != null)
                                {
                                    ViewData["SisaCuti"] = sdr["Sisa_Cuti"].ToString().Trim();
                                    if (Convert.ToInt32(sdr["Total_Cuti"].ToString().Trim()) > 24)
                                    {
                                        ViewData["TotalCuti"] = 24;
                                    }
                                    else
                                    {
                                        ViewData["TotalCuti"] = sdr["Total_Cuti"].ToString().Trim();
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses permintaan data profil. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses permintaan data profil. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                // kembali ke halaman profil
                SetPrivilege();
                return View("MobileProfil", model);
            }
        }
        #endregion

        #region Ubah Password
        public ActionResult UbahPassword()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                Session["ViewMode"] = "Desktop";

                SetPrivilege();
                var model = new PasswordModel();
                // tentukan NIK berdasarkan nama user ybs, atau ambil dari cookie
                record = Request.Cookies["LoginID"];
                if (record != null)
                {
                    if (!string.IsNullOrEmpty(record.Values["NIK"]))
                    {
                        NIK = record.Values["NIK"].ToString().Trim();
                    }
                }
                model.NIK = NIK;
                Session["NIK"] = NIK;
                return View("MobileUbahPassword", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UbahPassword(PasswordModel model)
        {
            // tahap penggantian password user
            if (!string.IsNullOrEmpty(model.Pass_Baru) && !string.IsNullOrEmpty(model.Konfirmasi))
            {
                // tahap 1: transfer model ke variabel kendali
                Parameter = model.NIK.ToString().Trim();
                Pass_Lama = FormsAuthentication.HashPasswordForStoringInConfigFile(model.Pass_Lama.ToString(), "SHA1");
                Pass_Baru = model.Pass_Baru.ToString().Trim();
                Konfirmasi = model.Konfirmasi.ToString().Trim();

                // tahap 2: cek keadaan password user terpilih yang berlaku saat ini
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = Parameter;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            NIK = sdr["NIK"].ToString().Trim();
                            Password = sdr["Password"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses pencarian data. Pesan: \"" + ex.Message + "\"";
                        model.NIK = NIK;
                        return View("MobileUbahPassword", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        return View("MobileUbahPassword", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    if (Pass_Lama != Password)
                    {
                        ViewData["ErrorMsg"] = "Password yang digunakan untuk mengatur akun anda saat ini salah atau keliru. Silakan periksa kembali form isian.";
                        SetPrivilege();
                        return View("MobileUbahPassword", model);
                    }

                    // tahap 3: periksa password baru beserta konfirmasinya
                    if (Konfirmasi == Pass_Baru)
                    {
                        // tahap 4-A: susun stored procedure
                        Password = FormsAuthentication.HashPasswordForStoringInConfigFile(Pass_Baru, "SHA1");
                        cmd = new SqlCommand("SM_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Password;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Password";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            return RedirectToAction("EditSukses", "Notif", new { area = "Mobile" });
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database.";
                            model.NIK = NIK;
                            return View("MobileUbahPassword", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            model.NIK = NIK;
                            return View("MobileUbahPassword", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                    else
                    {
                        // tahap 4-B: jika password saling tidak cocok
                        ViewData["ErrorMsg"] = "Password dan konfirmasinya tidak sesuai. Silakan periksa kembali form isian dan ulangi proses setting.";
                        model.NIK = NIK;
                        return View("MobileUbahPassword", model);
                    }
                }
            }
            else
            {
                SetPrivilege();
                ViewData["ErrorMsg"] = "Anda belum mengisi password dan/atau konfirmasinya. Periksa kembali form isian dan ulangi proses berjalan.";
                return View("MobileUbahPassword", model);
            }
        }
        #endregion

        #endregion

        #region Pengaturan Cuti

        #region Pengajuan Cuti
        /*
         *  Sistem pengajuan cuti
         *  Proses pengajuan cuti khusus dan tahunan di dalam satu formulir pengajuan
         *  Tabel terlibat: TT_Pengajuan, TT_Approve_Khusus, TT_Approve_Tahunan, TM_Riwayat
         */

        /* BEGIN PROC */
        public ActionResult Pengajuan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                var model = new CutiModel();

                // isi kolom NIK dan seluruh kolom identitas karyawan secara otomatis
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
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            model.NIK = sdr["NIK"].ToString().Trim();
                            model.Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            model.Jenis_Kelamin = sdr["Jenis_Kelamin"].ToString().Trim();
                            model.Agama = sdr["Agama"].ToString().Trim();
                            model.Status_Nikah = sdr["Status_Nikah"].ToString().Trim();
                            model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            model.Cabang = sdr["Cabang"].ToString().Trim();
                            model.Jabatan = sdr["Jabatan"].ToString().Trim();
                            model.Departemen = sdr["Departemen"].ToString().Trim();
                            model.Tgl_Masuk = (DateTime)Convert.ToDateTime(sdr["Tgl_Masuk"].ToString().Trim());
                            model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            model.Pemberi = model.Nama_Atasan.ToString().Trim();

                            model.Jenis_Cuti = "Cuti Tahunan";
                            Tgl_Setuju = DateTime.Now;
                            model.Tgl_Pengajuan = Tgl_Setuju;
                            model.Tgl_Mulai = Tgl_Setuju;
                            model.Tgl_Selesai = Tgl_Setuju.AddDays(1d);
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }
                SetPrivilege();
                return View("MobilePengajuan", model);
            }
        }

        #region Partial View
        public ActionResult PengajuanPartial()
        {
            string Status_Nikah_T = (Request.Params["Status_Nikah"] ?? string.Empty).ToString();
            string Agama_T = (Request.Params["Agama"] ?? string.Empty).ToString();
            return PartialView("_Pengajuan", new CutiModel { Status_Nikah = Status_Nikah_T, Agama = Agama_T });
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pengajuan(CutiModel model, string returnUrl)
        {
            // mengingat model ini tidak dapat menggunakan prosedur IsValid,
            // pengecekan diubah menjadi pengujian model read-only & model yang dapat berubah
            using (var conn = new SqlConnection(connstring))
            {
                if (!string.IsNullOrEmpty(model.Nama_Karyawan) && !string.IsNullOrEmpty(model.Jenis_Cuti) && !string.IsNullOrEmpty(model.Keperluan))
                {
                    // tahap 1: pengujian tanggal user bergabung pertama kali di perusahaan
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"].ToString().Trim());
                            Email = sdr["Email"].ToString().Trim();
                            Email_Perusahaan = sdr["Email_Perusahaan"].ToString().Trim();
                            Area_Kerja = sdr["Area_Kerja"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses pencarian data. Pesan: \"" + ex.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    Tgl_Pengajuan = (DateTime)Convert.ToDateTime(model.Tgl_Pengajuan);
                    Tgl_Mulai = (DateTime)Convert.ToDateTime(model.Tgl_Mulai);
                    Tgl_Selesai = (DateTime)Convert.ToDateTime(model.Tgl_Selesai);

                    // tahap 2: ID cuti terakhir otomatis dimasukkan ke daftar cuti
                    // tabel acuan: TM_Riwayat
                    cmd = new SqlCommand("SM_Riwayat", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "ID Baru";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            ID_Cuti = sdr["ID_Baru"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses pencarian data. Pesan: \"" + ex.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // cek apakah sudah ada yang mengajukan cuti di database
                    if (string.IsNullOrEmpty(ID_Cuti))
                    {
                        ID_Baru = 0; // nilai standar
                    }
                    else
                    {
                        // cek tanggal terakhir pengajuan, jika berbeda gunakan indeks 01 kembali
                        DateTime TempDate = DateTime.Parse(ID_Cuti.Substring(0, 4) + "/" + ID_Cuti.Substring(4, 2) + "/" + ID_Cuti.Substring(6, 2));
                        if (TempDate.Date < DateTime.Now.Date || TempDate.Date > DateTime.Now.Date)
                        {
                            ID_Baru = 0;
                        }
                        else
                        {
                            try
                            {
                                ID_Baru = Convert.ToInt64(ID_Cuti.Substring(9, 4));
                            }
                            catch (ArgumentOutOfRangeException e)
                            {
                                ViewBag.Message = e.Message;
                                ID_Baru = Convert.ToInt64(ID_Cuti.Substring(9, 2));
                            }
                        }
                    }

                    ID_Baru++;
                    ID_Cuti = string.Format("{0:yyyyMMdd}", DateTime.Now.Date) + "-" + ID_Baru.ToString("0000");

                    Session["ID_Cuti"] = ID_Cuti;
                    ViewData["ID_Cuti"] = ID_Cuti;

                    // tahap 3: perhitungan masa cuti karyawan
                    // perhitungan dilakukan melalui SP, input parameter dari sistem ASP
                    /*
                     *  Perhatian:
                     *  Untuk Kantor Pusat/HO, Sabtu dan Minggu selalu masuk dalam perhitungan hari libur
                     *  Untuk cabang, hanya hari Minggu yang masuk dalam perhitungan hari libur
                     */

                    /* BEGIN PROC COUNT CUTI */
                    SqlParameter Jumlah_Hari_Cuti;

                    cmd = new SqlCommand("SP_Masa_Cuti", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Tgl_Mulai", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Tgl_Selesai", SqlDbType.DateTime).Value = Tgl_Selesai;
                    Jumlah_Hari_Cuti = new SqlParameter("@Jumlah_Hari", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(Jumlah_Hari_Cuti);

                    // masalah cabang yang menggunakan perhitungan 6 hari kerja
                    // parameter SP diubah sedemikian untuk cabang-cabang bersangkutan dengan seleksi kolom cabang
                    string Nama_Cabang = model.Cabang.Substring(0, 4);

                    if (Nama_Cabang != "Kant")
                    {
                        cmd.Parameters.Add("@Hari_Per_Pekan", SqlDbType.Int).Value = 6;
                    }
                    else
                    {
                        if (Area_Kerja == "Cabang")
                        {
                            cmd.Parameters.Add("@Hari_Per_Pekan", SqlDbType.Int).Value = 6;
                        }
                        else
                        {
                            cmd.Parameters.Add("@Hari_Per_Pekan", SqlDbType.Int).Value = 5;
                        }
                    }

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
                        return View("MobilePengajuan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    // default masa cuti dihitung dengan ketentuan di atas

                    /* END PROC COUNT CUTI */

                    Masa_Cuti = Convert.ToInt32(cmd.Parameters["@Jumlah_Hari"].Value.ToString());

                    // penentuan jenis cuti dan skema perhitungan cuti
                    if (Jenis_Cuti == null)
                    {
                        Jenis_Cuti = model.Jenis_Cuti.ToString().Trim();
                    }

                    Keperluan = model.Keperluan.ToString().Trim();

                    int t = 0;

                    // update 160225: filter berdasarkan kata per kata yang diuji
                    // jika keperluan yang diisi user termasuk dalam filter ini, secara otomatis sistem akan memilih berdasarkan data yang ada

                    // update 160105: menentukan jenis cuti secara otomatis berdasarkan keperluannya
                    // pencarian tidak case-sensitive

                    // NB: jenis cuti lainnya tidak termasuk di sini karena memiliki lebih dari satu kemungkinan deret string
                    // untuk kasus dua atau lebih string yang serupa, yang pertama dicari adalah yang paling spesifik terlebih dahulu

                    // contoh: untuk "saudara kandung meninggal" yang hampir serupa dengan "saudara kandung menikah", yang dicari terlebih dahulu adalah "meninggal" (cuti khusus), kemudian "saudara" (cuti tahunan) & terakhir "menikah" (cuti khusus).
                    cmd = new SqlCommand("SP_Pengajuan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Keperluan";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        using (dt = new DataTable())
                        {
                            dt.Load(sdr);
                            ArrayData = new String[dt.Rows.Count];
                            ResultData = new String[dt.Rows.Count];
                            for (t = 0; t < dt.Rows.Count; t++)
                            {
                                ArrayData[t] = dt.Rows[t][1].ToString().Trim(); // Array data = kumpulan kata kunci preset dalam pengambilan keputusan
                                ResultData[t] = dt.Rows[t][2].ToString().Trim(); // Result data = hasil keputusan yang diharapkan dari kata kunci sesuai indeks yang dimiliki
                            }
                        }

                        // pemeriksaan keperluan cuti
                        for (int n = 0; n < (ArrayData.Length); n++)
                        {
                            if (HasValues(Keperluan, ArrayData[n]) && !HasValues(Keperluan, "Meninggal"))
                            {
                                // tentukan jenis cuti berdasarkan nilai data yang sesuai & keluar dari loop
                                Jenis_Cuti = ResultData[n].ToString().Trim();
                                break;
                            }
                            else
                            {
                                Jenis_Cuti = model.Jenis_Cuti.ToString().Trim();
                                // ingat, jangan lakukan "break" di sini karena ada pemeriksaan terhadap kata berikutnya dalam daftar tabel sampai loop selesai
                                // pastikan jenis cuti diisi nilai default supaya jika tidak ada satu pun kondisi yang memenuhi kata kunci
                                // secara otomatis sistem mengambil nilai dari kotak isian
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = ex.Message;

                        // jika menemui masalah, gunakan perintah ini
                        // jangan keluarkan pesan error karena adanya exception memindahkan kendali penyesuaian jenis cuti ke mode manual

                        if (HasValues(Keperluan, "Meninggal"))
                        {
                            Jenis_Cuti = model.Jenis_Cuti.ToString().Trim();
                        }
                        // kriteria cuti tahunan: jika ada kata-kata berikut ini, sistem otomatis memilih "cuti tahunan" meskipun terdapat kata lain di sekitarnya.
                        else if (
                            HasValues(Keperluan, "Pribadi") || HasValues(Keperluan, "Keluarga")
                         || HasValues(Keperluan, "Libur") || HasValues(Keperluan, "Pulang")
                         || HasValues(Keperluan, "Mudik") || HasValues(Keperluan, "Kampung")
                         || HasValues(Keperluan, "Acara") || HasValues(Keperluan, "Rumah")
                         || HasValues(Keperluan, "Urus") || HasValues(Keperluan, "KTP")
                         || HasValues(Keperluan, "BPJS") || HasValues(Keperluan, "KPR")
                         || HasValues(Keperluan, "General") || HasValues(Keperluan, "Pindah")
                         || HasValues(Keperluan, "Medi") || HasValues(Keperluan, "Sakit")
                         || HasValues(Keperluan, "Operasi") || HasValues(Keperluan, "Istirahat")
                         || HasValues(Keperluan, "Aqiqah") || HasValues(Keperluan, "Tambahan")
                         || HasValues(Keperluan, "Saudara") || HasValues(Keperluan, "Ipar")
                         || HasValues(Keperluan, "Keponakan") || HasValues(Keperluan, "Kerabat")
                         || HasValues(Keperluan, "SIM") || HasValues(Keperluan, "Sepupu")
                         || HasValues(Keperluan, "Opname") || HasValues(Keperluan, "Izin")
                         || HasValues(Keperluan, "Kandung") || HasValues(Keperluan, "Jenguk")
                         || HasValues(Keperluan, "Wawancara") || HasValues(Keperluan, "Surat")
                         || HasValues(Keperluan, "Siap")
                        )
                        {
                            Jenis_Cuti = "Cuti Tahunan";
                        }
                        else if (Keperluan.IndexOf("Istri Melahirkan", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Jenis_Cuti = "Istri Melahirkan";
                        }
                        else if (Keperluan.IndexOf("Melahirkan", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Jenis_Cuti = "Melahirkan";
                        }
                        else if (
                           Keperluan.IndexOf("Menikah", StringComparison.OrdinalIgnoreCase) >= 0
                        || Keperluan.IndexOf("Pernikahan", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Jenis_Cuti = "Pernikahan Ybs.";
                        }
                        else if (Keperluan.IndexOf("Wisuda", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Jenis_Cuti = "Wisuda";
                        }
                        // bugfix 160128:
                        // khusus haji dan umroh terdapat proses administrasi calon haji & izin masuk, jadi proses administrasi tersebut didahulukan untuk diuji
                        else if (Keperluan.IndexOf("Administrasi", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Jenis_Cuti = "Cuti Tahunan";
                        }
                        else if (
                           Keperluan.IndexOf("Haji", StringComparison.OrdinalIgnoreCase) >= 0
                        || Keperluan.IndexOf("Berangkat Haji", StringComparison.OrdinalIgnoreCase) >= 0
                        || Keperluan.IndexOf("Naik Haji", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Jenis_Cuti = "Haji";
                        }
                        else if (Keperluan.IndexOf("Umroh", StringComparison.OrdinalIgnoreCase) >= 0 || Keperluan.IndexOf("Berangkat Umroh", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Jenis_Cuti = "Umroh";
                        }
                        else
                        {
                            Jenis_Cuti = model.Jenis_Cuti.ToString().Trim();
                        }
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    /* BEGIN PROC SPCL CUTI KHUSUS */
                    if (Jenis_Cuti != "Cuti Tahunan")
                    {
                        // susun parameter stored procedure cuti khusus
                        cmd = new SqlCommand("SM_Default", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Kode_Jenis", SqlDbType.NVarChar).Value = "Jenis_Cuti";
                        cmd.Parameters.Add("@Jenis_Isi", SqlDbType.NVarChar).Value = Jenis_Cuti;

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            sdr = cmd.ExecuteReader();
                            while (sdr.Read())
                            {
                                if (!Convert.IsDBNull(sdr["Jumlah_Hari"]))
                                {
                                    // jika cuti khusus yang dipilih memiliki masa cuti yang tetap
                                    // untuk cuti khusus berupa haji dan umroh perhitungannya serupa cuti tahunan tetapi tidak dibatasi 12 hari
                                    Masa_Cuti = Convert.ToInt32(sdr["Jumlah_Hari"].ToString());
                                    // manipulasi tanggal akhir masa cuti jika diketahui tanggal awal cuti ybs
                                    Tgl_Selesai = Tgl_Mulai.AddDays(Masa_Cuti - 1);
                                }
                            }

                            if (Jenis_Cuti == "Wisuda")
                            {
                                Masa_Cuti = 1;
                                Tgl_Selesai = Tgl_Mulai;
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobilePengajuan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobilePengajuan", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }

                        // periksa jangkauan tanggal cuti, apabila cuti khusus tersebut berakhir pada atau menyinggung hari libur nasional
                        // maka ditambahkan satu hari pada tanggal selesainya cuti khusus tsb

                        // note: penambahan ini TIDAK berlaku untuk cuti melahirkan
                        cmd = new SqlCommand("SP_Hitung_Libur", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Tgl_Awal", SqlDbType.DateTime).Value = Tgl_Mulai;
                        cmd.Parameters.Add("@Tgl_Akhir", SqlDbType.DateTime).Value = Tgl_Selesai;

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            sdr = cmd.ExecuteReader();
                            // hari libur
                            if (Jenis_Cuti != "Melahirkan" && Jenis_Cuti != "Haji" && Jenis_Cuti != "Umroh" && Jenis_Cuti != "Lainnya")
                            {
                                // hitung jumlah hari libur yang dilewati oleh range cuti
                                switch (sdr.Cast<Object>().Count())
                                {
                                    case 0:
                                        // tidak melakukan apa-apa
                                        break;
                                    case 1:
                                        Tgl_Selesai = Tgl_Selesai.AddDays(1d);
                                        break;
                                    case 2:
                                        Tgl_Selesai = Tgl_Selesai.AddDays(2d);
                                        break;
                                    case 3:
                                        Tgl_Selesai = Tgl_Selesai.AddDays(3d);
                                        break;
                                    case 4:
                                        Tgl_Selesai = Tgl_Selesai.AddDays(4d);
                                        break;
                                    default:
                                        // tidak melakukan apa-apa
                                        break;
                                }
                            }

                            // khusus untuk pusat: jika masa cuti selesai di hari Sabtu, ubah ke Senin pekan berikutnya
                            if (Nama_Cabang == "Kant" && (Tgl_Selesai.DayOfWeek == DayOfWeek.Saturday || (Tgl_Mulai.DayOfWeek == DayOfWeek.Friday && Masa_Cuti >= 2)))
                            {
                                Tgl_Selesai = Tgl_Selesai.AddDays(2d);
                            }
                            else
                            {
                                // tambah 1 hari jika masa cuti berakhir di hari Minggu
                                if (Tgl_Selesai.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    Tgl_Selesai = Tgl_Selesai.AddDays(1d);
                                }
                                // jika 1 hari sebelumnya adalah hari Minggu dan masa cuti melebihi dua hari,
                                // tambahkan satu hari kerja
                                if (Masa_Cuti > 2 && Tgl_Selesai.AddDays(-1d).DayOfWeek == DayOfWeek.Sunday)
                                {
                                    Tgl_Selesai = Tgl_Selesai.AddDays(1d);
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobilePengajuan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobilePengajuan", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }

                        // kedua jenis cuti ini tidak dibatasi tanggal, sederhananya kurangi tanggal akhir dengan tanggal awal
                        if (Jenis_Cuti == "Haji" || Jenis_Cuti == "Umroh" || Jenis_Cuti == "Lainnya")
                        {
                            if (Convert.ToInt32((Tgl_Selesai - Tgl_Mulai).TotalDays) == 0)
                            {
                                Masa_Cuti = 1;
                            }
                            else
                            {
                                Masa_Cuti = (int)(Tgl_Selesai - Tgl_Mulai).TotalDays;
                            }
                        }
                    }
                    /* END PROC SPCL CUTI KHUSUS */

                    // periksa tanggal selesainya cuti
                    if (Tgl_Selesai.Date < Tgl_Mulai.Date)
                    {
                        ViewData["ErrorMsg"] = "Tanggal selesai cuti harus lebih besar dari tanggal awal cuti.";
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }

                    // tahap 4: periksa input data tanggal
                    // pengajuan cuti hanya diperbolehkan satu jenis cuti dalam rentang tanggal tertentu dengan NIK yang sama
                    // backdate diperbolehkan dengan privilege user admin/manajer/SPV

                    // susun stored procedure
                    // pemeriksaan cuti pada rentang tanggal tertentu
                    cmd = new SqlCommand("SP_Pengajuan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    cmd.Parameters.Add("@Jenis_Kelamin", SqlDbType.NVarChar).Value = model.Jenis_Kelamin.ToString().Trim();
                    cmd.Parameters.Add("@Status_Nikah", SqlDbType.NVarChar).Value = model.Status_Nikah.ToString().Trim();
                    cmd.Parameters.Add("@Tgl_Mulai", SqlDbType.DateTime).Value = model.Tgl_Mulai.Date;
                    cmd.Parameters.Add("@Tgl_Selesai", SqlDbType.DateTime).Value = model.Tgl_Selesai.Date;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Check";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        if (sdr.HasRows)
                        {
                            // sistem filter otomatis untuk tanggal mulai cuti yang sama dan belum dibatalkan
                            ViewData["ErrorMsg"] = "Anda memiliki satu atau lebih cuti yang telah diajukan pada rentang tanggal yang dipilih. Silakan memilih rentang tanggal lainnya.";
                            SetPrivilege();
                            return View("MobilePengajuan", model);
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses pencarian data. Pesan: \"" + ex.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // tahap 5: susun stored procedure untuk mencatat data pengajuan cuti
                    NIK = model.NIK.ToString().Trim();
                    Nama_Karyawan = model.Nama_Karyawan.ToString().Trim();
                    Jenis_Kelamin = model.Jenis_Kelamin.ToString().Trim();
                    if (!string.IsNullOrEmpty(model.Agama))
                    {
                        Agama = model.Agama.ToString().Trim();
                    }
                    if (!string.IsNullOrEmpty(model.Status_Nikah))
                    {
                        Status_Nikah = model.Status_Nikah.ToString().Trim();
                    }
                    Perusahaan = model.Perusahaan.ToString().Trim();
                    Cabang = model.Cabang.ToString().Trim();
                    Jabatan = model.Jabatan.ToString().Trim();
                    Departemen = model.Departemen.ToString().Trim();
                    Tgl_Masuk = Convert.ToDateTime(model.Tgl_Masuk.ToString().Trim());
                    Nama_Atasan = model.Nama_Atasan.ToString().Trim();
                    if (!string.IsNullOrEmpty(model.Nama_Supervisor))
                    {
                        Nama_Supervisor = model.Nama_Supervisor.ToString().Trim();
                    }
                    if (Masa_Cuti == 90 && Jenis_Cuti.Equals("Cuti Tahunan"))
                    {
                        Jenis_Cuti = "Melahirkan";
                    }
                    Keperluan = model.Keperluan.ToString().Trim();
                    Pemberi = model.Pemberi.ToString().Trim();
                    Tgl_Pengajuan = Convert.ToDateTime(model.Tgl_Pengajuan.ToString());

                    // status default: belum disetujui (menunggu kebijakan atasan)
                    Status_Cuti = "Tunggu";

                    cmd = new SqlCommand("SP_Pengajuan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Jenis_Kelamin", SqlDbType.NVarChar).Value = Jenis_Kelamin;
                    if (!string.IsNullOrEmpty(model.Agama))
                    {
                        cmd.Parameters.Add("@Agama", SqlDbType.NVarChar).Value = Agama;
                    }
                    if (!string.IsNullOrEmpty(model.Status_Nikah))
                    {
                        cmd.Parameters.Add("@Status_Nikah", SqlDbType.NVarChar).Value = Status_Nikah;
                    }
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                    if (!string.IsNullOrEmpty(model.Nama_Supervisor))
                    {
                        cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
                    }
                    cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.DateTime).Value = Tgl_Masuk;
                    cmd.Parameters.Add("@Jenis_Cuti", SqlDbType.NVarChar).Value = Jenis_Cuti;
                    cmd.Parameters.Add("@Tgl_Pengajuan", SqlDbType.DateTime).Value = Tgl_Pengajuan;
                    cmd.Parameters.Add("@Tgl_Mulai", SqlDbType.DateTime).Value = Tgl_Mulai;
                    cmd.Parameters.Add("@Tgl_Selesai", SqlDbType.DateTime).Value = Tgl_Selesai;
                    cmd.Parameters.Add("@Masa_Cuti", SqlDbType.Int).Value = Masa_Cuti;
                    cmd.Parameters.Add("@Keperluan", SqlDbType.NVarChar).Value = Keperluan;
                    cmd.Parameters.Add("@Pemberi", SqlDbType.NVarChar).Value = Pemberi;
                    cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = DateTime.Now.Year;
                    cmd.Parameters.Add("@User_Login", SqlDbType.NVarChar).Value = User.Identity.Name;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    // jika ada input di 3 kotak opsional berikut ini
                    if (!string.IsNullOrEmpty(model.Keterangan))
                    {
                        Keterangan = model.Keterangan.ToString().Trim();
                        cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    }

                    if (!string.IsNullOrEmpty(model.Lokasi))
                    {
                        Lokasi = model.Lokasi.ToString().Trim();
                        cmd.Parameters.Add("@Lokasi", SqlDbType.NVarChar).Value = Lokasi;
                    }

                    if (!string.IsNullOrEmpty(model.Nomor_Kontak))
                    {
                        Nomor_Kontak = model.Nomor_Kontak.ToString().Trim();
                        cmd.Parameters.Add("@Nomor_Kontak", SqlDbType.NVarChar).Value = Nomor_Kontak;
                    }

                    // tahap 6: update tabel perhitungan jumlah cuti
                    // jika pengajuan = cuti tahunan, sisa cuti tahunan dikurangi
                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();

                        // temukan email atasan dari karyawan ybs
                        using (var sqlconn = new SqlConnection(connstring))
                        {
                            cmd = new SqlCommand("SM_Karyawan", sqlconn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Atasan;
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                            try
                            {
                                sqlconn.Open();
                                sdr = cmd.ExecuteReader();
                                while (sdr.Read())
                                {
                                    Privilege_At = sdr["Privilege"].ToString().Trim();
                                    if (!Convert.IsDBNull(sdr["Email_Perusahaan"]))
                                        Email_Atasan = sdr["Email_Perusahaan"].ToString().Trim();
                                    else
                                        Email_Atasan = sdr["Email"].ToString().Trim();
                                }
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("MobilePengajuan", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobilePengajuan", model);
                            }
                            finally
                            {
                                sdr.Close();
                                sqlconn.Close();
                            }

                            // jika tidak ada supervisor dari karyawan ybs, lewati prosedur ini
                            if (!string.IsNullOrEmpty(Nama_Supervisor))
                            {
                                // temukan email supervisor dari karyawan ybs
                                cmd = new SqlCommand("SM_Karyawan", sqlconn);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Supervisor;
                                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                                try
                                {
                                    sqlconn.Open();
                                    sdr = cmd.ExecuteReader();
                                    while (sdr.Read())
                                    {
                                        if (!Convert.IsDBNull(sdr["Email_Perusahaan"]))
                                            Email_Supervisor = sdr["Email_Perusahaan"].ToString().Trim();
                                        else
                                            Email_Supervisor = sdr["Email"].ToString().Trim();
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    // jika terdapat gangguan koneksi database
                                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                    SetPrivilege();
                                    return View("MobilePengajuan", model);
                                }
                                catch (Exception e)
                                {
                                    // jika terdapat gangguan lainnya
                                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                    SetPrivilege();
                                    return View("MobilePengajuan", model);
                                }
                                finally
                                {
                                    sdr.Close();
                                    sqlconn.Close();
                                }
                            }
                        }

                        // tahap 7: proses pengiriman email kepada atasan dari karyawan ybs
                        // sistem pengiriman email disusun secara otomatis dan dikirimkan melalui server publik
                        // setting SMTP server
                        try
                        {
                            Msg = new StringBuilder();
                            Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                            Msg.Append("<br />");
                            Msg.Append("Yth. " + Nama_Atasan + ",");
                            Msg.Append("<br /><br />");
                            if (Jenis_Cuti == "Cuti Tahunan")
                            {
                                Msg.Append("Anda telah menerima permohonan cuti tahunan dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Mulai) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Selesai) + " dengan keperluan: " + Keperluan + ".<br /><br />");
                                if (Privilege_At == "Admin")
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Admin/ApprovalTahunan\">http://" + ServerIP + "/Admin/ApprovalTahunan</a>.");
                                else if (Privilege_At == "Manager")
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalTahunan\">http://" + ServerIP + "/Manager/ApprovalTahunan</a>.");
                                else
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalTahunan\">http://" + ServerIP + "/Manager/ApprovalTahunan</a>.");

                                Msg.Append("<br /><br />");
                                Msg.Append("Apabila anda ingin mewakilkan persetujuan cuti kepada supervisor dari karyawan ybs, tautan/link berikut ini disediakan untuk supervisor: <a href=\"http://" + ServerIP + "/Supervisor/ApprovalTahunan\">http://" + ServerIP + "/Supervisor/ApprovalTahunan</a>.");
                            }
                            else
                            {
                                Msg.Append("Anda telah menerima permohonan cuti yaitu cuti " + Jenis_Cuti.ToLower() + " dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Mulai) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Selesai) + " dengan keperluan: " + Keperluan + ".<br /><br />");
                                if (Privilege_At == "Admin")
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Admin/ApprovalKhusus\">http://" + ServerIP + "/Admin/ApprovalKhusus</a>.");
                                else if (Privilege_At == "Manager")
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalKhusus\">http://" + ServerIP + "/Manager/ApprovalKhusus</a>.");
                                else
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalKhusus\">http://" + ServerIP + "/Manager/ApprovalKhusus</a>.");

                                Msg.Append("<br /><br />");
                                Msg.Append("Apabila anda ingin mewakilkan persetujuan cuti kepada supervisor dari karyawan ybs, tautan/link berikut ini disediakan untuk supervisor: <a href=\"http://" + ServerIP + "/Supervisor/ApprovalKhusus\">http://" + ServerIP + "/Supervisor/ApprovalKhusus</a>.");
                            }
                            Msg.Append("<br /><br />");
                            Msg.Append("Atas perhatiannya kami mengucapkan terima kasih.");
                            Msg.Append("<br /><br /><br /><br />");
                            Msg.Append("Jakarta, " + string.Format("{0:dd MMMM yyyy}", DateTime.Now));
                            Msg.Append("<br /><br />");
                            Msg.Append("Hormat kami,");
                            Msg.Append("<br /><br />");
                            Msg.Append("<br /><br />");
                            Msg.Append("<u>Human Resources Department ARISTA Group</u>");
                            Msg.Append("<br /><br />");
                            Msg.Append("Tembusan: " + Email_Atasan + ", " + Email_Supervisor);
                            Msg.Append("</span>");

                            Helpers.SendPengajuan(Msg.ToString(), Email_Atasan, Email_Supervisor);
                        }
                        catch (SmtpException smtpex)
                        {
                            ViewBag.Message = smtpex.Message;
                            Session["ErrorData"] = smtpex.Message;
                            return RedirectToAction("PengajuanError", "Notif", new { area = "Mobile" });
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = e.Message;
                            Session["ErrorData"] = e.Message;
                            return RedirectToAction("PengajuanError", "Notif", new { area = "Mobile" });
                        }
                        return RedirectToAction("PengajuanSukses", "Notif", new { area = "Mobile" });
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                else
                {
                    SetPrivilege();
                    // jika NIK tidak masuk melalui combo box, isi lewat model
                    if (NIK == null)
                    {
                        NIK = model.NIK.ToString().Trim();
                    }
                    else
                    {
                        NIK = model.NIK.ToString().Trim();
                    }

                    // membaca data karyawan
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
                            model.Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            model.Jenis_Kelamin = sdr["Jenis_Kelamin"].ToString().Trim();
                            model.Agama = sdr["Agama"].ToString().Trim();
                            model.Status_Nikah = sdr["Status_Nikah"].ToString().Trim();
                            model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            model.Cabang = sdr["Cabang"].ToString().Trim();
                            model.Jabatan = sdr["Jabatan"].ToString().Trim();
                            model.Departemen = sdr["Departemen"].ToString().Trim();
                            model.Tgl_Masuk = (DateTime)Convert.ToDateTime(sdr["Tgl_Masuk"].ToString().Trim());
                            model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            model.Pemberi = model.Nama_Atasan.ToString().Trim();
                        }
                        model.Jenis_Cuti = "Cuti Tahunan";

                        if (string.IsNullOrEmpty(model.Keperluan))
                            ViewData["ErrorMsg"] = "Keperluan cuti anda harus diisi.";

                        return View(model);
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pemuatan data karyawan. Silakan mencoba kembali.";
                        SetPrivilege();
                        return View("MobilePengajuan", model);
                    }
                }
            }
        }

        /* END PROC */
        #endregion

        #endregion

        #region Pengaturan Resign Karyawan

        #region Pengajuan Resign Karyawan
        public ActionResult Resignation()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                Session["ViewMode"] = "Desktop";

                var model = new ResignModel();
                // identitas karyawan langsung diisi
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
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            model.NIK = sdr["NIK"].ToString().Trim();
                            model.Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            model.Cabang = sdr["Cabang"].ToString().Trim();
                            model.Jabatan = sdr["Jabatan"].ToString().Trim();
                            model.Departemen = sdr["Departemen"].ToString().Trim();
                            model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resignation(ResignModel model)
        {
            if (!string.IsNullOrEmpty(model.NIK) && !string.IsNullOrEmpty(model.Tgl_Resign.ToString()) && !string.IsNullOrEmpty(model.Alasan))
            {
                // tahap 1: transfer model ke variabel kendali
                NIK = model.NIK.ToString().Trim();
                Nama_Karyawan = model.Nama_Karyawan.ToString().Trim();
                Perusahaan = model.Perusahaan.ToString().Trim();
                Cabang = model.Cabang.ToString().Trim();
                Jabatan = model.Jabatan.ToString().Trim();
                Departemen = model.Departemen.ToString().Trim();
                Nama_Atasan = model.Nama_Atasan.ToString().Trim();

                Tgl_Resign = Convert.ToDateTime(model.Tgl_Resign.ToString().Trim());
                Alasan = model.Alasan.ToString().Trim();

                // tentukan email perusahaan dari karyawan ybs
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
                            Email = sdr["Email"].ToString().Trim();
                            Email_Perusahaan = sdr["Email_Perusahaan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // tahap 2: susun stored procedure
                    cmd = new SqlCommand("SM_Resign", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                    cmd.Parameters.Add("@Tgl_Resign", SqlDbType.DateTime).Value = Tgl_Resign;
                    cmd.Parameters.Add("@Alasan", SqlDbType.NVarChar).Value = Alasan;
                    cmd.Parameters.Add("@Status_Resign", SqlDbType.NVarChar).Value = "Tunggu";
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        try
                        {
                            // setting SMTP server
                            Msg = new StringBuilder();
                            Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                            Msg.Append("<br />");
                            Msg.Append("Yth. " + Nama_Atasan + ",");
                            Msg.Append("<br /><br />");
                            Msg.Append("Anda menerima permohonan resign dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " dengan tanggal resign " + Tgl_Resign + ".<br />");
                            Msg.Append("Mohon segera memproses persetujuan resign dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalResign\">http://" + ServerIP + "/Manager/ApprovalResign</a>.");
                            Msg.Append("<br /><br />");
                            Msg.Append("Atas perhatiannya kami mengucapkan terima kasih.");
                            Msg.Append("<br /><br />");
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

                            Helpers.SendMutasi(Msg.ToString(), Email_Temp, Email_Perusahaan, Email);
                        }
                        catch (SmtpException smtpex)
                        {
                            return RedirectToAction("ResignError", "Notif", new { area = "Mobile" });
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = e.Message;
                            return RedirectToAction("ResignError", "Notif", new { area = "Mobile" });
                        }
                        return RedirectToAction("ResignSukses", "Notif", new { area = "Mobile" });
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                // jika user hanya memilih NIK
                SetPrivilege();
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            model.NIK = sdr["NIK"].ToString().Trim();
                            model.Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                            model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            model.Cabang = sdr["Cabang"].ToString().Trim();
                            model.Jabatan = sdr["Jabatan"].ToString().Trim();
                            model.Departemen = sdr["Departemen"].ToString().Trim();
                            model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat pencarian data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pembacaan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }
                return View(model);
            }
        }

        #endregion

        #endregion

        /*
         *  Bagian Data List
         *  Khusus untuk fungsi-fungsi view yang melibatkan data grid view
         */

        /* BEGIN DATA LIST */

        #region Riwayat Cuti
        public ActionResult Riwayat()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                return View("MobileRiwayat");
            }
        }

        public ActionResult RiwayatPartial()
        {
            // var model = DB.TM_Riwayats;
            var model = (from Riwayat in DB.TM_Riwayats
                         where Riwayat.Nama_Karyawan == User.Identity.Name
                         orderby Tgl_Pengajuan descending
                         select Riwayat);
            return PartialView("_MobileRiwayat", model.ToList());
        }
        #endregion

        #region Sisa Cuti Tahunan
        public ActionResult SisaTahunan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                var model = new FindModel();
                model.Tgl_Akhir = DateTime.Now.Date;
                record = Request.Cookies["LoginID"];
                if (record != null)
                {
                    if (!string.IsNullOrEmpty(record.Values["NIK"]))
                    {
                        NIK = record.Values["NIK"].ToString();
                    }

                    ViewData["NIK"] = NIK;
                    model.NIK = NIK;

                    // susun stored procedure
                    // mencari data karyawan yang sedang aktif login
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
                                Perusahaan = sdr["Perusahaan"].ToString().Trim();
                                Cabang = sdr["Cabang"].ToString().Trim();
                                Jabatan = sdr["Jabatan"].ToString().Trim();
                                Departemen = sdr["Departemen"].ToString().Trim();
                                Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                                Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"]);
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobileSisaTahunan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileSisaTahunan", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }

                        Tgl_Awal = Tgl_Masuk;
                        if (Tgl_Masuk.Month < DateTime.Now.Month)
                        {
                            Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year + 1));
                        }
                        else if (Tgl_Masuk.Day > DateTime.Now.Day && Tgl_Masuk.Month <= DateTime.Now.Month)
                        {
                            Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year));
                        }
                        else if (Tgl_Masuk.Day <= DateTime.Now.Day && Tgl_Masuk.Month <= DateTime.Now.Month)
                        {
                            Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year + 1));
                        }
                        else
                        {
                            Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year));
                        }

                        // interupsi jika karyawan punya tanggal resign
                        if (Tgl_Resign != null && Tgl_Resign != DateTime.MinValue)
                        {
                            Tgl_Akhir = Tgl_Resign;
                        }

                        Selisih_Awal = (Tgl_Akhir - Tgl_Awal).TotalDays;

                        // susun stored procedure
                        // masukkan parameter sesuai dengan prosedur sisa cuti

                        cmd = new SqlCommand("SP_Hitung_Cuti_Hangus", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                        cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                        cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                        cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                        cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                        cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                        cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
                        cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.DateTime).Value = Tgl_Masuk;
                        cmd.Parameters.Add("@Tgl_Akhir", SqlDbType.DateTime).Value = Tgl_Akhir;
                        cmd.Parameters.Add("@Temp1", SqlDbType.NVarChar).Value = Selisih_Awal.ToString(); // ambil dari combo box model
                        cmd.Parameters.Add("@Temp2", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Temp3", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Temp4", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@Temp5", SqlDbType.Int).Value = Tgl_Akhir.Year - Tgl_Masuk.Year;

                        if (Tgl_Resign != null && Tgl_Resign != DateTime.MinValue)
                        {
                            cmd.Parameters.Add("@Tgl_Resign", SqlDbType.DateTime).Value = Tgl_Resign;
                        }

                        // eksekusi parameter query di balik layar
                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            sda = new SqlDataAdapter(cmd);
                            ds = new DataSet();
                            sda.Fill(ds);
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobileSisaTahunan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileSisaTahunan", model);
                        }
                        finally
                        {
                            conn.Close();
                        }

                        // tentukan apakah masa cuti sudah melewati tanggal cuti massal tahun berjalan
                        cmd = new SqlCommand("SM_List_Cuti", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Tgl_Cuti", SqlDbType.DateTime).Value = Tgl_Akhir;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Max";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            sdr = cmd.ExecuteReader();
                            while (sdr.Read())
                            {
                                Total_CM = Convert.ToInt32(sdr["Hari_CM"]);
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobileSisaTahunan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileSisaTahunan", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }

                        if (Tgl_Akhir.Year > DateTime.Now.Year)
                            Tgl_Cuti_Massal = Convert.ToDateTime(DB.TM_List_Cutis.Where(dt => dt.Tgl_Cuti.Value.Year == Tgl_Akhir.Year - 1 && dt.ID_Daftar.Contains("P")).Select(dt => dt.Tgl_Cuti).Max());
                        else
                            Tgl_Cuti_Massal = Convert.ToDateTime(DB.TM_List_Cutis.Where(dt => dt.Tgl_Cuti.Value.Year == Tgl_Akhir.Year && dt.ID_Daftar.Contains("P")).Select(dt => dt.Tgl_Cuti).Max());

                        if (DateTime.Now.Date < Tgl_Cuti_Massal.Date)
                        {
                            if (Total_CM == 0)
                                ViewData["Catatan"] = "Sisa cuti pada tabel sudah termasuk potongan cuti massal tahun " + DateTime.Now.AddYears(-1).Year.ToString() + " sebelum periode berjalan selesai.";
                            else
                                ViewData["Catatan"] = "Sisa cuti pada tabel belum termasuk potongan cuti massal tahun " + DateTime.Now.Year.ToString() + " sebanyak " + Total_CM.ToString() + " hari sebelum periode berjalan selesai.";
                        }
                        else
                        {
                            ViewData["Catatan"] = "Sisa cuti pada tabel sudah termasuk potongan cuti massal tahun " + DateTime.Now.Year.ToString() + " sebelum periode berjalan selesai.";
                        }

                        return View("MobileSisaTahunan", model);
                    }
                }
                return View("MobileSisaTahunan", model);
            }
        }

        public ActionResult SisaTahunanPartial()
        {
            var model = DB.TM_Cutis;

            return PartialView("_MobileSisaTahunan", model.ToList());
        }
        #endregion

        #region Riwayat Pengajuan Cuti
        // riwayat pengajuan cuti
        public ActionResult RiwayatUser(FindModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                // penulisan data ke database riwayat pengajuan cuti
                ViewData["Tahun"] = DateTime.Now.Year;
                Session["Tahun"] = ViewData["Tahun"].ToString();

                // set stored procedure
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_Riwayat_User", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                    cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = Convert.ToInt32(DateTime.Now.Year);
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Cuti Khusus";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data dari database. Pesan: \"" + ex.Message + "\"";
                        return View(model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    cmd = new SqlCommand("SP_Riwayat_User", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                    cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = Convert.ToInt32(DateTime.Now.Year);
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Cuti Tahunan";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data dari database. Pesan: \"" + ex.Message + "\"";
                        return View(model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return View(model);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult RiwayatUser(FindModel se, string returnUrl)
        {
            // penulisan data ke database riwayat pengajuan cuti
            string Tahun_Cuti = se.String_Akhir.ToString();
            ViewData["Tahun"] = Tahun_Cuti;
            Session["Tahun"] = ViewData["Tahun"].ToString();

            se.String_Akhir = DateTime.Now.Year.ToString();

            // set stored procedure
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_Riwayat_User", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = Convert.ToInt32(Tahun_Cuti);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Cuti Khusus";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data dari database. Pesan: \"" + ex.Message + "\"";
                    return View(se);
                }
                finally
                {
                    conn.Close();
                }

                cmd = new SqlCommand("SP_Riwayat_User", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = Convert.ToInt32(Tahun_Cuti);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Cuti Tahunan";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data dari database. Pesan: \"" + ex.Message + "\"";
                    return View(se);
                }
                finally
                {
                    conn.Close();
                }
            }

            // deklarasi model
            SetPrivilege();
            return View(se);
        }

        public ActionResult RiwayatUserPartial()
        {
            return PartialView("_RiwayatUser");
        }
        #endregion

        /* END DATA LIST */

        #region Cuti Massal
        public ActionResult CutiMassal()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                return View();
            }
        }

        public ActionResult CutiMassalPartial()
        {
            var model = DB.TM_List_Cutis;
            return PartialView("_CutiMassal", model.ToList());
        }
        #endregion

        #region Kalender
        public ActionResult Kalender(EventModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                // refresh scheduler setiap kali data dibuka
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_Kalender", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Refresh";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi gangguan database selama proses pengisian data dengan pesan berikut: " + ex.Message;
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses pengisian data dengan pesan berikut: " + e.Message;
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                var DataRiwayat = (from Riwayat in DB.TM_Riwayats
                                   where Riwayat.Nama_Karyawan == User.Identity.Name && Riwayat.Status_Cuti == "Disetujui"
                                   select Riwayat).ToArray();
                // provisional -> dapat berubah ke model cuti massal tunggal jika diperlukan
                var DataMassal = (from Massal in DB.TM_List_Cutis select Massal).ToArray();
                var DataLibur = (from Libur in DB.TM_List_Liburs select Libur).ToArray();

                // masukkan riwayat cuti terlebih dahulu
                foreach (TM_Riwayat Riwayats in DataRiwayat)
                {
                    Nama_Event = Riwayats.Nama_Karyawan;
                    Jenis_Event = Riwayats.Jenis_Cuti;
                    Tgl_Awal = (DateTime)Riwayats.Tgl_Mulai;
                    Tgl_Akhir = (DateTime)Riwayats.Tgl_Selesai;

                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Kalender", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Event", SqlDbType.NVarChar).Value = Nama_Event;
                        cmd.Parameters.Add("@Jenis_Event", SqlDbType.NVarChar).Value = Jenis_Event;
                        cmd.Parameters.Add("@Tgl_Mulai", SqlDbType.DateTime).Value = Tgl_Awal;
                        cmd.Parameters.Add("@Tgl_Selesai", SqlDbType.DateTime).Value = Tgl_Akhir;
                        cmd.Parameters.Add("@Harian", SqlDbType.Bit).Value = false;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                        // eksekusi query insert
                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi gangguan database selama proses pengisian data dengan pesan berikut: " + ex.Message;
                            SetPrivilege();
                            return View("");
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses pengisian data dengan pesan berikut: " + e.Message;
                            SetPrivilege();
                            return View("");
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }

                // masukkan cuti massal
                foreach (TM_List_Cuti CTMassal in DataMassal)
                {
                    Nama_Event = CTMassal.Keterangan;
                    Jenis_Event = "Cuti Massal";
                    Tgl_Awal = (DateTime)CTMassal.Tgl_Cuti;
                    Tgl_Akhir = Tgl_Awal;

                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Kalender", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Event", SqlDbType.NVarChar).Value = Nama_Event;
                        cmd.Parameters.Add("@Jenis_Event", SqlDbType.NVarChar).Value = Jenis_Event;
                        cmd.Parameters.Add("@Tgl_Mulai", SqlDbType.DateTime).Value = Tgl_Awal;
                        cmd.Parameters.Add("@Tgl_Selesai", SqlDbType.DateTime).Value = Tgl_Akhir;
                        cmd.Parameters.Add("@Harian", SqlDbType.Bit).Value = true;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                        // eksekusi query insert
                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi gangguan database selama proses pengisian data dengan pesan berikut: " + ex.Message;
                            SetPrivilege();
                            return View(model);
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses pengisian data dengan pesan berikut: " + e.Message;
                            SetPrivilege();
                            return View(model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }

                // masukkan hari libur
                foreach (TM_List_Libur Liburan in DataLibur)
                {
                    Nama_Event = Liburan.Keterangan;
                    Jenis_Event = "Libur Nasional";
                    Tgl_Awal = (DateTime)Liburan.Tgl_Libur;
                    Tgl_Akhir = Tgl_Awal;

                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Kalender", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Event", SqlDbType.NVarChar).Value = Nama_Event;
                        cmd.Parameters.Add("@Jenis_Event", SqlDbType.NVarChar).Value = Jenis_Event;
                        cmd.Parameters.Add("@Tgl_Mulai", SqlDbType.DateTime).Value = Tgl_Awal;
                        cmd.Parameters.Add("@Tgl_Selesai", SqlDbType.DateTime).Value = Tgl_Akhir;
                        cmd.Parameters.Add("@Harian", SqlDbType.Bit).Value = true;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                        // eksekusi query insert
                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi gangguan database selama proses pengisian data dengan pesan berikut: " + ex.Message;
                            SetPrivilege();
                            return View(model);
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses pengisian data dengan pesan berikut: " + e.Message;
                            SetPrivilege();
                            return View(model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }

                // bentuk scheduling
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

        public bool HasValues(string source, string word)
        {
            return source.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion
	}
}