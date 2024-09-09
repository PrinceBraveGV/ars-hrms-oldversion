﻿using System;
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
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using AristaHRM.Models;

namespace AristaHRM.Areas.Mobile.Controllers
{
    [RouteArea("Mobile")]
    [RoutePrefix("Manager")]
    public class ManagerController : Controller
    {
        #region Daftar Variabel
        // variabel database
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
        // variabel string

        // model umum
        public string NIK;
        public string Nama_Karyawan;
        public string Password, Pass_Lama, Pass_Baru, Pass_Hash, Konfirmasi;
        public string Jenis_Kelamin;
        public string Tempat_Lahir;
        public string Email_Perusahaan, Email_Atasan, Email_Supervisor, Email_User;
        public string Alamat;
        public string Agama;
        public string Status_Nikah;
        public string Email, Email_Temp;
        public string Nama_Perusahaan, Nama_Cabang, Jabatan, Departemen, Nama_Atasan, Nama_Supervisor, Nama_Advisor;
        public string Pembuat, Privilege, Privilege_S, Privilege_At;
        public string Nama_Event, Jenis_Event;
        public string Notes, NotesBaru;

        // model khusus cuti
        public string ID_Cuti;
        public string Jenis_Cuti;
        public string Pemberi, Keperluan;
        public string Status_Cuti, Keterangan, No_Kontak;
        public string Nomor_Kontak;

        // model khusus perusahaan & cabang
        public string Kode_Perusahaan, Kode_Cabang;
        public string Lokasi, Kode_Singkat, Area_Kerja;

        // model khusus mutasi
        public string ID_Mutasi, ID_Pesan, Perusahaan_Baru, Cabang_Baru, Jabatan_Baru, Departemen_Baru, Atasan_Baru, Supervisor_Baru;
        public string Atasan_Ganti, Staf_Ganti;

        // model khusus resign
        public string Alasan;

        // model khusus feedback
        public string Subjek, Isi_Pesan;

        // model array
        public string[] ArrayData;
        public string[] ResultData;

        // variabel angka
        public long ID_Baru;
        public int Masa_Cuti;
        public int Libur_Awal, Libur_Akhir, Sisa_Tahunan;
        public int Jatah_Cuti, Total_Cuti, Cuti_Tahunan, Cuti_Massal, Sisa_Cuti, Total_CM;
        public int Counter, Total_Count;
        public double Selisih_Awal, Selisih_Akhir;

        // variabel tanggal
        public DateTime Tgl_Lahir;
        public DateTime Tgl_Masuk;
        public DateTime Periode_Awal, Periode_Akhir;
        public DateTime Tgl_Pengajuan, Tgl_Setuju, Tgl_Cuti_Massal;
        public DateTime Tgl_Mulai, Tgl_Selesai, Tgl_Awal, Tgl_Akhir;
        public DateTime Tgl_Mutasi;
        public DateTime Tgl_Resign;

        // variabel boolean
        public bool AutoApprove;

        // variabel lainnya

        #endregion

        // variabel lainnya
        public HttpCookie record;
        public StringBuilder Msg, Token;
        public MailMessage EmailRequest;
        public SmtpClient SMTPServer;
        public string Reset_Token;
        public HttpPostedFileBase UploadCtrl;
        public string Parameter;

        public string PhotoDir;

        #endregion

        /* BEGIN */

        #region Menu Manajer
        public ActionResult ManagerMenu()
        {
            Session["ViewMode"] = "Mobile";
            return View("MobileManagerMenu");
        }

        public ActionResult ManageKaryawan()
        {
            Session["ViewMode"] = "Mobile";
            return View("MobileManageKaryawan");
        }

        public ActionResult ManageCuti()
        {
            Session["ViewMode"] = "Mobile";
            return View("MobileManageCuti");
        }

        public ActionResult MasterData()
        {
            Session["ViewMode"] = "Mobile";
            return View("MobileMasterData");
        }


        #endregion

        #region Pengaturan Karyawan
        
        #region Mini Profil
        public ActionResult Profil(KaryawanModel model, string id)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
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

                if (!string.IsNullOrEmpty(id))
                {
                    // overwrite NIK dengan hasil request
                    NIK = id;
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
                            Nama_Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            Nama_Cabang = sdr["Cabang"].ToString().Trim();
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
                        return View("MobileProfil", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileProfil", model);
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
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
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
                        return View("MobileProfil", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileProfil", model);
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

                    return View("MobileProfil", model);
                }
            }
        }


        [HttpPost]
        public ActionResult Profil(KaryawanModel model)
        {
            SetPrivilege();
            ModelState.Clear();
            if (!String.IsNullOrEmpty(model.NIK))
            {
                using (var DB = new HRISContext())
                {
                    var result = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == model.NIK);
                    if (result != null)
                    {
                        model.Nama_Karyawan = result.Nama_Karyawan;
                        model.Jenis_Kelamin = result.Jenis_Kelamin;
                        model.Perusahaan = result.Perusahaan;
                        model.Cabang = result.Cabang;
                        model.Jabatan = result.Jabatan;
                        model.Departemen = result.Departemen;
                        model.Nama_Atasan = result.Nama_Atasan;
                        model.Nama_Supervisor = result.Nama_Supervisor;
                        model.Tgl_Masuk = result.Tgl_Masuk.Value;
                        model.Status_Karyawan = result.Status_Karyawan;
                        model.Tempat_Lahir = result.Tempat_Lahir;
                        model.Tgl_Lahir = result.Tgl_Lahir.Value;
                        model.Provinsi = result.Provinsi;
                        model.Kota = result.Kota;
                        model.Alamat = result.Alamat;
                        model.Agama = result.Agama;
                        model.Status_Nikah = result.Status_Nikah;
                    }
                }
            }

            return View("MobileProfil", model);
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
                SetPrivilege();
                // membaca NIK dari cookie bila ada
                if (ViewBag.Privilege == "Manager")
                {
                    var model = new PasswordModel();
                    // melalui cookie
                    record = Request.Cookies["LoginID"];
                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(record.Values["NIK"]))
                        {
                            NIK = record.Values["NIK"].ToString();
                            Nama_Karyawan = record.Values["Nama_Karyawan"].ToString();
                            model.NIK = NIK;
                            model.Nama_Karyawan = Nama_Karyawan;
                            return View("MobileUbahPassword", model);
                        }
                        else
                        {
                            // melalui sesi
                            model.NIK = Session["NIK"].ToString().Trim();
                            return View("MobileUbahPassword", model);
                        }
                    }
                    else
                    {
                        // melalui sesi
                        model.NIK = Session["NIK"].ToString().Trim();
                        return View("MobileUbahPassword", model);
                    }
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UbahPassword(PasswordModel model)
        {
            if (!string.IsNullOrEmpty(model.Pass_Baru) && !string.IsNullOrEmpty(model.Konfirmasi))
            {
                // tahap 1: transfer model ke variabel kendali
                Parameter = model.NIK.ToString().Trim();
                if (!string.IsNullOrEmpty(model.Pass_Lama))
                {
                    Pass_Lama = FormsAuthentication.HashPasswordForStoringInConfigFile(model.Pass_Lama.ToString(), "SHA1");
                }
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
                }

                if (!string.IsNullOrEmpty(Pass_Lama) && Pass_Lama != Password)
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
                            return RedirectToAction("EditSukses", "Notif", new { area = "Mobile" });
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
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
                }
                else
                {
                    // tahap 4-B: jika password saling tidak cocok
                    ViewData["ErrorMsg"] = "Password dan konfirmasinya tidak sesuai. Silakan periksa kembali form isian dan ulangi proses setting.";
                    model.NIK = NIK;
                    return View("MobileUbahPassword", model);
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

        #region Reset Password
        public ActionResult ResetPassword()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                return View();
            }
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
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                // tahap 3-A: jika NIK dari database dikenali
                if (!string.IsNullOrEmpty(NIK))
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
                        Msg.Append("Anda telah mengirim permintaan untuk proses reset password akun karyawan dengan NIK " + NIK + ".<br />");
                        Msg.Append("Jika anda merasa tidak melakukan permintaan demikian, abaikan pesan email ini.");
                        Msg.Append("<br /><br />");
                        Msg.Append("<b><u>Perhatian</u></b>: Link token reset yang disertakan pada email ini akan kadaluarsa secara otomatis setelah dipakai atau melebihi batas waktu 7 hari yang ditentukan oleh sistem.");
                        Msg.Append("<br /><br />");
                        Msg.Append("Silakan klik link token berikut ini untuk melakukan reset password: ");
                        Msg.Append("<br /><br />");
                        Msg.Append("<a href=\"" + string.Format("http://202.148.16.206:444/Home/EditAkun/{0}/{1}", NIK, Reset_Token) + "\">" + string.Format("http://202.148.16.206:444/Home/EditAkun/{0}/{1}", NIK, Reset_Token) + "</a>"); // => ubah localhost dengan alamat situs remote SMTP server
                        Msg.Append("<br /><br /><br /><br />");
                        Msg.Append("Jakarta, " + string.Format("{0:dd MMMM yyyy}", DateTime.Now));
                        Msg.Append("<br /><br />");
                        Msg.Append("Hormat kami,");
                        Msg.Append("<br /><br />");
                        Msg.Append("<br /><br />");
                        Msg.Append("<u>Human Resources Department ARISTA Group</u>");
                        Msg.Append("</span>");

                        // baca email mana yang valid
                        if (!string.IsNullOrEmpty(Email_Perusahaan))
                        {
                            Email_Temp = Email_Perusahaan;
                        }
                        else
                        {
                            Email_Temp = Email;
                        }

                        Helpers.SendReset(Msg.ToString(), Email_Temp);
                    }
                    catch (SmtpException smtpex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam proses pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator terkait untuk melaporkan masalah. Pesan: \"" + smtpex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan dalam pengiriman email ke " + Email_Temp + ". Silakan hubungi administrator terkait untuk melaporkan masalah. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }

                    // tulis token reset ke dalam database melalui stored procedure
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Reset_Token", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ID_Token", SqlDbType.NVarChar).Value = Reset_Token;
                        cmd.Parameters.Add("@Token", SqlDbType.NVarChar).Value = Reset_Token;
                        cmd.Parameters.Add("@Tgl_Buat", SqlDbType.NVarChar).Value = DateTime.Now;
                        cmd.Parameters.Add("@Tgl_Akhir", SqlDbType.NVarChar).Value = DateTime.Now.AddDays(7d);
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            return RedirectToAction("KonfirmasiReset", "Home");
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            model.NIK = NIK;
                            return View(model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            model.NIK = NIK;
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
                    ViewData["ErrorMsg"] = "NIK yang anda masukkan tidak terdaftar pada sistem. Silakan hubungi admin untuk mengetahui NIK anda.";
                    SetPrivilege();
                    return View(model);
                }
            }
            else
            {
                ViewData["ErrorMsg"] = "Anda belum mengisi kolom NIK untuk syarat reset password. Pastikan kotak isian NIK telah terisi dengan benar.";
                SetPrivilege();
                return View(model);
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
        public ActionResult Pengajuan(CutiModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    string Status_Nikah_T = (Request.Params["Status_Nikah"] ?? string.Empty).ToString();
                    string Agama_T = (Request.Params["Agama"] ?? string.Empty).ToString();
                    Tgl_Setuju = DateTime.Now;
                    model.Jenis_Cuti = "Cuti Tahunan";
                    model.Tgl_Pengajuan = Tgl_Setuju;
                    model.Tgl_Mulai = Tgl_Setuju;
                    model.Tgl_Selesai = Tgl_Setuju.AddDays(1d);

                    // default tampilan = user ybs
                    record = Request.Cookies["LoginID"];
                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(record.Values["NIK"]))
                        {
                            NIK = record.Values["NIK"].ToString();
                        }

                        if (!string.IsNullOrEmpty(record.Values["Departemen"]))
                        {
                            Departemen = record.Values["Departemen"].ToString();
                            ViewData["Departemen"] = Departemen;
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
                                model.Agama = sdr["Agama"].ToString().Trim();
                                model.Status_Nikah = sdr["Status_Nikah"].ToString().Trim();
                                model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                                model.Cabang = sdr["Cabang"].ToString().Trim();
                                model.Jabatan = sdr["Jabatan"].ToString().Trim();
                                model.Departemen = sdr["Departemen"].ToString().Trim();
                                model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                                model.Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"].ToString().Trim());
                                model.Pemberi = model.Nama_Atasan.ToString().Trim();
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
                    return View("MobilePengajuan", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        #region Partial View
        public ActionResult PengajuanPartial()
        {
            string Status_Nikah_T = (Request.Params["Status_Nikah"] ?? string.Empty).ToString();
            string Agama_T = (Request.Params["Agama"] ?? string.Empty).ToString();
            string Jenis_T = (Request.Params["Jenis_Kelamin"] ?? String.Empty).ToString();
            string NIK = (Request.Params["NIK"] ?? String.Empty).ToString();
            return PartialView("_Pengajuan", new CutiModel { Status_Nikah = Status_Nikah_T, Agama = Agama_T, Jenis_Kelamin = Jenis_T, NIK = NIK });
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pengajuan(CutiModel model, string returnUrl)
        {
            // mengingat model ini tidak dapat menggunakan prosedur IsValid,
            // pengecekan diubah menjadi pengujian model read-only & model yang dapat berubah
            record = Request.Cookies["LoginID"];
            using (var conn = new SqlConnection(connstring))
            {
                if (record != null)
                {
                    if (!string.IsNullOrEmpty(record.Values["NIK"]))
                    {
                        NIK = record.Values["NIK"].ToString();
                    }

                    if (!string.IsNullOrEmpty(record.Values["Departemen"]))
                    {
                        Departemen = record.Values["Departemen"].ToString();
                        ViewData["Departemen"] = Departemen;
                    }
                }

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

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

                    // pemeriksaan data karyawan
                    using (var DB = new HRISContext())
                    {
                        var karyawan = DB.TM_Karyawans.Where(x => x.NIK == model.NIK).Select(x => x.Nama_Karyawan).FirstOrDefault();
                        if (!String.IsNullOrEmpty(karyawan) && karyawan != model.Nama_Karyawan)
                        {
                            // jika nama karyawan tidak sesuai
                            ViewData["ErrorMsg"] = "Nama karyawan tidak sesuai dengan NIK yang digunakan.";
                            model.NIK = NIK;
                            model.Nama_Karyawan = karyawan;
                            SetPrivilege();
                            return View("MobilePengajuan", model);
                        }
                    }

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
                                ID_Baru = Convert.ToInt64(ID_Cuti.Substring(9, 2));
                                ViewBag.Message = e.Message;
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
                    Nama_Perusahaan = model.Perusahaan.ToString().Trim();
                    Nama_Cabang = model.Cabang.ToString().Trim();
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
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
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
                        }

                        // jika tidak ada supervisor dari karyawan ybs, lewati prosedur ini
                        if (!string.IsNullOrEmpty(Nama_Supervisor))
                        {
                            // temukan email supervisor dari karyawan ybs
                            using (var sqlconn = new SqlConnection(connstring))
                            {
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
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://202.148.16.206:444/Admin/ApprovalTahunan\">http://202.148.16.206:444/Admin/ApprovalTahunan</a>.");
                                else if (Privilege_At == "Manager")
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://202.148.16.206:444/Manager/ApprovalTahunan\">http://202.148.16.206:444/Manager/ApprovalTahunan</a>.");
                                else
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://202.148.16.206:444/Manager/ApprovalTahunan\">http://202.148.16.206:444/Manager/ApprovalTahunan</a>.");

                                Msg.Append("<br /><br />");
                                Msg.Append("Apabila anda ingin mewakilkan persetujuan cuti kepada supervisor dari karyawan ybs, tautan/link berikut ini disediakan untuk supervisor: <a href=\"http://202.148.16.206:444/Supervisor/ApprovalTahunan\">http://202.148.16.206:444/Supervisor/ApprovalTahunan</a>.");
                            }
                            else
                            {
                                Msg.Append("Anda telah menerima permohonan cuti yaitu cuti " + Jenis_Cuti.ToLower() + " dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Mulai) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Selesai) + " dengan keperluan: " + Keperluan + ".<br /><br />");
                                if (Privilege_At == "Admin")
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://202.148.16.206:444/Admin/ApprovalKhusus\">http://202.148.16.206:444/Admin/ApprovalKhusus</a>.");
                                else if (Privilege_At == "Manager")
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://202.148.16.206:444/Manager/ApprovalKhusus\">http://202.148.16.206:444/Manager/ApprovalKhusus</a>.");
                                else
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://202.148.16.206:444/Manager/ApprovalKhusus\">http://202.148.16.206:444/Manager/ApprovalKhusus</a>.");

                                Msg.Append("<br /><br />");
                                Msg.Append("Apabila anda ingin mewakilkan persetujuan cuti kepada supervisor dari karyawan ybs, tautan/link berikut ini disediakan untuk supervisor: <a href=\"http://202.148.16.206:444/Supervisor/ApprovalKhusus\">http://202.148.16.206:444/Supervisor/ApprovalKhusus</a>.");
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
                            return RedirectToAction("PengajuanError", "Notif", new { area = "Mobile" });
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = e.Message;
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
                    ModelState.Clear();
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
                        ViewData["Departemen"] = model.Departemen.ToString().Trim();
                        model.Jenis_Cuti = "Cuti Tahunan";

                        if (string.IsNullOrEmpty(model.Keperluan))
                            ViewData["ErrorMsg"] = "Keperluan cuti yang bersangkutan harus diisi.";

                        return View("MobilePengajuan", model);
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

        public PartialViewResult GetKaryawan(String NIK, CutiModel model)
        {
            SetPrivilege();
            // jika NIK tidak masuk melalui combo box, isi lewat model
            if (NIK == null)
            {
                NIK = model.NIK.ToString().Trim();
            }

            // membaca data karyawan
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
                        ViewData["ErrorMsg"] = "Keperluan cuti yang bersangkutan harus diisi.";

                    return PartialView("_PanelUserInfo", model);
                }
                catch (SqlException ex)
                {
                    ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pemuatan data karyawan. Silakan mencoba kembali. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return PartialView("_PanelUserInfo", model);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /* END PROC */

        #endregion

        #region Persetujuan Cuti Khusus
        // Bagian tampilan form persetujuan untuk cuti khusus
        public ActionResult ApprovalKhusus(String id)
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
                    var model = (from Approval in DB.TT_Approval_K
                                 where (Approval.Nama_Supervisor.Contains(User.Identity.Name.Trim()) || (Approval.Pemberi.Contains(User.Identity.Name.Trim()) || Approval.Nama_Atasan.Contains(User.Identity.Name.Trim())))
                                 select Approval).ToList();

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
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            sdr = cmd.ExecuteReader();
                            while (sdr.Read())
                            {
                                NIK = sdr["NIK"].ToString().Trim();
                                Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                                Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                Privilege = sdr["Privilege"].ToString().Trim();
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobileApprovalKhusus", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileApprovalKhusus", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }
                    }

                    // tentukan jenis grid yang ditampilkan
                    if (!string.IsNullOrEmpty(id))
                    {
                        ViewData["Selection"] = id;
                    }
                    else
                    {
                        ViewData["Selection"] = "Standard";
                    }

                    return View("MobileApprovalKhusus", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ApprovalKhususPartial()
        {
            // var model = DB.TT_Approval_K;

            var model = (from Approval in DB.TT_Approval_K
                         where (Approval.Nama_Supervisor.Contains(User.Identity.Name.Trim()) || (Approval.Pemberi.Contains(User.Identity.Name.Trim()) || Approval.Nama_Atasan.Contains(User.Identity.Name.Trim())))
                         select Approval);

            // dapatkan data atasan utama dari supervisor ybs
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SM_Karyawan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        if (sdr.HasRows)
                        {
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            Nama_Advisor = sdr["Nama_Advisor"].ToString().Trim();
                            ViewData["Departemen"] = Departemen;
                            ViewData["Nama_Atasan"] = Nama_Atasan;
                            ViewData["Nama_Advisor"] = Nama_Advisor;
                        }
                    }
                }

                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MobileApprovalKhusus", model.ToList());
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MobileApprovalKhusus", model.ToList());
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            return PartialView("_MobileApprovalKhusus", model.ToList());
        }

        public ActionResult MultiKhususPartial()
        {
            var model = (from Approval in DB.TT_Approval_K
                         where (Approval.Nama_Supervisor.Contains(User.Identity.Name.Trim()) || (Approval.Pemberi.Contains(User.Identity.Name.Trim()) || Approval.Nama_Atasan.Contains(User.Identity.Name.Trim())))
                         select Approval);

            // dapatkan data atasan utama dari supervisor ybs
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SM_Karyawan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        if (sdr.HasRows)
                        {
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            Nama_Advisor = sdr["Nama_Advisor"].ToString().Trim();
                            ViewData["Departemen"] = Departemen;
                            ViewData["Nama_Atasan"] = Nama_Atasan;
                            ViewData["Nama_Advisor"] = Nama_Advisor;
                        }
                    }
                }

                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MultiKhusus", model.ToList());
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MultiKhusus", model.ToList());
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            return PartialView("_MultiKhusus", model.ToList());
        }

        // Bagian script engine untuk proses persetujuan cuti khusus

        #region Proses Persetujuan
        public ActionResult SetujuCK(string id)
        {
            var model = (from Approval in DB.TT_Approval_K
                         where (Approval.Pemberi == User.Identity.Name)
                         select Approval).ToList();

            // tahap 1: tandai status cuti = setuju, tanggal persetujuan hari ini
            Status_Cuti = "Disetujui";
            Tgl_Setuju = DateTime.Now;

            if (!string.IsNullOrEmpty(id))
            {
                ID_Cuti = id;
                Session["ID_Cuti"] = ID_Cuti;
                ViewData["ID_Cuti"] = ID_Cuti;
            }
            else
            {
                SetPrivilege();
                return View("MobileApprovalKhusus", model);
            }

            // tahap 2: tentukan informasi pengajuan sesuai dengan ID cuti yang diberikan
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_ApproveKhusus", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        Masa_Cuti = Convert.ToInt32(sdr["Masa_Cuti"].ToString().Trim());
                        Session["NIK"] = NIK;
                        ViewData["NIK"] = NIK;
                        Session["Nama_Karyawan"] = Nama_Karyawan;
                        ViewData["Nama_Karyawan"] = Nama_Karyawan;
                        Session["Masa_Cuti"] = Masa_Cuti;
                        ViewData["Masa_Cuti"] = Masa_Cuti;
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            // tahap 3: update riwayat cuti
            // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_ApproveKhusus", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@Pemberi", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Tgl_Setuju", SqlDbType.DateTime).Value = Tgl_Setuju;
                cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                cmd.Parameters.Add("@Approved_By", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Setuju";

                // --> masukkan sesi terlebih dahulu
                Session["Setuju"] = Status_Cuti.ToLower();
                ViewData["Setuju"] = Status_Cuti.ToLower();
                Session["Jenis_Cuti"] = "Khusus";
                ViewData["Jenis_Cuti"] = "Khusus";

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
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    conn.Close();
                }

                // pencarian informasi email perusahaan
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
                        Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            ViewBag.Privilege = Privilege;

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
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    sqlconn.Close();
                }
            }

            // setting SMTP server
            try
            {
                Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti khusus anda dengan kode # " + ID_Cuti + " telah disetujui oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
            }
            catch (SmtpException smtpex)
            {
                ViewBag.Message = smtpex.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }

            // tahap 4: tampilkan konfirmasi persetujuan
            return RedirectToAction("MobileApprovalSukses", "Notif", new { area = "Mobile" });
        }

        #endregion

        // update 160129: proses persetujuan dengan multiple selection

        #region Proses Persetujuan Multiple Select
        [HttpPost]
        public ActionResult SetujuMSCK(String[] arg, CutiModel model)
        {
            ViewData["Selection"] = "Standard";
            Status_Cuti = "Disetujui";
            Tgl_Setuju = DateTime.Now;
            Session["Status_Cuti"] = Status_Cuti;

            int i = 0;

            // deklarasi email sender
            SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost;
            SMTPServer.Port = EmailHelper.EmailPort;
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser;
            SMTPServer.EnableSsl = true;

            // mendapatkan alamat email berdasarkan nama atasan yang aktif
            Nama_Atasan = (from Kar in DB.TM_Karyawans
                           where Kar.Nama_Karyawan == User.Identity.Name
                           select Kar.Nama_Karyawan).ToString().Trim();

            Email_Atasan = ((from Kar in DB.TM_Karyawans
                             where Kar.Nama_Atasan == Nama_Atasan
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Atasan == Nama_Atasan
                                                              select Kar.Email)).ToString().Trim() ?? string.Empty;

            // proses inti persetujuan
            // memeriksa indeks argumen paket HTTP untuk menemukan ID pengajuan cuti yang dipilih atasan
            foreach (String idc in arg)
            {
                ID_Cuti = idc.ToString().Trim();

                // proses ID terpilih satu per satu
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveKhusus", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            NIK = sdr["NIK"].ToString().Trim();
                            Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                Email_Perusahaan = (from Kar in DB.TM_Karyawans
                                    where Kar.NIK == NIK
                                    select Kar.Email_Perusahaan).ToString() ?? string.Empty;
                Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

                Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).ToString() ?? string.Empty;

                ViewBag.Privilege = Privilege;

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                // definisi body dasar email
                var Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti khusus anda dengan kode # " + ID_Cuti + " telah disetujui oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                // tahap 2: update riwayat cuti
                // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveKhusus", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Tgl_Setuju", SqlDbType.DateTime).Value = Tgl_Setuju;
                    cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Approved_By", SqlDbType.NVarChar).Value = User.Identity.Name;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Setuju";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    // update 170201: jika data pada list approval tidak terhapus, hapus dengan query ini
                    var query = DB.TT_Approval_K.FirstOrDefault(x => x.ID_Cuti == ID_Cuti);
                    if (query != null)
                    {
                        DB.TT_Approval_K.Remove(query);
                        DB.SaveChanges();
                    }
                }

                try
                {
                    Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);

                    // kosongkan kedua variabel email
                    Email_Perusahaan = "";
                    Email = "";
                }
                catch (SmtpException smtpex)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pengiriman email kepada atasan dengan pesan berikut: \"" + smtpex.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }
                catch (Exception e)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }

                i++;
                if (i % 3 == 0)
                    Thread.Sleep(10000);
            }

            return RedirectToAction("MultiApprovalSukses", "Notif", new { area = "Mobile" });
        }
        #endregion

        #endregion

        #region Persetujuan Cuti Tahunan
        // Bagian tampilan form persetujuan untuk cuti tahunan
        public ActionResult ApprovalTahunan(String id)
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
                    var model = (from Approval in DB.TT_Approval_T
                                 where (Approval.Nama_Supervisor.Contains(User.Identity.Name.Trim()) || (Approval.Pemberi.Contains(User.Identity.Name.Trim()) || Approval.Nama_Atasan.Contains(User.Identity.Name.Trim())))
                                 select Approval).ToList();

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
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            sdr = cmd.ExecuteReader();
                            while (sdr.Read())
                            {
                                NIK = sdr["NIK"].ToString().Trim();
                                Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                                Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                Privilege = sdr["Privilege"].ToString().Trim();
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobileApprovalTahunan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileApprovalTahunan", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }

                        // tentukan jenis grid yang ditampilkan
                        if (!string.IsNullOrEmpty(id))
                        {
                            ViewData["Selection"] = id;
                        }
                        else
                        {
                            ViewData["Selection"] = "Standard";
                        }
                    }

                    return View("MobileApprovalTahunan", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ApprovalTahunanPartial()
        {
            // var model = DB.TT_Approval_T;

            var model = (from Approval in DB.TT_Approval_T
                         where (Approval.Nama_Supervisor.Contains(User.Identity.Name.Trim()) || (Approval.Pemberi.Contains(User.Identity.Name.Trim()) || Approval.Nama_Atasan.Contains(User.Identity.Name.Trim())))
                         select Approval);

            // dapatkan data atasan utama dari supervisor ybs
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SM_Karyawan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        if (sdr.HasRows)
                        {
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            Nama_Advisor = sdr["Nama_Advisor"].ToString().Trim();
                            ViewData["Departemen"] = Departemen;
                            ViewData["Nama_Atasan"] = Nama_Atasan;
                            ViewData["Nama_Advisor"] = Nama_Advisor;
                        }
                    }
                }

                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MobileApprovalTahunan", model.ToList());
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MobileApprovalTahunan", model.ToList());
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            return PartialView("_MobileApprovalTahunan", model.ToList());
        }

        public ActionResult MultiTahunanPartial()
        {
            var model = (from Approval in DB.TT_Approval_T
                         where (Approval.Nama_Supervisor == User.Identity.Name) || (Approval.Nama_Atasan == User.Identity.Name)
                         select Approval);

            // dapatkan data atasan utama dari supervisor ybs
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SM_Karyawan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        if (sdr.HasRows)
                        {
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            Nama_Advisor = sdr["Nama_Advisor"].ToString().Trim();
                            ViewData["Departemen"] = Departemen;
                            ViewData["Nama_Atasan"] = Nama_Atasan;
                            ViewData["Nama_Advisor"] = Nama_Advisor;
                        }
                    }
                }

                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MultiTahunan", model.ToList());
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return PartialView("_MultiTahunan", model.ToList());
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            return PartialView("_MultiTahunan", model.ToList());
        }

        // Bagian script engine untuk proses persetujuan cuti tahunan

        #region Proses Persetujuan
        public ActionResult SetujuCT(string id)
        {
            var model = (from Approval in DB.TT_Approval_T
                         where (Approval.Pemberi == User.Identity.Name)
                         select Approval).ToList();

            // tahap 1: tandai status cuti = setuju, tanggal persetujuan hari ini
            Status_Cuti = "Disetujui";
            Tgl_Setuju = DateTime.Now;
            Session["Status_Cuti"] = Status_Cuti;

            if (!string.IsNullOrEmpty(id))
            {
                ID_Cuti = id;
                Session["ID_Cuti"] = ID_Cuti;
                ViewData["ID_Cuti"] = ID_Cuti;
            }
            else
            {
                // failsafe
                SetPrivilege();
                return View("MobileApprovalTahunan", model);
            }

            // tahap 2: tentukan informasi pengajuan sesuai dengan ID cuti yang diberikan
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_ApproveTahunan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        Masa_Cuti = Convert.ToInt32(sdr["Masa_Cuti"].ToString().Trim());
                        Session["NIK"] = NIK;
                        Session["Nama_Karyawan"] = Nama_Karyawan;
                        Session["Masa_Cuti"] = Masa_Cuti;
                        ViewData["NIK"] = NIK;
                        ViewData["Nama_Karyawan"] = Nama_Karyawan;

                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            // tahap 3: update riwayat cuti
            // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_ApproveTahunan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@Pemberi", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Tgl_Setuju", SqlDbType.DateTime).Value = Tgl_Setuju;
                cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                cmd.Parameters.Add("@Approved_By", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Setuju";

                // --> masukkan sesi terlebih dahulu
                Session["Setuju"] = Status_Cuti.ToLower();
                ViewData["Setuju"] = Status_Cuti.ToLower();
                Session["Jenis_Cuti"] = "Tahunan";
                ViewData["Jenis_Cuti"] = "Tahunan";

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
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    conn.Close();
                }

                // tahap 4: tentukan informasi cuti tahunan dalam tabel sisa cuti tahunan
                Tgl_Awal = new DateTime(DateTime.Now.Year, 1, 1);
                Tgl_Akhir = DateTime.Now.AddYears(1);

                cmd = new SqlCommand("SP_Sisa_Cuti", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Periode_Awal", SqlDbType.DateTime).Value = Tgl_Awal;
                cmd.Parameters.Add("@Periode_Akhir", SqlDbType.DateTime).Value = Tgl_Akhir;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Periode Tgl";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        Jatah_Cuti = Convert.ToInt32(sdr["Jatah_Cuti"].ToString().Trim());
                        Total_Cuti = Convert.ToInt32(sdr["Total_Cuti"].ToString().Trim());
                        Cuti_Tahunan = Convert.ToInt32(sdr["Cuti_Pribadi"].ToString().Trim());
                        Cuti_Massal = Convert.ToInt32(sdr["Cuti_Massal"].ToString().Trim());
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat pencarian data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pembacaan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            // tahap 5: tambah jumlah cuti tahunan yang telah diambil sesuai masa cuti
            Cuti_Tahunan += Masa_Cuti;

            // pencarian informasi email perusahaan
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
                        Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            ViewBag.Privilege = Privilege;

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
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    sqlconn.Close();
                }
            }

            // setting SMTP server
            try
            {
                Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti tahunan anda dengan kode # " + ID_Cuti + " telah disetujui oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
            }
            catch (SmtpException smtpex)
            {
                ViewBag.Message = smtpex.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }

            return RedirectToAction("MobileApprovalSukses", "Notif", new { area = "Mobile" });
        }

        #endregion

        // update 160129: proses persetujuan dengan multiple selection

        #region Proses Persetujuan Multi Select
        [HttpPost]
        public ActionResult SetujuMSCT(String[] arg, CutiModel model)
        {
            ViewData["Selection"] = "Standard";
            Status_Cuti = "Disetujui";
            Tgl_Setuju = DateTime.Now;
            Session["Status_Cuti"] = Status_Cuti;

            int i = 0;

            // deklarasi email sender
            SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost;
            SMTPServer.Port = EmailHelper.EmailPort;
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser;
            SMTPServer.EnableSsl = true;

            // mendapatkan alamat email berdasarkan nama atasan yang aktif
            Nama_Atasan = (from Kar in DB.TM_Karyawans
                           where Kar.Nama_Karyawan == User.Identity.Name
                           select Kar.Nama_Karyawan).ToString().Trim();

            Email_Atasan = ((from Kar in DB.TM_Karyawans
                             where Kar.Nama_Atasan == Nama_Atasan
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Atasan == Nama_Atasan
                                                              select Kar.Email)).ToString().Trim() ?? string.Empty;

            // proses inti persetujuan
            // memeriksa indeks argumen paket HTTP untuk menemukan ID pengajuan cuti yang dipilih atasan
            foreach (String idc in arg)
            {
                ID_Cuti = idc.ToString().Trim();

                // proses ID terpilih satu per satu
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveTahunan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            NIK = sdr["NIK"].ToString().Trim();
                            Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                Email_Perusahaan = (from Kar in DB.TM_Karyawans
                                    where Kar.NIK == NIK
                                    select Kar.Email_Perusahaan).ToString() ?? string.Empty;
                Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

                Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).ToString() ?? string.Empty;

                ViewBag.Privilege = Privilege;

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                // definisi body dasar email
                var Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti tahunan anda dengan kode # " + ID_Cuti + " telah disetujui oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                EmailRequest.Body = Msg.ToString();

                // tahap 2: update riwayat cuti
                // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveTahunan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Tgl_Setuju", SqlDbType.DateTime).Value = Tgl_Setuju;
                    cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Approved_By", SqlDbType.NVarChar).Value = User.Identity.Name;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Setuju";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    // update 170201: jika data pada list approval tidak terhapus, hapus dengan query ini
                    var query = DB.TT_Approval_T.FirstOrDefault(x => x.ID_Cuti == ID_Cuti);
                    if (query != null)
                    {
                        DB.TT_Approval_T.Remove(query);
                        DB.SaveChanges();
                    }
                }

                try
                {
                    Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);

                    // kosongkan kedua variabel email
                    Email_Perusahaan = "";
                    Email = "";
                }
                catch (SmtpException smtpex)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pengiriman email kepada atasan dengan pesan berikut: \"" + smtpex.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }
                catch (Exception e)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }

                i++;
                if (i % 3 == 0)
                    Thread.Sleep(10000);
            }

            return RedirectToAction("MultiApprovalSukses", "Notif", new { area = "Mobile" });
        }
        #endregion

        #endregion

        #region Penolakan Cuti Khusus
        // Bagian script engine untuk proses penolakan cuti khusus
        public ActionResult TolakCK(string id)
        {
            var model = (from Approval in DB.TT_Approval_K
                         where (Approval.Pemberi == User.Identity.Name)
                         select Approval).ToList();

            // tahap 1: tandai status cuti = ditolak
            Status_Cuti = "Ditolak";
            Session["Status_Cuti"] = Status_Cuti;

            if (!string.IsNullOrEmpty(id))
            {
                ID_Cuti = id;
                Session["ID_Cuti"] = ID_Cuti;
            }
            else
            {
                // failsafe
                SetPrivilege();
                return View("MobileApprovalKhusus", model);
            }

            // tahap 2: tentukan informasi pengajuan sesuai dengan ID cuti yang diberikan
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_ApproveKhusus", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        Session["NIK"] = NIK;
                        ViewData["NIK"] = NIK;
                        Session["Nama_Karyawan"] = Nama_Karyawan;
                        ViewData["Nama_Karyawan"] = Nama_Karyawan;
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }

                // tahap 3: update riwayat cuti
                // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
                cmd = new SqlCommand("SP_ApproveKhusus", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Tolak";

                Session["Setuju"] = Status_Cuti.ToLower();
                ViewData["Setuju"] = Status_Cuti.ToLower();
                Session["Jenis_Cuti"] = "Khusus";
                ViewData["Jenis_Cuti"] = "Khusus";

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
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    conn.Close();
                }
            }

            // pencarian informasi email perusahaan
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
                        Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            ViewBag.Privilege = Privilege;

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
                    return View("MobileApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    sqlconn.Close();
                }
            }

            // setting SMTP server
            try
            {
                Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti khusus anda dengan kode # " + ID_Cuti + " telah ditolak oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
            }
            catch (SmtpException smtpex)
            {
                ViewBag.Message = smtpex.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }

            // tampilkan konfirmasi penolakan cuti
            return RedirectToAction("MobileApprovalReject", "Notif", new { area = "Mobile" });
        }
        #endregion

        #region Penolakan Cuti Khusus (Multi Select)
        [HttpPost]
        public ActionResult TolakMSCK(String[] arg, String res, CutiModel model)
        {
            ViewData["Selection"] = "Standard";
            Status_Cuti = "Ditolak";
            Session["Status_Cuti"] = Status_Cuti;

            int i = 0;

            // deklarasi email sender
            SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost;
            SMTPServer.Port = EmailHelper.EmailPort;
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser;
            SMTPServer.EnableSsl = true;

            // mendapatkan alamat email berdasarkan nama atasan yang aktif
            Nama_Atasan = (from Kar in DB.TM_Karyawans
                           where Kar.Nama_Karyawan == User.Identity.Name
                           select Kar.Nama_Karyawan).ToString().Trim();

            Email_Atasan = ((from Kar in DB.TM_Karyawans
                             where Kar.Nama_Atasan == Nama_Atasan
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Atasan == Nama_Atasan
                                                              select Kar.Email)).ToString().Trim() ?? string.Empty;

            // proses inti persetujuan
            // memeriksa indeks argumen paket HTTP untuk menemukan ID pengajuan cuti yang dipilih atasan
            foreach (String idc in arg)
            {
                ID_Cuti = idc.ToString().Trim();

                // proses ID terpilih satu per satu
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveKhusus", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            NIK = sdr["NIK"].ToString().Trim();
                            Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                Email_Perusahaan = (from Kar in DB.TM_Karyawans
                                    where Kar.NIK == NIK
                                    select Kar.Email_Perusahaan).ToString() ?? string.Empty;
                Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

                Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).ToString() ?? string.Empty;

                ViewBag.Privilege = Privilege;

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                // definisi body dasar email
                var Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti khusus anda dengan kode # " + ID_Cuti + " telah ditolak oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                // tahap 2: update riwayat cuti
                // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveKhusus", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Tolak";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalKhusus", model);
                    }
                    finally
                    {
                        conn.Close();
                    }

                    // update 170201: jika data pada list approval tidak terhapus, hapus dengan query ini
                    var query = DB.TT_Approval_K.FirstOrDefault(x => x.ID_Cuti == ID_Cuti);
                    if (query != null)
                    {
                        DB.TT_Approval_K.Remove(query);
                        DB.SaveChanges();
                    }
                }

                try
                {
                    Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);

                    // kosongkan kedua variabel email
                    Email_Perusahaan = "";
                    Email = "";
                }
                catch (SmtpException smtpex)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pengiriman email kepada atasan dengan pesan berikut: \"" + smtpex.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }
                catch (Exception e)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }

                i++;
                if (i % 3 == 0)
                    Thread.Sleep(10000);
            }

            return RedirectToAction("MultiApprovalSukses", "Notif", new { area = "Mobile" });
        }
        #endregion

        #region Penolakan Cuti Tahunan
        // Bagian script engine untuk proses penolakan cuti tahunan
        public ActionResult TolakCT(string id)
        {
            var model = (from Approval in DB.TT_Approval_T
                         where (Approval.Pemberi == User.Identity.Name)
                         select Approval).ToList();

            // tahap 1: tandai status cuti = ditolak
            Status_Cuti = "Ditolak";
            Session["Status_Cuti"] = Status_Cuti;

            if (!string.IsNullOrEmpty(id))
            {
                ID_Cuti = id;
                Session["ID_Cuti"] = ID_Cuti;
                ViewData["ID_Cuti"] = ID_Cuti;
            }
            else
            {
                // failsafe
                SetPrivilege();
                return View("MobileApprovalTahunan", model);
            }

            // tahap 2: tentukan informasi pengajuan sesuai dengan ID cuti yang diberikan
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_ApproveTahunan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        Session["NIK"] = NIK;
                        ViewData["NIK"] = NIK;
                        Session["Nama_Karyawan"] = Nama_Karyawan;
                        ViewData["Nama_Karyawan"] = Nama_Karyawan;
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }

                // tahap 3: update riwayat cuti
                // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
                cmd = new SqlCommand("SP_ApproveTahunan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Tolak";

                // --> masukkan sesi terlebih dahulu
                Session["Setuju"] = Status_Cuti.ToLower();
                ViewData["Setuju"] = Status_Cuti.ToLower();
                Session["Jenis_Cuti"] = "Tahunan";
                ViewData["Jenis_Cuti"] = "Tahunan";

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
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    conn.Close();
                }

                // pencarian informasi email perusahaan
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
                        Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            ViewBag.Privilege = Privilege;

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
                    return View("MobileApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    sqlconn.Close();
                }
            }

            // setting SMTP server
            try
            {
                Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti tahunan anda dengan kode # " + ID_Cuti + " telah ditolak oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
            }
            catch (SmtpException smtpex)
            {
                ViewBag.Message = smtpex.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                return RedirectToAction("MobileApprovalError", "Notif", new { area = "Mobile" });
            }

            // tampilkan konfirmasi penolakan cuti
            return RedirectToAction("MobileApprovalReject", "Notif", new { area = "Mobile" });
        }
        #endregion

        #region Penolakan Cuti Tahunan (Multi Select)
        [HttpPost]
        public ActionResult TolakMSCT(String[] arg, String res, CutiModel model)
        {
            ViewData["Selection"] = "Standard";
            Status_Cuti = "Ditolak";
            Tgl_Setuju = DateTime.Now;
            Session["Status_Cuti"] = Status_Cuti;

            int i = 0;

            // deklarasi email sender
            SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost;
            SMTPServer.Port = EmailHelper.EmailPort;
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser;
            SMTPServer.EnableSsl = true;

            // mendapatkan alamat email berdasarkan nama atasan yang aktif
            Nama_Atasan = (from Kar in DB.TM_Karyawans
                           where Kar.Nama_Karyawan == User.Identity.Name
                           select Kar.Nama_Karyawan).ToString().Trim();

            Email_Atasan = ((from Kar in DB.TM_Karyawans
                             where Kar.Nama_Atasan == Nama_Atasan
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Atasan == Nama_Atasan
                                                              select Kar.Email)).ToString().Trim() ?? string.Empty;

            // proses inti persetujuan
            // memeriksa indeks argumen paket HTTP untuk menemukan ID pengajuan cuti yang dipilih atasan
            foreach (String idc in arg)
            {
                ID_Cuti = idc.ToString().Trim();

                // proses ID terpilih satu per satu
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveTahunan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            NIK = sdr["NIK"].ToString().Trim();
                            Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }
                }

                Email_Perusahaan = (from Kar in DB.TM_Karyawans
                                    where Kar.NIK == NIK
                                    select Kar.Email_Perusahaan).ToString() ?? string.Empty;
                Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

                Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).ToString() ?? string.Empty;

                ViewBag.Privilege = Privilege;

                // baca email mana yang valid
                if (!string.IsNullOrEmpty(Email_Perusahaan))
                {
                    Email_Temp = Email_Perusahaan;
                }
                else
                {
                    Email_Temp = Email;
                }

                // definisi body dasar email
                var Msg = new StringBuilder();
                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                Msg.Append("<br />");
                Msg.Append("Yth. " + Nama_Karyawan + ",");
                Msg.Append("<br /><br />");
                Msg.Append("Permohonan cuti tahunan anda dengan kode # " + ID_Cuti + " telah ditolak oleh atasan anda (" + Nama_Atasan + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                if (ViewBag.Privilege == "Manager")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Manager/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                }
                else
                {
                    Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                Msg.Append("</span>");

                // tahap 2: update riwayat cuti
                // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_ApproveTahunan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Tolak";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("MobileApprovalTahunan", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                try
                {
                    Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);

                    // kosongkan kedua variabel email
                    Email_Perusahaan = "";
                    Email = "";
                }
                catch (SmtpException smtpex)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pengiriman email kepada atasan dengan pesan berikut: \"" + smtpex.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }
                catch (Exception e)
                {
                    ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                    HttpContext.Response.AddHeader("FAILED_LOAD", "1");
                    Response.StatusCode = 401;
                    // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
                }

                i++;
                if (i % 3 == 0)
                    Thread.Sleep(10000);
            }

            return RedirectToAction("MultiApprovalSukses", "Notif", new { area = "Mobile" });
        }
        #endregion

        #region Pembatalan Cuti Khusus
        public ActionResult BatalKhusus(string id)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    // tahap 1: periksa ID cuti yang akan dibatalkan
                    var model = new CutiModel();
                    if (!string.IsNullOrEmpty(id))
                    {
                        string DateStamp = id.Substring(0, 8);
                        if (id.Length == 11)
                        {
                            Parameter = id.Substring(8, 2);
                        }
                        else if (id.Length == 13)
                        {
                            Parameter = id.Substring(8, 4);
                        }
                        string RestoreID = DateStamp + "-" + Parameter;
                        ID_Cuti = RestoreID;
                        Session["ID_Cuti"] = ID_Cuti;
                        ViewData["ID_Cuti"] = ID_Cuti;

                        // mencari data karyawan dengan kode ID cuti yang dimaksud
                        using (var conn = new SqlConnection(connstring))
                        {
                            cmd = new SqlCommand("SM_Riwayat", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                            try
                            {
                                if (conn.State != ConnectionState.Open) { conn.Open(); }
                                sdr = cmd.ExecuteReader();
                                while (sdr.Read())
                                {
                                    NIK = sdr["NIK"].ToString().Trim();
                                    Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                                }
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalKhusus", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalKhusus", model);
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
                            }

                            // tentukan email perusahaan dari karyawan ybs
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
                                    Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                    Privilege = sdr["Privilege"].ToString().Trim();
                                }
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalKhusus", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalKhusus", model);
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
                            }
                        }

                        ViewBag.Privilege = Privilege;

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
                                return View("MobileBatalKhusus", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalKhusus", model);
                            }
                            finally
                            {
                                sdr.Close();
                                sqlconn.Close();
                            }
                        }

                        // tahap 2: susun stored procedure
                        using (var conn = new SqlConnection(connstring))
                        {
                            cmd = new SqlCommand("SM_Riwayat", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                            cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = "Dibatalkan";
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Batal Cuti";

                            // eksekusi
                            try
                            {
                                if (conn.State != ConnectionState.Open) { conn.Open(); }
                                cmd.ExecuteNonQuery();

                                // setting SMTP server
                                try
                                {
                                    Msg = new StringBuilder();
                                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                                    Msg.Append("<br />");
                                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                                    Msg.Append("<br /><br />");
                                    if (ViewBag.Privilege == "Manager")
                                    {
                                        Msg.Append("Cuti khusus anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                                    }
                                    else if (ViewBag.Privilege == "Supervisor")
                                    {
                                        Msg.Append("Cuti khusus anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                                    }
                                    else
                                    {
                                        Msg.Append("Cuti khusus anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                                    Msg.Append("</span>");

                                    // baca email mana yang valid
                                    if (!string.IsNullOrEmpty(Email_Perusahaan))
                                    {
                                        Email_Temp = Email_Perusahaan;
                                    }
                                    else
                                    {
                                        Email_Temp = Email;
                                    }

                                    Helpers.SendPembatalan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                                }
                                catch (SmtpException smtpex)
                                {
                                    ViewBag.Message = smtpex.Message;
                                    return RedirectToAction("BatalCutiError", "Notif", new { area = "Mobile" });
                                }
                                catch (Exception e)
                                {
                                    ViewBag.Message = e.Message;
                                    return RedirectToAction("BatalCutiError", "Notif", new { area = "Mobile" });
                                }
                                return RedirectToAction("BatalCutiSukses", "Notif", new { area = "Mobile" });
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalKhusus", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalKhusus", model);
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                    else
                    {
                        return View("MobileBatalKhusus");
                    }
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult BatalKhususPartial()
        {
            // fungsi partial untuk pembatalan
            // hanya menampilkan cuti khusus saja
            var model = DB.TM_Riwayats;
            return PartialView("_MobileBatalKhusus", model.ToList());
        }

        #endregion

        #region Pembatalan Cuti Tahunan
        public ActionResult BatalTahunan(string id)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    var model = new CutiModel();
                    // tahap 1: periksa ID cuti yang akan dibatalkan
                    if (!string.IsNullOrEmpty(id))
                    {
                        string DateStamp = id.Substring(0, 8);
                        if (id.Length == 10)
                        {
                            Parameter = id.Substring(8, 2);
                        }
                        else if (id.Length == 12)
                        {
                            Parameter = id.Substring(8, 4);
                        }
                        string RestoreID = DateStamp + "-" + Parameter;
                        ID_Cuti = RestoreID;
                        Session["ID_Cuti"] = ID_Cuti;
                        ViewData["ID_Cuti"] = ID_Cuti;

                        // mencari data karyawan dengan kode ID cuti yang dimaksud
                        using (var conn = new SqlConnection(connstring))
                        {
                            cmd = new SqlCommand("SM_Riwayat", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                            try
                            {
                                if (conn.State != ConnectionState.Open) { conn.Open(); }
                                sdr = cmd.ExecuteReader();
                                while (sdr.Read())
                                {
                                    NIK = sdr["NIK"].ToString().Trim();
                                    Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                                }
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalTahunan", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalTahunan", model);
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
                            }

                            // tentukan email perusahaan dari karyawan ybs
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
                                    Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                    Privilege = sdr["Privilege"].ToString().Trim();
                                }
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalTahunan", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalTahunan", model);
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
                            }
                        }

                        ViewBag.Privilege = Privilege;

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
                                return View("MobileBatalTahunan", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalTahunan", model);
                            }
                            finally
                            {
                                sdr.Close();
                                sqlconn.Close();
                            }
                        }

                        // tahap 2: susun stored procedure
                        using (var conn = new SqlConnection(connstring))
                        {
                            cmd = new SqlCommand("SM_Riwayat", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                            cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = "Dibatalkan";
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Batal Cuti";

                            // eksekusi
                            try
                            {
                                if (conn.State != ConnectionState.Open) { conn.Open(); }
                                cmd.ExecuteNonQuery();

                                // setting SMTP server
                                try
                                {
                                    Msg = new StringBuilder();
                                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                                    Msg.Append("<br />");
                                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                                    Msg.Append("<br /><br />");
                                    if (ViewBag.Privilege == "Manager")
                                    {
                                        Msg.Append("Cuti tahunan anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Manager/Riwayat</a>");
                                    }
                                    else if (ViewBag.Privilege == "Supervisor")
                                    {
                                        Msg.Append("Cuti tahunan anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://202.148.16.206:444/Supervisor/Riwayat\">http://202.148.16.206:444/Supervisor/Riwayat</a>");
                                    }
                                    else
                                    {
                                        Msg.Append("Cuti tahunan anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://202.148.16.206:444/User/Riwayat\">http://202.148.16.206:444/User/Riwayat</a>");
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
                                    Msg.Append("</span>");

                                    // baca email mana yang valid
                                    if (!string.IsNullOrEmpty(Email_Perusahaan))
                                    {
                                        Email_Temp = Email_Perusahaan;
                                    }
                                    else
                                    {
                                        Email_Temp = Email;
                                    }

                                    Helpers.SendPembatalan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                                }
                                catch (SmtpException smtpex)
                                {
                                    ViewBag.Message = smtpex.Message;
                                    return RedirectToAction("BatalCutiError", "Notif", new { area = "Mobile" });
                                }
                                catch (Exception e)
                                {
                                    ViewBag.Message = e.Message;
                                    return RedirectToAction("BatalCutiError", "Notif", new { area = "Mobile" });
                                }
                                return RedirectToAction("BatalCutiSukses", "Notif", new { area = "Mobile" });
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalTahunan", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileBatalTahunan", model);
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                    else
                    {
                        SetPrivilege();
                        return View("MobileBatalTahunan");
                    }
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult BatalTahunanPartial()
        {
            // fungsi partial untuk pembatalan
            // hanya menampilkan cuti tahunan saja
            var model = DB.TM_Riwayats;
            return PartialView("_MobileBatalTahunan", model.ToList());
        }

        #endregion

        // fungsi persetujuan otomatis tidak terpakai lagi

        #region Persetujuan Cuti Otomatis (Deprecated)
        [Obsolete("Konsep persetujuan cuti otomatis telah dibatalkan. Harap tidak menggunakan metode ini.")]
        public ActionResult AutoApproval(ApprovalModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    // membaca cookie persetujuan otomatis
                    record = Request.Cookies["Approval"];
                    if (record != null)
                    {
                        // parameter cookie sebagai string, perlu dikonversi ke boolean
                        if (!string.IsNullOrEmpty(record.Values["Startup"]))
                        {
                            AutoApprove = Convert.ToBoolean(record.Values["Startup"]);
                            model.SetStartup = AutoApprove;
                        }
                        else
                        {
                            model.SetStartup = false;
                        }
                    }

                    return View("MobileApprovalTahunan", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [Obsolete("Konsep persetujuan cuti otomatis telah dibatalkan. Harap tidak menggunakan metode ini.")]
        public ActionResult AutoApproval(ApprovalModel model, String id)
        {
            // periksa apakah approval otomatis diaktifkan saat startup
            // jika ya, tulis pada sebuah cookie sebagai setting utama untuk halaman indeks

            if (model.SetStartup == true)
            {
                record = new HttpCookie("Approval");
                record["Startup"] = model.SetStartup.ToString().Trim();
                record.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Add(record);
                ViewData["Message"] = "Proses aktivasi berhasil. Semua pengajuan cuti yang belum disetujui setelah 7 hari akan disetujui secara otomatis setelah anda login ke sistem.";
            }

            if (model.Privilege != null)
            {
                // mode pencarian
                // pastikan isian kotak atasan dan pilihan privilege terisi
                if (model.Privilege == "Manager")
                {
                    model.IsManager = true;
                    model.IsSupervisor = false;
                }
                else if (model.Privilege == "Supervisor")
                {
                    model.IsManager = false;
                    model.IsSupervisor = true;
                }

                if (model.IsManager == true)
                {
                    // jika privilege manajer yang dipilih
                    if (!string.IsNullOrEmpty(model.Nama_Atasan.ToString()))
                        Nama_Atasan = model.Nama_Atasan.ToString().Trim();

                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Auto_Approve", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Pemberi", SqlDbType.NVarChar).Value = Nama_Atasan;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select MGR";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            ViewData["Message"] = "Proses persetujuan otomatis untuk nama atasan " + Nama_Atasan + " telah selesai dilakukan.";
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan pada database dalam proses persetujuan otomatis dengan pesan kesalahan: " + ex.Message;
                            SetPrivilege();
                            return View("MobileApprovalTahunan", model);
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses persetujuan otomatis dengan pesan kesalahan: " + e.Message;
                            SetPrivilege();
                            return View("MobileApprovalTahunan", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
                else if (model.IsSupervisor == true)
                {
                    // jika privilege supervisor yang dipilih
                    // standarnya sistem pengajuan cuti menempatkan manajer sebagai pemberi
                    // sehingga perlu mencari manajer ybs terlebih dahulu

                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Auto_Approve", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Pemberi", SqlDbType.NVarChar).Value = Nama_Atasan;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select SPV";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            ViewData["Message"] = "Proses persetujuan otomatis untuk nama atasan " + Nama_Atasan + " telah selesai dilakukan.";
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan pada database dalam proses persetujuan otomatis dengan pesan kesalahan: " + ex.Message;
                            SetPrivilege();
                            return View("MobileApprovalTahunan", model);
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses persetujuan otomatis dengan pesan kesalahan: " + e.Message;
                            SetPrivilege();
                            return View("MobileApprovalTahunan", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }

            SetPrivilege();
            return View("MobileApprovalTahunan", model);
        }

        #endregion

        #endregion

        #region Pengaturan Mutasi Karyawan

        #region Mutasi Karyawan Level Staf
        public ActionResult MutasiStaf(MutasiModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    // set default input dengan nama user yang aktif
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
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

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
                                model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            }
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses permintaan data profil. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("MobileMutasiStaff", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses permintaan data profil. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("MobileMutasiStaff", model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }
                    }
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MutasiStaf(MutasiModel model, string returnUrl)
        {
            if (!string.IsNullOrEmpty(model.NIK) && !string.IsNullOrEmpty(model.Tgl_Mutasi.ToString()) && !string.IsNullOrEmpty(model.Atasan_Baru))
            {
                // tahap 1: transfer model ke variabel kendali
                NIK = model.NIK.ToString().Trim();
                Nama_Karyawan = model.Nama_Karyawan.ToString().Trim();
                Nama_Perusahaan = model.Perusahaan.ToString().Trim();
                Nama_Cabang = model.Cabang.ToString().Trim();
                Jabatan = model.Jabatan.ToString().Trim();
                Departemen = model.Departemen.ToString().Trim();
                Tgl_Mutasi = (DateTime)Convert.ToDateTime(model.Tgl_Mutasi);

                // periksa keberadaan input pada perusahaan, cabang, jabatan, departemen & atasan
                if (!string.IsNullOrEmpty(model.Perusahaan_Baru))
                {
                    Perusahaan_Baru = model.Perusahaan_Baru.ToString().Trim();
                }

                if (!string.IsNullOrEmpty(model.Cabang_Baru))
                {
                    Cabang_Baru = model.Cabang_Baru.ToString().Trim();
                }

                if (!string.IsNullOrEmpty(model.Jabatan_Baru))
                {
                    Jabatan_Baru = model.Jabatan_Baru.ToString().Trim();
                }

                if (!string.IsNullOrEmpty(model.Departemen_Baru))
                {
                    Departemen_Baru = model.Departemen_Baru.ToString().Trim();
                }

                if (!string.IsNullOrEmpty(model.Atasan_Baru))
                {
                    Atasan_Baru = model.Atasan_Baru.ToString().Trim();
                }

                if (!string.IsNullOrEmpty(model.Supervisor_Baru))
                {
                    Supervisor_Baru = model.Supervisor_Baru.ToString().Trim();
                }
                if (model.Staf_Ganti != null)
                {
                    Staf_Ganti = model.Staf_Ganti.ToString().Trim();
                }

                if (!string.IsNullOrEmpty(model.Keterangan))
                {
                    Keterangan = model.Keterangan.ToString().Trim();
                }

                // tentukan privilege asal dari karyawan ybs
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
                            Privilege_S = sdr["Privilege"].ToString().Trim();
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

                    // tahap 2: tentukan ID atau nomor pengenal mutasi
                    cmd = new SqlCommand("SM_Mutasi", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "ID Baru";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            ID_Mutasi = sdr["ID_Baru"].ToString().Trim();
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

                if (!string.IsNullOrEmpty(ID_Mutasi))
                {
                    ID_Baru = Convert.ToInt64(ID_Mutasi);
                }
                else
                {
                    ID_Baru = 0;
                }

                ID_Baru++;
                ID_Mutasi = ID_Baru.ToString("0000000000000000");

                switch (Jabatan_Baru)
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

                // tahap 3: susun stored procedure
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Mutasi", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Mutasi", SqlDbType.NVarChar).Value = ID_Mutasi;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                    cmd.Parameters.Add("@Tgl_Mutasi", SqlDbType.DateTime).Value = Tgl_Mutasi;
                    cmd.Parameters.Add("@Privilege", SqlDbType.NVarChar).Value = Privilege_S;
                    cmd.Parameters.Add("@Privilege_Baru", SqlDbType.NVarChar).Value = Privilege;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Karyawan";

                    // periksa keberadaan input pada perusahaan, cabang, jabatan, departemen & atasan
                    if (!string.IsNullOrEmpty(model.Perusahaan_Baru))
                    {
                        Perusahaan_Baru = model.Perusahaan_Baru.ToString().Trim();
                        cmd.Parameters.Add("@Perusahaan_Baru", SqlDbType.NVarChar).Value = Perusahaan_Baru;
                    }

                    if (!string.IsNullOrEmpty(model.Cabang_Baru))
                    {
                        Cabang_Baru = model.Cabang_Baru.ToString().Trim();
                        cmd.Parameters.Add("@Cabang_Baru", SqlDbType.NVarChar).Value = Cabang_Baru;
                    }

                    if (!string.IsNullOrEmpty(model.Jabatan_Baru))
                    {
                        Jabatan_Baru = model.Jabatan_Baru.ToString().Trim();
                        cmd.Parameters.Add("@Jabatan_Baru", SqlDbType.NVarChar).Value = Jabatan_Baru;
                    }

                    if (!string.IsNullOrEmpty(model.Departemen_Baru))
                    {
                        Departemen_Baru = model.Departemen_Baru.ToString().Trim();
                        cmd.Parameters.Add("@Departemen_Baru", SqlDbType.NVarChar).Value = Departemen_Baru;
                    }

                    if (!string.IsNullOrEmpty(model.Atasan_Baru))
                    {
                        Atasan_Baru = model.Atasan_Baru.ToString().Trim();
                        cmd.Parameters.Add("@Atasan_Baru", SqlDbType.NVarChar).Value = Atasan_Baru;
                    }

                    if (!string.IsNullOrEmpty(model.Supervisor_Baru))
                    {
                        Supervisor_Baru = model.Supervisor_Baru.ToString().Trim();
                        cmd.Parameters.Add("@Supervisor_Baru", SqlDbType.NVarChar).Value = Supervisor_Baru;
                    }

                    if (model.Staf_Ganti != null)
                    {
                        Staf_Ganti = model.Staf_Ganti.ToString().Trim();
                        cmd.Parameters.Add("@Staf_Ganti", SqlDbType.NVarChar).Value = Staf_Ganti;
                    }

                    if (!string.IsNullOrEmpty(model.Keterangan))
                    {
                        Keterangan = model.Keterangan.ToString().Trim();
                        cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    }

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
                            Msg.Append("Yth. " + Nama_Karyawan + ",");
                            Msg.Append("<br /><br />");
                            Msg.Append("Anda telah melakukan proses mutasi karyawan dengan rincian sebagai berikut: <br />");
                            if (!string.IsNullOrEmpty(model.Perusahaan_Baru) && Perusahaan_Baru != Nama_Perusahaan)
                            {
                                Msg.Append("Perusahaan Baru: " + Perusahaan_Baru + "<br />");
                            }
                            else if (!string.IsNullOrEmpty(model.Cabang_Baru) && Cabang_Baru != Nama_Cabang)
                            {
                                Msg.Append("Cabang Baru: " + Cabang_Baru + "<br />");
                            }
                            else if (!string.IsNullOrEmpty(model.Jabatan_Baru) && Jabatan_Baru != Jabatan)
                            {
                                Msg.Append("Jabatan Baru: " + Jabatan_Baru + "<br />");
                            }
                            else if (!string.IsNullOrEmpty(model.Departemen_Baru) && Departemen_Baru != Departemen)
                            {
                                Msg.Append("Departemen Baru: " + Departemen_Baru + "<br />");
                            }
                            else if (!string.IsNullOrEmpty(Atasan_Baru) && Atasan_Baru != Nama_Atasan)
                            {
                                Msg.Append("Atasan Baru: " + Atasan_Baru + "<br />");
                            }

                            Msg.Append("Mohon untuk segera memberitahukan isi surat ini kepada atasan anda.");
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

                            // baca email mana yang valid
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
                            ViewBag.Message = smtpex.Message;
                            return RedirectToAction("MutasiError", "Notif", new { area = "Mobile" });
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = e.Message;
                            return RedirectToAction("MutasiError", "Notif", new { area = "Mobile" });
                        }

                        return RedirectToAction("MutasiSukses", "Notif", new { area = "Mobile" });
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

        #region Partial View Mutasi
        public ActionResult CabangBaruPartial()
        {
            string Nama_Perusahaan = (Request.Params["Perusahaan_Baru"] != null ? Request.Params["Perusahaan_Baru"].ToString() : string.Empty);
            return PartialView(new MutasiModel { Perusahaan_Baru = Nama_Perusahaan });
        }

        public ActionResult AtasanBaruPartial()
        {
            string Departemen = (Request.Params["Departemen_Baru"] != null ? Request.Params["Departemen_Baru"].ToString() : string.Empty);
            return PartialView(new MutasiModel { Departemen_Baru = Departemen });
        }

        public ActionResult SupervisorBaruPartial()
        {
            string Departemen = (Request.Params["Departemen_Baru"] != null ? Request.Params["Departemen_Baru"].ToString() : string.Empty);
            return PartialView(new MutasiModel { Departemen_Baru = Departemen });
        }
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
                SetPrivilege();
                var model = new ResignModel();
                if (ViewBag.Privilege == "Manager")
                {
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
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
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
                Nama_Perusahaan = model.Perusahaan.ToString().Trim();
                Nama_Cabang = model.Cabang.ToString().Trim();
                Jabatan = model.Jabatan.ToString().Trim();
                Departemen = model.Departemen.ToString().Trim();
                Nama_Atasan = model.Nama_Atasan.ToString().Trim();
                Nama_Supervisor = model.Nama_Supervisor.ToString().Trim();

                Tgl_Resign = Convert.ToDateTime(model.Tgl_Resign);
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
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                    cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
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
                            Msg.Append("Anda menerima permohonan resign dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " dengan tanggal resign " + Tgl_Resign + ".<br /><br />");
                            Msg.Append("Mohon segera memproses persetujuan resign dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://202.148.16.206:444/Manager/ApprovalResign\">http://202.148.16.206:444/Manager/ApprovalResign</a>.");
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

                            // baca email mana yang valid
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
                            ViewBag.Message = smtpex.Message;
                            return RedirectToAction("ResignError", "Notif", new { area = "Mobile" });
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = e.Message;
                            return RedirectToAction("ResignError", "Notif", new { area = "Mobile" });
                        }

                        return RedirectToAction("ResignSukses");
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

        #region Persetujuan Resign Karyawan
        public ActionResult ApprovalResign()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ApprovalResignPartial()
        {
            var model = DB.TM_Resigns;
            return PartialView("_ApprovalResign", model.ToList());
        }

        public ActionResult SetujuResignation(string id, ResignModel model)
        {
            // tahap 1: NIK diberikan melalui parameter URL (id)
            if (!string.IsNullOrEmpty(id))
            {
                NIK = id;
                Session["NIK"] = NIK;
            }
            else
            {
                SetPrivilege();
                return View("MobileApprovalResign", model);
            }

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

                // tahap 2: tentukan informasi resign sesuai dengan NIK
                cmd = new SqlCommand("SM_Resign", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        Session["NIK"] = NIK;
                        Session["Nama_Karyawan"] = Nama_Karyawan;
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalResign", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }

                /* Proses approval resign sama halnya dengan proses approval cuti */
                // tahap 3: update keadaan resign karyawan
                cmd = new SqlCommand("SM_Resign", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Tgl_Resign", SqlDbType.NVarChar).Value = Tgl_Resign;
                cmd.Parameters.Add("@Alasan", SqlDbType.NVarChar).Value = Alasan;
                cmd.Parameters.Add("@Status_Resign", SqlDbType.NVarChar).Value = "Disetujui";
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Setuju";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.ExecuteNonQuery();
                    // setting SMTP server
                    try
                    {
                        Msg = new StringBuilder();
                        Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                        Msg.Append("<br />");
                        Msg.Append("Yth. " + Nama_Karyawan + ",");
                        Msg.Append("<br /><br />");
                        Msg.Append("Permohonan resign anda yang dilakukan tanggal " + Tgl_Resign + " telah disetujui oleh atasan anda (" + Nama_Atasan + ").<br /><br />");
                        Msg.Append("Terima kasih atas kontribusi anda selama bekerja di ARISTA Group. Mohon memberitahukan isi surat ini kepada atasan anda serta mengikuti prosedur resign berikutnya yang telah ditentukan oleh HRD.");
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

                        // baca email mana yang valid
                        if (!string.IsNullOrEmpty(Email_Perusahaan))
                        {
                            Email_Temp = Email_Perusahaan;
                        }
                        else
                        {
                            Email_Temp = Email;
                        }

                        Helpers.SendResignation(Msg.ToString(), Email_Temp, Email_Perusahaan, Email);
                    }
                    catch (SmtpException smtpex)
                    {
                        ViewBag.Message = smtpex.Message;
                        return RedirectToAction("ResignSetuju", "Admin");
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message = e.Message;
                        return RedirectToAction("ResignSetuju", "Admin");
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalResign", model);
                }
                finally
                {
                    conn.Close();
                }
            }
            return RedirectToAction("ResignSetuju");
        }

        public ActionResult BatalResign(string id, ResignModel model)
        {
            // tahap 1: NIK diberikan melalui parameter URL (id)
            if (!string.IsNullOrEmpty(id))
            {
                NIK = id;
                Session["NIK"] = NIK;
            }
            else
            {
                SetPrivilege();
                return View("MobileApprovalResign", model);
            }

            // tahap 2: tentukan informasi resign sesuai dengan NIK
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SM_Resign", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
                        Session["NIK"] = NIK;
                        Session["Nama_Karyawan"] = Nama_Karyawan;
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalResign", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }

                /* Proses pembatalan resign sama halnya dengan proses pembatalan cuti */
                // tahap 3: update keadaan resign karyawan
                cmd = new SqlCommand("SM_Resign", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Tgl_Resign", SqlDbType.NVarChar).Value = Tgl_Resign;
                cmd.Parameters.Add("@Alasan", SqlDbType.NVarChar).Value = Alasan;
                cmd.Parameters.Add("@Status_Resign", SqlDbType.NVarChar).Value = "Disetujui";
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Batal";

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
                    return View("MobileApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("MobileApprovalResign", model);
                }
                finally
                {
                    conn.Close();
                }
            }
            return RedirectToAction("ResignBatal", "Notif", new { area = "Mobile" });
        }
        #endregion

        #endregion

        #region Pengingat Atasan
        public ActionResult RememberAtasan()
        {
            var Atasan = (from Kar in DB.TM_Karyawans
                          where Kar.Nama_Karyawan == User.Identity.Name
                          select Kar.Nama_Atasan).ToString().Trim();

            var DataRiwayat = (from Riwayat in DB.TM_Riwayats
                               where Riwayat.Status_Cuti == "Tunggu" && Riwayat.Pemberi == Atasan
                               select Riwayat).ToArray();

            return RedirectToAction("Riwayat", "Manager");
        }
        #endregion

        /*
         *  Bagian Data List
         *  Khusus untuk fungsi-fungsi view yang melibatkan data grid view
         */

        /* BEGIN DATA LIST */

        #region Data Karyawan
        public ActionResult Karyawan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    record = Request.Cookies["LoginID"];
                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(record.Values["NIK"]))
                        {
                            NIK = record.Values["NIK"].ToString().Trim();
                        }
                        if (!string.IsNullOrEmpty(record.Values["Departemen"]))
                        {
                            Departemen = record.Values["Departemen"].ToString().Trim();
                        }
                    }

                    ViewData["NIK"] = NIK;
                    ViewData["Departemen"] = Departemen;

                    return View("MobileKaryawan");
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult KaryawanPartial()
        {
            ModelState.Clear();
            // sembunyikan user admin dari daftar edit
            var model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.NIK != "00000" && Karyawan.NIK != "99999"
                         select Karyawan);

            record = Request.Cookies["LoginID"];
            if (record != null)
            {
                if (!string.IsNullOrEmpty(record.Values["NIK"]))
                {
                    NIK = record.Values["NIK"].ToString().Trim();
                }
                if (!string.IsNullOrEmpty(record.Values["Departemen"]))
                {
                    Departemen = record.Values["Departemen"].ToString().Trim();
                }
            }

            ViewData["NIK"] = NIK;
            ViewData["Departemen"] = Departemen;

            return PartialView("_MobileKaryawan", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateKaryawan(TM_Karyawan items)
        {
            var model = DB.TM_Karyawans;
            try
            {
                var modelitem = model.FirstOrDefault(it => it.NIK == items.NIK);
                var modelname = model.FirstOrDefault(it => it.Nama_Karyawan == items.Nama_Karyawan);
                if (modelitem != null || modelname != null)
                {
                    this.UpdateModel(modelitem);
                    DB.SaveChanges();
                    ViewData["EditSuccess"] = "Data karyawan '" + items.Nama_Karyawan + "' sudah di-update";
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
            }
            return PartialView("_Karyawan", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HapusKaryawan(string NIK)
        {
            var model = DB.TM_Karyawans;
            if (!string.IsNullOrEmpty(NIK))
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.NIK == NIK);
                    if (item != null)
                    {
                        // cek apakah NIK tsb adalah admin
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
                                    Privilege = sdr["Privilege"].ToString().Trim();
                                }
                            }
                            catch (SqlException ex)
                            {
                                ViewData["ErrorMsg"] = "Terjadi gangguan saat membaca data dari database. Pesan: \"" + ex.Message + "\"";
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
                            }

                            if ((Privilege ?? string.Empty) == "Admin")
                            {
                                // cek apakah NIK tsb merupakan satu-satunya admin
                                cmd = new SqlCommand("SM_Karyawan", conn);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Privilege", SqlDbType.NVarChar).Value = Privilege;
                                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Privilege";

                                try
                                {
                                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                                    sdr = cmd.ExecuteReader();
                                    while (sdr.Read())
                                    {
                                        Parameter = sdr["Jumlah"].ToString().Trim();
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    ViewData["ErrorMsg"] = "Terjadi gangguan saat membaca data dari database. Pesan: \"" + ex.Message + "\"";
                                }
                                finally
                                {
                                    sdr.Close();
                                    conn.Close();
                                }

                                if (Parameter != null && Convert.ToInt32(Parameter) == 1)
                                {
                                    ViewData["EditError"] = "Anda tidak dapat menghapus karyawan yang menjadi satu-satunya admin aktif dalam sistem.";
                                }
                                else
                                {
                                    model.Remove(item);
                                    DB.SaveChanges();
                                    ViewData["EditSuccess"] = "Data karyawan terpilih sudah dihapus";
                                }
                            }
                            else
                            {
                                model.Remove(item);
                                DB.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
                }
            }
            return PartialView("_Karyawan", model.ToList());
        }
        #endregion

        #region Data Perusahaan
        public ActionResult Perusahaan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult PerusahaanPartial()
        {
            ModelState.Clear();
            var model = DB.TM_Perusahaans;
            return PartialView("_Perusahaan", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdatePerusahaan(TM_Perusahaan items)
        {
            var model = DB.TM_Perusahaans;
            try
            {
                var modelitem = model.FirstOrDefault(it => it.Kode_Perusahaan == items.Kode_Perusahaan);
                var modelname = model.FirstOrDefault(it => it.Nama_Perusahaan == items.Nama_Perusahaan);
                if (modelitem != null)
                {
                    this.UpdateModel(modelitem);
                    DB.SaveChanges();
                    ViewData["EditSuccess"] = "Data perusahaan '" + items.Nama_Perusahaan + "' sudah di-update";
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
            }
            return PartialView("_Perusahaan", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HapusPerusahaan(string Kode_Perusahaan)
        {
            var model = DB.TM_Perusahaans;
            if (Kode_Perusahaan != null)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.Kode_Perusahaan == Kode_Perusahaan);
                    if (item != null)
                    {
                        model.Remove(item);
                        DB.SaveChanges();
                        ViewData["EditSuccess"] = "Data perusahaan terpilih sudah dihapus";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
                }
            }
            return PartialView("_Perusahaan", model.ToList());
        }
        #endregion

        #region Data Cabang
        public ActionResult Cabang()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    var DataCabang = (from Cbg in DB.TM_Cabangs
                                      select Cbg).ToArray();

                    foreach (TM_Cabang cb in DataCabang)
                    {
                        Nama_Perusahaan = cb.Nama_Perusahaan.ToString().Trim();
                        Nama_Cabang = cb.Nama_Cabang.ToString().Trim();

                        // ambil data kepala cabang
                        using (var conn = new SqlConnection(connstring))
                        {
                            cmd = new SqlCommand("SM_Cabang", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@Nama_Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                            cmd.Parameters.Add("@Nama_Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Kepala Cabang";

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
                                return View();
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View();
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }

                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult CabangPartial()
        {
            ModelState.Clear();
            var model = DB.TM_Cabangs;
            return PartialView("_CabangGrid", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateCabang(TM_Cabang items)
        {
            var model = DB.TM_Cabangs;
            try
            {
                var modelitem = model.FirstOrDefault(it => it.Kode_Cabang == items.Kode_Cabang);
                var modelname = model.FirstOrDefault(it => it.Nama_Cabang == items.Nama_Cabang);
                if (modelitem != null)
                {
                    this.UpdateModel(modelitem);
                    DB.SaveChanges();
                    ViewData["EditSuccess"] = "Data cabang '" + items.Nama_Cabang + "' sudah di-update";
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
            }
            return PartialView("_CabangGrid", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HapusCabang(string Kode_Cabang)
        {
            var model = DB.TM_Cabangs;
            if (Kode_Cabang != null)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.Kode_Cabang == Kode_Cabang);
                    if (item != null)
                    {
                        model.Remove(item);
                        DB.SaveChanges();
                        ViewData["EditSuccess"] = "Data cabang sudah dihapus";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
                }
            }
            return PartialView("_CabangGrid", model.ToList());
        }
        #endregion

        #region Data Riwayat Cuti
        public ActionResult Riwayat(CutiModel model, String id)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();

                if (ViewBag.Privilege == "Manager")
                {
                    // filtering berlaku sampai tanggal saat ini
                    model.Tgl_Mulai = DateTime.Now;
                    model.Tgl_Selesai = DateTime.Now;
                    Session["FilterTanggal"] = "";

                    if (!string.IsNullOrEmpty(id))
                    {
                        Session["Filter"] = "[NIK] = '" + id + "'";
                    }
                    else
                    {
                        Session["Filter"] = null;
                    }

                    // update 160125:
                    // sebelum kembali ke view, periksa apakah ybs memiliki pengajuan yang belum disetujui
                    var WaitingList = (from Riwayat in DB.TM_Riwayats
                                       where (Riwayat.Atasan == User.Identity.Name && Riwayat.Status_Cuti == "Tunggu" && Riwayat.Nama_Karyawan != User.Identity.Name)
                                       select Riwayat).ToArray();

                    if (WaitingList.Length > 0)
                    {
                        ViewData["Init"] = "Anda masih mempunyai " + WaitingList.Length + " pengajuan cuti yang menunggu persetujuan anda.";
                    }

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult RiwayatPartial()
        {
            ModelState.Clear();

            var model = (from Riwayat in DB.TM_Riwayats
                         where (Riwayat.Nama_Karyawan == User.Identity.Name || Riwayat.Atasan == User.Identity.Name)
                         select Riwayat);
            return PartialView("_Riwayat", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateRiwayat(TM_Riwayat items)
        {
            var model = DB.TM_Riwayats;
            try
            {
                var modelitem = model.FirstOrDefault(it => it.ID_Cuti == items.ID_Cuti);
                var modelname = model.FirstOrDefault(it => it.NIK == items.NIK);
                if (modelitem != null && modelname != null)
                {
                    // hitung masa cuti secara otomatis, tidak peduli dengan masa cuti yang disediakan user
                    Tgl_Awal = (DateTime)modelitem.Tgl_Mulai;
                    Tgl_Akhir = (DateTime)modelitem.Tgl_Selesai;

                    // untuk jenis cuti tertentu, tanggal selesai ditimpa dengan hasil perhitungan sesuai masa cuti yang ditentukan
                    switch (modelitem.Jenis_Cuti)
                    {
                        case "Melahirkan":
                            Masa_Cuti = 90;
                            Tgl_Akhir = Tgl_Awal.AddDays(Masa_Cuti);
                            modelitem.Tgl_Selesai = Tgl_Akhir;
                            break;
                        case "Pernikahan Ybs.":
                            Masa_Cuti = 3;
                            Tgl_Akhir = Tgl_Awal.AddDays(Masa_Cuti);
                            if (Tgl_Akhir.DayOfWeek == DayOfWeek.Sunday)
                                Tgl_Akhir.AddDays(1);
                            modelitem.Tgl_Selesai = Tgl_Akhir;
                            break;
                        default:
                            Masa_Cuti = (int)(Tgl_Akhir - Tgl_Awal).TotalDays;
                            break;
                    }

                    modelitem.Masa_Cuti = Masa_Cuti;
                    this.UpdateModel(modelitem);
                    DB.SaveChanges();
                    ViewData["EditSuccess"] = "Data riwayat cuti nomor '" + items.ID_Cuti + "' sudah di-update";
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
            }
            return PartialView("_Riwayat", model.ToList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HapusRiwayat(string ID_Cuti)
        {
            var model = DB.TM_Riwayats;
            if (ID_Cuti != null)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.ID_Cuti == ID_Cuti);
                    if (item != null)
                    {
                        model.Remove(item);
                        DB.SaveChanges();
                        ViewData["EditSuccess"] = "Data riwayat cuti terpilih sudah dihapus";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
                }
            }
            return PartialView("_Riwayat", model.ToList());
        }
        #endregion

        #region Data Sisa Cuti Tahunan
        public ActionResult SisaTahunan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    var model = new SearchModel();
                    model.Tgl_Akhir = DateTime.Now.Date;

                    // tahap 1: kalkulasi sisa cuti menurut tahun masuk s/d tahun terkini

                    record = Request.Cookies["LoginID"];
                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(record.Values["NIK"]))
                        {
                            NIK = record.Values["NIK"].ToString();
                        }

                        Session["NIK"] = NIK;
                        ViewData["NIK"] = NIK;

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
                                    Nama_Perusahaan = sdr["Perusahaan"].ToString().Trim();
                                    Nama_Cabang = sdr["Cabang"].ToString().Trim();
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

                                // hati-hati: untuk nama "Sri Maryati" cek apakah privilegenya staff atau supervisor
                                if (Nama_Karyawan == "Sri Maryati" && Privilege == "Staff")
                                {
                                    NIK = "00100";
                                }
                                else if (Nama_Karyawan == "Sri Maryati" && Privilege == "Supervisor")
                                {
                                    NIK = "00095";
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

                            // tahap 2: set tanggal awal & tanggal akhir
                            // tanggal awal = tanggal masuk kerja pertama kali
                            // tanggal akhir = tanggal awal + jumlah tahun kerja + 1 tahun
                            // jika karyawan resign sebelum tanggal akhir masa cuti berjalan, tanggal akhir diatur nilainya dengan tanggal resign
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
                            cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                            cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
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
                        }
                        return View(model);
                    }
                    else
                    {
                        SetPrivilege();
                        return View(model);
                    }
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult SisaTahunanPartial()
        {
            ModelState.Clear();
            var model = DB.TM_Cutis;
            return PartialView("_SisaTahunan", model.ToList());
        }

        // bagian isian untuk NIK tertentu
        [HttpPost]
        public ActionResult SisaTahunan(SearchModel model)
        {
            if (!string.IsNullOrEmpty(model.NIK))
            {
                NIK = model.NIK.ToString().Trim();
                Session["NIK"] = NIK;

                // susun stored procedure
                // mencari data karyawan yang sedang aktif
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
                            Nama_Perusahaan = sdr["Perusahaan"].ToString().Trim();
                            Nama_Cabang = sdr["Cabang"].ToString().Trim();
                            Jabatan = sdr["Jabatan"].ToString().Trim();
                            Departemen = sdr["Departemen"].ToString().Trim();
                            Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"]);
                            Privilege = sdr["Privilege"].ToString().Trim();
                            if (!string.IsNullOrEmpty(sdr["Tgl_Resign"].ToString()))
                            {
                                Tgl_Resign = Convert.ToDateTime(sdr["Tgl_Resign"]);
                            }
                        }

                        // hati-hati: untuk nama "Sri Maryati" cek apakah privilegenya staff atau supervisor
                        if (Nama_Karyawan == "Sri Maryati" && Privilege == "Staff")
                        {
                            NIK = "00100";
                        }
                        else if (Nama_Karyawan == "Sri Maryati" && Privilege == "Supervisor")
                        {
                            NIK = "00095";
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

                    // tahap 2: set tanggal awal & tanggal akhir
                    // tanggal awal = tanggal masuk kerja pertama kali
                    // tanggal akhir = tanggal awal + jumlah tahun kerja + 1 tahun
                    // jika karyawan resign sebelum tanggal akhir masa cuti berjalan, tanggal akhir diatur nilainya dengan tanggal resign
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
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Nama_Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Nama_Cabang;
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
                }
                SetPrivilege();
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        #endregion

        #region Data Sisa Cuti Tahunan (Semua Bawahan)
        public ActionResult SisaBawahan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    // tahap 1: susun array untuk semua karyawan bawahan yang aktif
                    var DataKaryawan = (from Karyawan in DB.TM_Karyawans
                                        where (Karyawan.Nama_Atasan == User.Identity.Name || Karyawan.Nama_Karyawan == User.Identity.Name || Karyawan.Nama_Advisor == User.Identity.Name) && Karyawan.Status_Kerja == "Aktif"
                                        select Karyawan).ToArray();

                    foreach (TM_Karyawan Kar in DataKaryawan)
                    {
                        NIK = Kar.NIK;
                        Nama_Karyawan = Kar.Nama_Karyawan;
                        Nama_Atasan = Kar.Nama_Atasan;
                        if (!string.IsNullOrEmpty(Kar.Nama_Advisor))
                        {
                            Nama_Advisor = Kar.Nama_Advisor;
                        }
                        Tgl_Masuk = Convert.ToDateTime(Kar.Tgl_Masuk);
                        Tgl_Akhir = DateTime.Now.Date;

                        // susun stored procedure
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
                                    Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                    Tgl_Masuk = Convert.ToDateTime(sdr["Tgl_Masuk"]);
                                }
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View(ViewData);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View(ViewData);
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
                            }

                            // tahap 2: set tanggal awal & tanggal akhir
                            // tanggal awal = tanggal masuk kerja pertama kali
                            // tanggal akhir = tanggal awal + jumlah tahun kerja + 1 tahun
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

                            Selisih_Awal = (Tgl_Akhir - Tgl_Awal).TotalDays;

                            // susun stored procedure
                            // masukkan parameter sesuai dengan prosedur sisa cuti

                            cmd = new SqlCommand("SP_Hitung_Cuti_Tahunan", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                            cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                            cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                            if (!string.IsNullOrEmpty(Nama_Advisor))
                            {
                                cmd.Parameters.Add("@Nama_Advisor", SqlDbType.NVarChar).Value = Nama_Advisor;
                            }
                            cmd.Parameters.Add("@Tgl_Masuk", SqlDbType.DateTime).Value = Tgl_Masuk;
                            cmd.Parameters.Add("@Tgl_Akhir", SqlDbType.DateTime).Value = Tgl_Akhir;
                            cmd.Parameters.Add("@Temp1", SqlDbType.NVarChar).Value = Selisih_Awal.ToString(); // ambil dari combo box model
                            cmd.Parameters.Add("@Temp5", SqlDbType.Int).Value = Tgl_Akhir.Year - Tgl_Masuk.Year;

                            // eksekusi parameter query di balik layar
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
                                return View(ViewData);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View(ViewData);
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }

                    // tentukan apakah masa cuti sudah melewati tanggal cuti massal tahun berjalan
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SM_List_Cuti", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Tgl_Cuti", SqlDbType.DateTime).Value = DateTime.Parse(DateTime.Now.Year.ToString() + "-12-31");
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
                            return View(ViewData);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View(ViewData);
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
                            ViewData["Catatan"] = "Sisa cuti pada tabel belum termasuk potongan cuti massal tahun " + DateTime.Now.Year.ToString() + " sebanyak " + Total_CM.ToString() + " hari.";
                        }
                        else
                        {
                            ViewData["Catatan"] = "Sisa cuti pada tabel sudah termasuk potongan cuti massal tahun " + DateTime.Now.Year.ToString() + " sebanyak " + Total_CM.ToString() + " hari.";
                        }
                    }

                    return View(ViewData);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult SisaBawahanPartial()
        {
            var model = (from Karyawan in DB.TM_Cuti_Bawahans
                         where Karyawan.Nama_Atasan == User.Identity.Name || Karyawan.Nama_Karyawan == User.Identity.Name || Karyawan.Nama_Advisor == User.Identity.Name
                         select Karyawan).ToList();
            return PartialView("_SisaBawahan", model);
        }
        #endregion

        #region Cuti Massal
        public ActionResult CutiMassal()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                return View();
            }
        }

        public ActionResult CutiMassalPartial()
        {
            var model = DB.TM_List_Cutis;
            return PartialView("_CutiMassal", model.ToList());
        }
        #endregion

        #region Data Riwayat Mutasi
        public ActionResult DataMutasi()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { area = "Mobile" });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Manager")
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult DataMutasiPartial()
        {
            var model = DB.TM_Mutasis;
            return PartialView("_DataMutasi", model.ToList());
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
                if (ViewBag.Privilege == "Manager")
                {
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

                        var DataRiwayat = (from Riwayat in DB.TM_Riwayats
                                           where (Riwayat.Status_Cuti == "Disetujui" && (Riwayat.Nama_Karyawan == User.Identity.Name || Riwayat.Pemberi == User.Identity.Name || Riwayat.Atasan == User.Identity.Name))
                                           select Riwayat).ToArray();
                        var DataMassal = (from Massal in DB.TM_List_Cutis select Massal).ToArray();
                        var DataLibur = (from Libur in DB.TM_List_Liburs select Libur).ToArray();

                        // masukkan riwayat cuti terlebih dahulu
                        foreach (TM_Riwayat Riwayats in DataRiwayat)
                        {
                            Nama_Event = Riwayats.Nama_Karyawan;
                            Jenis_Event = Riwayats.Jenis_Cuti;
                            Tgl_Awal = (DateTime)Riwayats.Tgl_Mulai;
                            Tgl_Akhir = (DateTime)Riwayats.Tgl_Selesai;

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

                        // masukkan cuti massal
                        foreach (TM_List_Cuti CTMassal in DataMassal)
                        {
                            Nama_Event = CTMassal.Keterangan;
                            Jenis_Event = "Cuti Massal";
                            Tgl_Awal = (DateTime)CTMassal.Tgl_Cuti;
                            Tgl_Akhir = Tgl_Awal;

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

                        // masukkan hari libur
                        foreach (TM_List_Libur Liburan in DataLibur)
                        {
                            Nama_Event = Liburan.Keterangan;
                            Jenis_Event = "Libur Nasional";
                            Tgl_Awal = (DateTime)Liburan.Tgl_Libur;
                            Tgl_Akhir = Tgl_Awal;

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
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult KalenderPartial()
        {
            var model = DB.TM_Kalenders;
            return PartialView("_Kalender", model.ToList());
        }
        #endregion

        /* END DATA LIST */

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

        public bool HasValues(string source, string word)
        {
            return source.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion
	}
}