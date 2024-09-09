using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
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
namespace AristaHRM.Controllers
{
    // apabila koneksi diatur untuk menerima SSL/TLS,
    // hilangkan tanda komentar pada baris di bawah ini:

    // [RequireHttps]
    [Authorize]
    [RoutePrefix("Admin")]
    public class AdminController : Controller
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
        public string Perusahaan, Cabang, Jabatan, Departemen, NIK_Atasan, Nama_Atasan, NIK_Supervisor, Nama_Supervisor;
        public string Pembuat, Privilege, Privilege_S, Privilege_At;
        public string Notes, NotesBaru;

        // model khusus cuti
        public string ID_Cuti;
        public string Jenis_Cuti;
        public string Pemberi, Keperluan;
        public string Status_Cuti, Keterangan, No_Kontak;
        public string Nomor_Kontak;
        public string ApprovedBy, RejectedBy;

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
        public string GenderPrefix;
        public string ServerIP = Providers.GetServerIP();

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

        #region Pengaturan Karyawan

        #region Profil Karyawan

        #region Profil Dasar
        public ActionResult Profil(KaryawanModel model, string id)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
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

                    // periksa apakah user tersebut memiliki foto pribadi
                    var ImageID = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                   where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                   select Path.GetFileName(img)
                                  ).ToArray();

                    if (ImageID != null && ImageID.Length > 0)
                    {
                        foreach (string image in ImageID)
                        {
                            PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                        }
                    }
                    else
                    {
                        PhotoDir = "~/Images/Anon.png";
                    }

                    Session["ImagePath"] = PhotoDir;

                    return View(model);
                }
            }
        }

        // jika admin mengirim permintaan tampilan profil dengan NIK tertentu
        [HttpPost]
        public ActionResult Profil(KaryawanModel model, string id, string Length)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();

                // kosongkan notes
                Notes = null;
                NotesBaru = null;

                NIK = model.NIK.ToString().Trim();
                id = NIK;

                foreach (string file in Request.Files)
                {
                    UploadCtrl = this.Request.Files[file];
                }

                if (Length != null && !string.IsNullOrEmpty(UploadCtrl.FileName))
                {
                    // unggah gambar
                    try
                    {
                        if (UploadCtrl != null && !string.IsNullOrEmpty(UploadCtrl.FileName))
                        {
                            // file tersedia dan siap untuk diterima
                            string Ekstensi = Path.GetExtension(UploadCtrl.FileName);

                            // batasi ekstensi berkas
                            if (Ekstensi != ".jpg" && Ekstensi != ".jpeg" && Ekstensi != ".png" && Ekstensi != ".gif")
                            {
                                ViewData["ErrorMsg"] = "File yang diunggah harus berekstensi .jpg, .jpeg, .gif atau .png";

                                // periksa apakah user tersebut memiliki foto pribadi
                                var ImageID = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                               where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                               select Path.GetFileName(img)
                                              ).ToArray();

                                if (ImageID != null && ImageID.Length > 0)
                                {
                                    foreach (string image in ImageID)
                                    {
                                        PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                                    }
                                }
                                else
                                {
                                    PhotoDir = "~/Images/Anon.png";
                                }

                                Session["ImagePath"] = PhotoDir;

                                SetPrivilege();
                            }
                            // akhir blok error berkas

                            string PathFile = string.Format("{0}\\{1}", Server.MapPath("~/Files/Members"), NIK + Ekstensi);
                            if (System.IO.File.Exists(PathFile))
                            {
                                System.IO.File.Delete(PathFile);
                            }

                            // rename sesuai NIK karyawan bersangkutan pada model
                            UploadCtrl.SaveAs(PathFile);

                            // periksa apakah user tersebut memiliki foto pribadi
                            var ImageFind = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                             where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                             select Path.GetFileName(img)
                                          ).ToArray();

                            if (ImageFind != null && ImageFind.Length > 0)
                            {
                                foreach (string image in ImageFind)
                                {
                                    PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                                }
                            }
                            else
                            {
                                PhotoDir = "~/Images/Anon.png";
                            }

                            Session["ImagePath"] = PhotoDir;

                            // reset parameter Length supaya tidak mengganggu request lainnya
                            Length = null;

                        }
                        else
                        {
                            ViewData["ErrorMsg"] = "Anda belum memilih berkas untuk foto profil anda. Silakan lakukan kembali proses pengunggahan berkas.";

                            // periksa apakah user tersebut memiliki foto pribadi
                            var ImageFind2 = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                              where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                              select Path.GetFileName(img)
                                          ).ToArray();

                            if (ImageFind2 != null && ImageFind2.Length > 0)
                            {
                                foreach (string image in ImageFind2)
                                {
                                    PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                                }
                            }
                            else
                            {
                                PhotoDir = "~/Images/Anon.png";
                            }

                            Session["ImagePath"] = PhotoDir;
                        }
                    }
                    catch (IOException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi gangguan dalam proses pengunggahan berkas. Pesan kesalahan: " + ex.Message;

                        // periksa apakah user tersebut memiliki foto pribadi
                        var ImageFindB = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                          where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                          select Path.GetFileName(img)
                                      ).ToArray();

                        if (ImageFindB != null && ImageFindB.Length > 0)
                        {
                            foreach (string image in ImageFindB)
                            {
                                PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                            }
                        }
                        else
                        {
                            PhotoDir = "~/Images/Anon.png";
                        }

                        Session["ImagePath"] = PhotoDir;

                        SetPrivilege();
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi gangguan tak dikenal dalam proses perolehan berkas. Pesan kesalahan: " + e.Message;

                        // periksa apakah user tersebut memiliki foto pribadi
                        var ImageFindA = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                          where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                          select Path.GetFileName(img)
                                      ).ToArray();

                        if (ImageFindA != null && ImageFindA.Length > 0)
                        {
                            foreach (string image in ImageFindA)
                            {
                                PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                            }
                        }
                        else
                        {
                            PhotoDir = "~/Images/Anon.png";
                        }

                        Session["ImagePath"] = PhotoDir;

                        SetPrivilege();
                    }

                    // periksa apakah user tersebut memiliki foto pribadi
                    var ImageData = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                     where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                     select Path.GetFileName(img)
                                    ).ToArray();

                    if (ImageData != null && ImageData.Length > 0)
                    {
                        foreach (string image in ImageData)
                        {
                            PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                        }
                    }
                    else
                    {
                        PhotoDir = "~/Images/Anon.png";
                    }

                    Session["ImagePath"] = PhotoDir;
                }

                // tampilkan profil user

                if (!string.IsNullOrEmpty(model.NoteBaru))
                    NotesBaru = model.NoteBaru.ToString().Trim();

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

                    // periksa apakah user tersebut memiliki foto pribadi
                    var ImageRet = (from img in Directory.GetFiles(Server.MapPath("~/Files/Members"))
                                    where ((Path.GetExtension(img) == ".jpg" || Path.GetExtension(img) == ".png" || Path.GetExtension(img) == ".jpeg" || Path.GetExtension(img) == ".gif") && Path.GetFileNameWithoutExtension(img) == NIK)
                                    select Path.GetFileName(img)
                                   ).ToArray();

                    if (ImageRet != null && ImageRet.Length > 0)
                    {
                        foreach (string image in ImageRet)
                        {
                            PhotoDir = string.Format("{0}\\{1}", "~/Files/Members", image);
                        }
                    }
                    else
                    {
                        PhotoDir = "~/Images/Anon.png";
                    }

                    Session["ImagePath"] = PhotoDir;

                    // periksa apakah isian form melibatkan perubahan notes
                    if (!string.IsNullOrEmpty(NotesBaru))
                    {
                        // update model jika terjadi perubahan notes

                        cmd = new SqlCommand("SM_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                        cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = NotesBaru;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Notes";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();

                            Notes = NotesBaru;
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

                    model.Notes = Notes;
                    model.NoteBaru = null;

                    // kembali ke halaman profil
                    SetPrivilege();
                    return View(model);
                }
            }
        }
        #endregion

        #endregion

        #region Ubah Password
        public ActionResult UbahPassword()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                // membaca NIK dari cookie bila ada
                if (ViewBag.Privilege == "Admin")
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
                            return View(model);
                        }
                        else
                        {
                            // melalui sesi
                            model.NIK = Session["NIK"].ToString().Trim();
                            return View(model);
                        }
                    }
                    else
                    {
                        // melalui sesi
                        model.NIK = Session["NIK"].ToString().Trim();
                        return View(model);
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
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    if (!string.IsNullOrEmpty(Pass_Lama) && Pass_Lama != Password)
                    {
                        ViewData["ErrorMsg"] = "Password yang digunakan untuk mengatur akun anda saat ini salah atau keliru. Silakan periksa kembali form isian.";
                        SetPrivilege();
                        return View(model);
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
                            return RedirectToAction("EditSukses", "Notif");
                        }
                        catch (SqlException ex)
                        {
                            // jika terdapat gangguan koneksi database
                            ViewData["ErrorMsg"] = "Terjadi masalah saat memproses data ke database. Pesan: \"" + ex.Message + "\"";
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
                    else
                    {
                        // tahap 4-B: jika password saling tidak cocok
                        ViewData["ErrorMsg"] = "Password dan konfirmasinya tidak sesuai. Silakan periksa kembali form isian dan ulangi proses setting.";
                        model.NIK = NIK;
                        return View(model);
                    }
                }
            }
            else
            {
                SetPrivilege();
                ViewData["ErrorMsg"] = "Anda belum mengisi password dan/atau konfirmasinya. Periksa kembali form isian dan ulangi proses berjalan.";
                return View(model);
            }
        }
        #endregion

        #region Reset Password
        public ActionResult ResetPassword()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
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
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
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
                            Msg.Append("<a href=\"" + string.Format("http://" + ServerIP + "/Home/EditAkun/{0}/{1}", NIK, Reset_Token) + "\">" + string.Format("http://" + ServerIP + "/Home/EditAkun/{0}/{1}", NIK, Reset_Token) + "</a>"); // => ubah localhost dengan alamat situs remote SMTP server
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
                            return RedirectToAction("ResetSukses", "Notif");
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
                            conn.Close();
                        }
                    }
                    else
                    {
                        ViewData["ErrorMsg"] = "NIK yang anda masukkan tidak terdaftar pada sistem. Silakan hubungi admin untuk mengetahui NIK anda.";
                        SetPrivilege();
                        return View(model);
                    }
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
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin")
                {
                    string Status_Nikah_T = (Request.Params["Status_Nikah"] ?? string.Empty).ToString();
                    string Agama_T = (Request.Params["Agama"] ?? string.Empty).ToString();
                    Tgl_Setuju = DateTime.Now;
                    model.Jenis_Cuti = "1";
                    model.Tgl_Pengajuan = Tgl_Setuju;
                    model.Tgl_Mulai = Tgl_Setuju;
                    model.Tgl_Selesai = Tgl_Setuju;

                    // default tampilan = user ybs
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

        #region Partial View
        public ActionResult JenisCutiPartial()
        {
            string nik = (Request.Params["NIK"] ?? string.Empty).ToString();

            using (var DB = new HRISContext())
            {
                DateTime TglMasuk = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == nik).Tgl_Masuk.Value;

                return PartialView("_JenisCuti", new CutiModel { Tgl_Masuk = TglMasuk });
            }
        }

        public ActionResult PengajuanPartial()
        {
            string Status_Nikah_T = (Request.Params["Status_Nikah"] ?? string.Empty).ToString();
            string Agama_T = (Request.Params["Agama"] ?? string.Empty).ToString();
            string Jenis_T = (Request.Params["Jenis_Kelamin"] ?? String.Empty).ToString();
            string NIK = (Request.Params["NIK"] ?? String.Empty).ToString();
            string Jenis = (Request.Params["Jenis_Cuti"] ?? String.Empty).ToString();
            return PartialView("_Pengajuan", new CutiModel { Status_Nikah = Status_Nikah_T, Agama = Agama_T, Jenis_Kelamin = Jenis_T, NIK = NIK, Jenis_Cuti = Jenis });
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pengajuan(CutiModel model, string url)
        {
            // mengingat model ini tidak dapat menggunakan prosedur IsValid,
            // pengecekan diubah menjadi pengujian model read-only & model yang dapat berubah
            using (var conn = new SqlConnection(connstring))
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                if (string.IsNullOrEmpty(model.Keperluan) && model.Jenis_Cuti == "1")
                {
                    model.Keperluan = "Cuti Tahunan";
                }

                if (model.Perusahaan == null || model.Cabang == null || model.Jabatan == null || model.Departemen == null)
                {
                    ViewData["ErrorMsg"] = "Mohon melengkapi entri karyawan ybs terlebih dahulu sebelum melakukan pengajuan cuti.";
                    model.NIK = NIK;
                    SetPrivilege();
                    return View(model);
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
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View(model);
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
                            return View(model);
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
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // cek apakah sudah ada yang mengajukan cuti di database
                    // ID_Cuti == string.Empty || ID_Cuti == null
                    if (string.IsNullOrEmpty(ID_Cuti))
                    {
                        ID_Baru = 0; // nilai standar
                    }
                    else
                    {
                        // cek tanggal terakhir pengajuan, jika berbeda gunakan indeks 01 kembali

                        // 8 digit bagian depan = tanggal pengajuan
                        DateTime TempDate = DateTime.Parse(ID_Cuti.Substring(0, 4) + "/" + ID_Cuti.Substring(4, 2) + "/" + ID_Cuti.Substring(6, 2));
                        if (TempDate.Date < DateTime.Now.Date || TempDate.Date > DateTime.Now.Date)
                        {
                            ID_Baru = 0;
                        }
                        else
                        {
                            // 4 digit bagian belakang = nomor urut pendaftaran
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
                        conn.Close();
                    }

                    // default masa cuti dihitung dengan ketentuan di atas

                    /* END PROC COUNT CUTI */

                    Masa_Cuti = Convert.ToInt32(cmd.Parameters["@Jumlah_Hari"].Value.ToString());

                    Keperluan = model.Keperluan ?? "Cuti Tahunan";

                    // int t = 0;

                    // update 200123: kamus ini sudah tidak dipergunakan lagi

                    // update 160225: filter berdasarkan kata per kata yang diuji
                    // jika keperluan yang diisi user termasuk dalam filter ini, secara otomatis sistem akan memilih berdasarkan data yang ada

                    // update 160105: menentukan jenis cuti secara otomatis berdasarkan keperluannya
                    // pencarian tidak case-sensitive

                    // NB: jenis cuti lainnya tidak termasuk di sini karena memiliki lebih dari satu kemungkinan deret string
                    // untuk kasus dua atau lebih string yang serupa, yang pertama dicari adalah yang paling spesifik terlebih dahulu

                    // contoh: untuk "saudara kandung meninggal" yang hampir serupa dengan "saudara kandung menikah", yang dicari terlebih dahulu adalah "meninggal" (cuti khusus), kemudian "saudara" (cuti tahunan) & terakhir "menikah" (cuti khusus).
                    //cmd = new SqlCommand("SP_Pengajuan", conn);
                    //cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Keperluan";

                    //try
                    //{
                    //    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    //    sdr = cmd.ExecuteReader();
                    //    using (dt = new DataTable())
                    //    {
                    //        dt.Load(sdr);
                    //        ArrayData = new String[dt.Rows.Count];
                    //        ResultData = new String[dt.Rows.Count];
                    //        for (t = 0; t < dt.Rows.Count; t++)
                    //        {
                    //            ArrayData[t] = dt.Rows[t][1].ToString().Trim(); // Array data = kumpulan kata kunci preset dalam pengambilan keputusan
                    //            ResultData[t] = dt.Rows[t][2].ToString().Trim(); // Result data = hasil keputusan yang diharapkan dari kata kunci sesuai indeks yang dimiliki
                    //        }
                    //    }

                    //    // pemeriksaan keperluan cuti
                    //    for (int n = 0; n < (ArrayData.Length); n++)
                    //    {
                    //        if (HasValues(Keperluan, ArrayData[n]) && !HasValues(Keperluan, "Meninggal"))
                    //        {
                    //            // tentukan jenis cuti berdasarkan nilai data yang sesuai & keluar dari loop
                    //            Jenis_Cuti = ResultData[n].ToString().Trim();
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            Jenis_Cuti = model.Jenis_Cuti.ToString().Trim();
                    //            // ingat, jangan lakukan "break" di sini karena ada pemeriksaan terhadap kata berikutnya dalam daftar tabel sampai loop selesai
                    //            // pastikan jenis cuti diisi nilai default supaya jika tidak ada satu pun kondisi yang memenuhi kata kunci
                    //            // secara otomatis sistem mengambil nilai dari kotak isian
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    ViewBag.Message = ex.Message;

                    //    // jika menemui masalah, gunakan perintah ini
                    //    // jangan keluarkan pesan error karena adanya exception memindahkan kendali penyesuaian jenis cuti ke mode manual

                    //    //if (HasValues(Keperluan, "Meninggal"))
                    //    //{
                    //    //    Keperluan = model.Keperluan.ToString().Trim();
                    //    //}
                    //    // kriteria cuti tahunan: jika ada kata-kata berikut ini, sistem otomatis memilih "cuti tahunan" meskipun terdapat kata lain di sekitarnya.
                    //    //else if (
                    //    //    HasValues(Keperluan, "Pribadi") || HasValues(Keperluan, "Keluarga")
                    //    // || HasValues(Keperluan, "Libur") || HasValues(Keperluan, "Pulang")
                    //    // || HasValues(Keperluan, "Mudik") || HasValues(Keperluan, "Kampung")
                    //    // || HasValues(Keperluan, "Acara") || HasValues(Keperluan, "Rumah")
                    //    // || HasValues(Keperluan, "Urus") || HasValues(Keperluan, "KTP")
                    //    // || HasValues(Keperluan, "BPJS") || HasValues(Keperluan, "KPR")
                    //    // || HasValues(Keperluan, "General") || HasValues(Keperluan, "Pindah")
                    //    // || HasValues(Keperluan, "Medi") || HasValues(Keperluan, "Sakit")
                    //    // || HasValues(Keperluan, "Operasi") || HasValues(Keperluan, "Istirahat")
                    //    // || HasValues(Keperluan, "Aqiqah") || HasValues(Keperluan, "Tambahan")
                    //    // || HasValues(Keperluan, "Saudara") || HasValues(Keperluan, "Ipar")
                    //    // || HasValues(Keperluan, "Keponakan") || HasValues(Keperluan, "Kerabat")
                    //    // || HasValues(Keperluan, "SIM") || HasValues(Keperluan, "Sepupu")
                    //    // || HasValues(Keperluan, "Opname") || HasValues(Keperluan, "Izin")
                    //    // || HasValues(Keperluan, "Kandung") || HasValues(Keperluan, "Jenguk")
                    //    // || HasValues(Keperluan, "Wawancara") || HasValues(Keperluan, "Surat")
                    //    // || HasValues(Keperluan, "Siap")
                    //    //)
                    //    //{
                    //    //    Keperluan = "Cuti Tahunan";
                    //    //}
                    //    //else if (Keperluan.IndexOf("Istri Melahirkan", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    //{
                    //    //    Keperluan = "Istri Melahirkan";
                    //    //}
                    //    //else if (Keperluan.IndexOf("Melahirkan", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    //{
                    //    //    Keperluan = "Melahirkan";
                    //    //}
                    //    //else if (
                    //    //   Keperluan.IndexOf("Menikah", StringComparison.OrdinalIgnoreCase) >= 0
                    //    //|| Keperluan.IndexOf("Pernikahan", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    //{
                    //    //    Keperluan = "Pernikahan Ybs.";
                    //    //}
                    //    //else if (Keperluan.IndexOf("Wisuda", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    //{
                    //    //    Keperluan = "Wisuda";
                    //    //}
                    //    //// bugfix 160128:
                    //    //// khusus haji dan umroh terdapat proses administrasi calon haji & izin masuk, jadi proses administrasi tersebut didahulukan untuk diuji
                    //    //else if (Keperluan.IndexOf("Administrasi", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    //{
                    //    //    Keperluan = "Cuti Tahunan";
                    //    //}
                    //    //else if (
                    //    //   Keperluan.IndexOf("Haji", StringComparison.OrdinalIgnoreCase) >= 0
                    //    //|| Keperluan.IndexOf("Berangkat Haji", StringComparison.OrdinalIgnoreCase) >= 0
                    //    //|| Keperluan.IndexOf("Naik Haji", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    //{
                    //    //    Keperluan = "Haji";
                    //    //}
                    //    //else if (Keperluan.IndexOf("Umroh", StringComparison.OrdinalIgnoreCase) >= 0 || Keperluan.IndexOf("Berangkat Umroh", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    //{
                    //    //    Keperluan = "Umroh";
                    //    //}
                    //    //else
                    //    //{
                    //    //    Keperluan = model.Keperluan.ToString().Trim();
                    //    //}
                    //}
                    //finally
                    //{
                    //    sdr.Close();
                    //    conn.Close();
                    //}

                    // konversi jenis cuti
                    switch (model.Jenis_Cuti)
                    {
                        case "1":
                            Jenis_Cuti = "Cuti Tahunan";
                            break;
                        case "2":
                            Jenis_Cuti = "Cuti Khusus";
                            break;
                        case "3":
                            Jenis_Cuti = "Cuti Advance";
                            break;
                    }

                    /* BEGIN PROC SPCL CUTI KHUSUS */
                    if (model.Jenis_Cuti == "2")
                    {
                        // susun parameter stored procedure cuti khusus
                        cmd = new SqlCommand("SM_Default", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Kode_Jenis", SqlDbType.NVarChar).Value = "Jenis_Cuti";
                        cmd.Parameters.Add("@Jenis_Isi", SqlDbType.NVarChar).Value = Keperluan;

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

                            if (Keperluan == "Wisuda")
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
                            if ((Jenis_Cuti == "2") && Keperluan != "Melahirkan" && Keperluan != "Haji" && Keperluan != "Umroh")
                            {
                                // hitung jumlah hari libur yang dilewati oleh range cuti
                                // untuk 2 atau lebih hari libur berdekatan, lakukan type cast & tambahkan jumlah hari cuti sesuai hari libur
                                // bugfix 151222 - Ifa Maulidia: 2 hari libur berdekatan sudah dapat dideteksi
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

                        // kedua jenis cuti ini tidak dibatasi tanggal, sederhananya kurangi tanggal akhir dengan tanggal awal
                        if ((Jenis_Cuti == "2") && (Keperluan == "Haji" || Keperluan == "Umroh"))
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
                        return View(model);
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
                            return View(model);
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        ViewData["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses pencarian data. Pesan: \"" + ex.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        ViewData["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        model.NIK = NIK;
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // pemeriksaan akhir untuk masa cuti = 90 hari
                    if (Masa_Cuti == 90 && Keperluan.Equals("Cuti Tahunan"))
                    {
                        Keperluan = "Melahirkan";
                    }

                    // tahap 5: susun stored procedure untuk mencatat data pengajuan cuti
                    NIK = model.NIK.ToString().Trim();
                    Nama_Karyawan = model.Nama_Karyawan.ToString().Trim();
                    Jenis_Kelamin = model.Jenis_Kelamin.ToString().Trim();
                    if (!string.IsNullOrEmpty(model.Agama))
                    {
                        Agama = model.Agama.ToString().Trim();
                    }
                    // bugfix 151118 - Lina Ichwan: Status pernikahan tidak lagi diperhitungkan dalam pemeriksaan cuti
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
                    if (Masa_Cuti == 90 && Keperluan.Equals("Cuti Tahunan"))
                    {
                        Keperluan = "Melahirkan";
                    }
                    Keperluan = model.Keperluan ?? "Cuti Tahunan";
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
                                    sqlconn.Close();
                                }
                            }
                        }

                        // tahap 7: proses pengiriman email kepada atasan dari karyawan ybs
                        // sistem pengiriman email disusun secara otomatis dan dikirimkan melalui server publik
                        // setting SMTP server
                        if (model.Jenis_Cuti != "3")
                        {
                            try
                            {
                                Msg = new StringBuilder();
                                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                                Msg.Append("<br />");
                                Msg.Append("Yth. " + Nama_Atasan + ",");
                                Msg.Append("<br /><br />");
                                if (model.Jenis_Cuti == "1")
                                {
                                    Msg.Append("Anda telah menerima permohonan cuti tahunan dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Mulai) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Selesai) + " dengan keperluan: " + Keperluan + ".<br /><br />");
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan pada alamat berikut ini.");

                                    if (Privilege_At == "Manager")
                                    {
                                        Msg.Append("<a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/ApprovalTahunan</a>");
                                    }
                                    else if (Privilege_At == "Supervisor")
                                    {
                                        Msg.Append("<a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/ApprovalTahunan</a>");
                                    }

                                    Msg.Append("<br /><br />");
                                    Msg.Append("Apabila anda berhalangan hadir, persetujuan cuti ini dapat diwakilkan.");
                                }
                                else if (model.Jenis_Cuti == "2")
                                {
                                    Msg.Append("Anda telah menerima permohonan cuti yaitu cuti khusus dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Mulai) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Selesai) + " dengan keperluan: " + Keperluan + ".<br /><br />");
                                    Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan pada alamat berikut ini.");

                                    if (Privilege_At == "Manager")
                                    {
                                        Msg.Append("<a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/ApprovalKhusus</a>");
                                    }
                                    else if (Privilege_At == "Supervisor")
                                    {
                                        Msg.Append("<a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/ApprovalKhusus</a>");
                                    }

                                    Msg.Append("<br /><br />");
                                    Msg.Append("Apabila anda berhalangan hadir, persetujuan cuti ini dapat diwakilkan.");
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
                                Session["ErrorData"] = smtpex.Message + " - " + (smtpex.InnerException != null ? smtpex.InnerException.ToString() : string.Empty);
                                return RedirectToAction("PengajuanError", "Notif");
                            }
                            catch (Exception e)
                            {
                                ViewBag.Message = e.Message;
                                Session["ErrorData"] = e.Message + " - " + (e.InnerException != null ? e.InnerException.ToString() : string.Empty);
                                return RedirectToAction("PengajuanError", "Notif");
                            }
                        }

                        return RedirectToAction("PengajuanSukses", "Notif");
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

                        if (string.IsNullOrEmpty(model.Keperluan) && model.Jenis_Cuti == "Cuti Khusus")
                        {
                            ViewData["ErrorMsg"] = "Keperluan cuti yang bersangkutan harus diisi.";
                        }

                        ViewData["NIK"] = model.NIK;

                        return View(model);
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pemuatan data karyawan. Silakan mencoba kembali. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public PartialViewResult GetKaryawan(String NIK, CutiModel model)
        {
            SetPrivilege();
            // jika NIK tidak masuk melalui combo box, isi lewat model
            if (String.IsNullOrEmpty(NIK) && !String.IsNullOrEmpty(model.NIK))
            {
                NIK = model.NIK.ToString().Trim();
            }
            else if (String.IsNullOrEmpty(NIK) && String.IsNullOrEmpty(model.NIK))
            {
                NIK = String.Empty;
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
                        model.Nama_Advisor = sdr["Nama_Advisor"].ToString().Trim();
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

        public PartialViewResult GetMutasi(String NIK, MutasiModel model)
        {
            SetPrivilege();
            // jika NIK tidak masuk melalui combo box, isi lewat model
            if (String.IsNullOrEmpty(NIK) && !String.IsNullOrEmpty(model.NIK))
            {
                NIK = model.NIK.ToString().Trim();
            }
            else if (String.IsNullOrEmpty(NIK) && String.IsNullOrEmpty(model.NIK))
            {
                NIK = String.Empty;
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
                        model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                        model.Cabang = sdr["Cabang"].ToString().Trim();
                        model.Jabatan = sdr["Jabatan"].ToString().Trim();
                        model.Departemen = sdr["Departemen"].ToString().Trim();
                        model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                    }

                    return PartialView("_PanelMutasiInfo", model);
                }
                catch (SqlException ex)
                {
                    ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pemuatan data karyawan. Silakan mencoba kembali. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return PartialView("_PanelMutasiInfo", model);
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
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin")
                {
                    var model = new CutiModel();
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
                                model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                model.Privilege = sdr["Privilege"].ToString().Trim();
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

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ApprovalKhususPartial()
        {
            var model = DB.TT_Approval_K; // hanya penegasan, nilai awal tidak diubah
            return PartialView("_ApprovalKhusus", model.ToList());
        }

        public ActionResult MultiKhususPartial()
        {
            Pemberi = (Request.Params["Pemberi"] ?? "A. Irawan Gozali").ToString();

            if (!string.IsNullOrEmpty(Pemberi))
            {
                var model = (from Approval in DB.TT_Approval_K
                             where Approval.Pemberi == Pemberi
                             select Approval);
                return PartialView("_MultiKhusus", model.ToList());
            }
            else
            {
                var model = (from Approval in DB.TT_Approval_K
                             select Approval);
                return PartialView("_MultiKhusus", model.ToList());
            }
        }

        // Bagian script engine untuk proses persetujuan cuti khusus

        #region Proses Persetujuan
        public ActionResult SetujuCK(string id, CutiModel model)
        {
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
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Atasan = sdr["Nama_Karyawan"].ToString().Trim();
                    }
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            // tahap 1: tandai status cuti = setuju, tanggal persetujuan hari ini
            Status_Cuti = "Disetujui";
            Tgl_Setuju = DateTime.Now;
            ApprovedBy = Nama_Atasan;

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
                return View("ApprovalKhusus", model);
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
                    return View("ApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
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
                cmd.Parameters.Add("@Tgl_Setuju", SqlDbType.DateTime).Value = Tgl_Setuju;
                cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                cmd.Parameters.Add("@Approved_By", SqlDbType.NVarChar).Value = Nama_Atasan;
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
                    return View("ApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
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
                        NIK_Atasan = sdr["NIK_Atasan"]?.ToString()?.Trim();
                        Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        NIK_Supervisor = sdr["NIK_Supervisor"]?.ToString()?.Trim();
                        Nama_Supervisor = sdr["Nama_Supervisor"] != DBNull.Value ? sdr["Nama_Supervisor"].ToString().Trim() : string.Empty;
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }

                ViewBag.Privilege = Privilege;

                // temukan email atasan dari karyawan ybs
                using (var sqlconn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", sqlconn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK_Atasan;
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
                        sqlconn.Close();
                    }
                }

                // temukan email supervisor dari karyawan ybs
                if (!string.IsNullOrEmpty(Nama_Supervisor))
                {
                    using (var sqlconn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SM_Karyawan", sqlconn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK_Supervisor;
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
                            sqlconn.Close();
                        }
                    }
                }

                // tahap 4: pengiriman email pemberitahuan pada karyawan yang pengajuannya disetujui/ditolak
                // setting SMTP server
                try
                {
                    var kar = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK_Atasan);
                    if (kar != null)
                    {
                        switch (kar.Jenis_Kelamin)
                        {
                            case "Pria":
                                GenderPrefix = "Bpk. ";
                                break;

                            case "Wanita":
                                GenderPrefix = "Ibu ";
                                break;

                            default:
                                GenderPrefix = " ";
                                break;
                        }
                    }
                    else
                    {
                        GenderPrefix = " ";
                    }

                    Msg = new StringBuilder();
                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                    Msg.Append("<br />");
                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                    Msg.Append("<br /><br />");
                    Msg.Append("Permohonan cuti khusus dengan kode # " + ID_Cuti + " telah disetujui oleh " + GenderPrefix + ApprovedBy + " pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
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

                    if (!string.IsNullOrEmpty(Email_Supervisor))
                    {
                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email, Email_Supervisor);
                    }
                    else
                    {
                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                    }
                }
                catch (SmtpException smtpex)
                {
                    ViewBag.Message = smtpex.Message;
                    return RedirectToAction("ApprovalError", "Notif");
                }
                catch (Exception e)
                {
                    ViewBag.Message = e.Message;
                    return RedirectToAction("ApprovalError", "Notif");
                }

                // tahap 4: tampilkan konfirmasi persetujuan
                return RedirectToAction("ApprovalSukses", "Notif");
            }
        }
        #endregion

        // Tidak digunakan lagi
        #region Proses Persetujuan Multi Select
        //public ActionResult SetujuMSCK(String[] arg, CutiModel model)
        //{
        //    ViewData["Selection"] = "Standard";
        //    Status_Cuti = "Disetujui";
        //    Tgl_Setuju = DateTime.Now;
        //    ApprovedBy = User.Identity.Name;
        //    Session["Status_Cuti"] = Status_Cuti;

        //    int i = 0;

        //    // mendapatkan alamat email berdasarkan nama atasan
        //    if (!string.IsNullOrEmpty(model.Pemberi))
        //    {
        //        Nama_Atasan = model.Pemberi.ToString().Trim();
        //    }

        //    Email_Atasan = ((from Kar in DB.TM_Karyawans
        //                     where Kar.Nama_Atasan == Nama_Atasan
        //                     select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
        //                                                      where Kar.Nama_Atasan == Nama_Atasan
        //                                                      select Kar.Email)).ToString().Trim() ?? string.Empty;

        //    Email_Supervisor = ((from Kar in DB.TM_Karyawans
        //                         where Kar.Nama_Supervisor == Nama_Supervisor
        //                     select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
        //                                                      where Kar.Nama_Supervisor == Nama_Supervisor
        //                                                      select Kar.Email)).ToString().Trim() ?? string.Empty;

        //    // proses inti persetujuan
        //    // memeriksa indeks argumen paket HTTP untuk menemukan ID pengajuan cuti yang dipilih atasan
        //    foreach (String idc in arg)
        //    {
        //        ID_Cuti = idc.ToString().Trim();

        //        // proses ID terpilih satu per satu
        //        using (var conn = new SqlConnection(connstring))
        //        {
        //            cmd = new SqlCommand("SP_ApproveKhusus", conn);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
        //            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

        //            try
        //            {
        //                if (conn.State != ConnectionState.Open) { conn.Open(); }
        //                sdr = cmd.ExecuteReader();
        //                while (sdr.Read())
        //                {
        //                    NIK = sdr["NIK"].ToString().Trim();
        //                    Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
        //                }
        //            }
        //            catch (SqlException ex)
        //            {
        //                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
        //                SetPrivilege();
        //                return View("ApprovalKhusus", model);
        //            }
        //            catch (Exception e)
        //            {
        //                ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
        //                SetPrivilege();
        //                return View("ApprovalKhusus", model);
        //            }
        //            finally
        //            {
        //                sdr.Close();
        //                conn.Close();
        //            }

        //            Email_Perusahaan = (from Kar in DB.TM_Karyawans
        //                                where Kar.NIK == NIK
        //                                select Kar.Email_Perusahaan).ToString() ?? string.Empty;
        //            Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

        //            Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).FirstOrDefault() ?? string.Empty;

        //            ViewBag.Privilege = Privilege;

        //            // baca email mana yang valid
        //            if (!string.IsNullOrEmpty(Email_Perusahaan))
        //            {
        //                Email_Temp = Email_Perusahaan;
        //            }
        //            else
        //            {
        //                Email_Temp = Email;
        //            }

        //            // pembuatan pesan e-mail untuk bawahan
        //            EmailRequest = new MailMessage();
        //            EmailRequest.From = new MailAddress("noreply@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
        //            EmailRequest.To.Add(new MailAddress(Email_Temp, Email_Temp));
        //            EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Persetujuan Cuti";
        //            EmailRequest.IsBodyHtml = true;

        //            // definisi body dasar email
        //            var Msg = new StringBuilder();
        //            Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
        //            Msg.Append("<br />");
        //            Msg.Append("Yth. " + Nama_Karyawan + ",");
        //            Msg.Append("<br /><br />");
        //            Msg.Append("Permohonan cuti khusus anda dengan kode # " + ID_Cuti + " telah disetujui oleh atasan anda (" + ApprovedBy + ") pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
        //            if (ViewBag.Privilege == "Manager")
        //            {
        //                Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
        //            }
        //            else if (ViewBag.Privilege == "Supervisor")
        //            {
        //                Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
        //            }
        //            else
        //            {
        //                Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
        //            }
        //            Msg.Append("<br /><br />");
        //            Msg.Append("Atas perhatiannya kami mengucapkan terima kasih.");
        //            Msg.Append("<br /><br /><br /><br />");
        //            Msg.Append("Jakarta, " + string.Format("{0:dd MMMM yyyy}", DateTime.Now));
        //            Msg.Append("<br /><br />");
        //            Msg.Append("Hormat kami,");
        //            Msg.Append("<br /><br />");
        //            Msg.Append("<br /><br />");
        //            Msg.Append("<u>Human Resources Department ARISTA Group</u>");
        //            Msg.Append("</span>");

        //            // kosongkan kedua variabel email
        //            Email_Perusahaan = "";
        //            Email = "";

        //            EmailRequest.Body = Msg.ToString();

        //            // tahap 2: update riwayat cuti
        //            // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
        //            cmd = new SqlCommand("SP_ApproveKhusus", conn);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
        //            cmd.Parameters.Add("@Tgl_Setuju", SqlDbType.DateTime).Value = Tgl_Setuju;
        //            cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
        //            cmd.Parameters.Add("@Approved_By", SqlDbType.NVarChar).Value = User.Identity.Name;
        //            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Setuju";

        //            try
        //            {
        //                if (conn.State != ConnectionState.Open) { conn.Open(); }
        //                cmd.ExecuteNonQuery();
        //            }
        //            catch (SqlException ex)
        //            {
        //                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
        //                SetPrivilege();
        //                return View("ApprovalKhusus", model);
        //            }
        //            catch (Exception e)
        //            {
        //                ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
        //                SetPrivilege();
        //                return View("ApprovalKhusus", model);
        //            }
        //            finally
        //            {
        //                conn.Close();
        //            }
                    
        //            // update 170201: jika data pada list approval tidak terhapus, hapus dengan query ini
        //            var query = DB.TT_Approval_K.FirstOrDefault(x => x.ID_Cuti == ID_Cuti);
        //            if (query != null)
        //            {
        //                DB.TT_Approval_K.Remove(query);
        //                DB.SaveChanges();
        //            }

        //            try
        //            {
        //                if (!string.IsNullOrEmpty(Email_Supervisor))
        //                {
        //                    if (arg.Length <= 3)
        //                    {
        //                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email, Email_Supervisor);
        //                    }
        //                }
        //                else
        //                {
        //                    if (arg.Length <= 3)
        //                    {
        //                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
        //                    }
        //                }
        //            }
        //            catch (SmtpException smtpex)
        //            {
        //                ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pengiriman email kepada atasan dengan pesan berikut: \"" + smtpex.Message + "\"";
        //                HttpContext.Response.AddHeader("FAILED_LOAD", "1");
        //                Response.StatusCode = 401;
        //                // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
        //            }
        //            catch (Exception e)
        //            {
        //                ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
        //                HttpContext.Response.AddHeader("FAILED_LOAD", "1");
        //                Response.StatusCode = 401;
        //                // return Redirect(DevExpressHelper.GetUrl(new { Controller = "Notif", Action = "MultiApprovalError" }));
        //            }

        //            i++;
        //            //if (i % 3 == 0)
        //            //    Thread.Sleep(10000);
        //        }
        //    }

        //    return RedirectToAction("MultiApprovalSukses", "Notif");
        //}
        #endregion

        #endregion

        #region Persetujuan Cuti Tahunan
        // Bagian tampilan form persetujuan untuk cuti tahunan
        public ActionResult ApprovalTahunan(String id)
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
                    var model = new CutiModel();
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
                                model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                                model.Privilege = sdr["Privilege"].ToString().Trim();
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

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ApprovalTahunanPartial()
        {
            var model = DB.TT_Approval_T;
            return PartialView("_ApprovalTahunan", model.ToList());
        }
        
        // Tidak digunakan lagi
        public ActionResult MultiTahunanPartial()
        {
            Pemberi = (Request.Params["Pemberi"] ?? "A. Irawan Gozali").ToString();

            if (!string.IsNullOrEmpty(Pemberi))
            {
                var model = (from Approval in DB.TT_Approval_T
                             where Approval.Pemberi == Pemberi
                             select Approval);
                return PartialView("_MultiTahunan", model.ToList());
            }
            else
            {
                var model = (from Approval in DB.TT_Approval_T
                             select Approval);
                return PartialView("_MultiTahunan", model.ToList());
            }
        }

        // Bagian script engine untuk proses persetujuan cuti tahunan

        #region Proses Persetujuan
        public ActionResult SetujuCT(string id, CutiModel model)
        {
            Email_Temp = "";
            Email_Atasan = "";
            Email_Advisor = "";

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
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        NIK = sdr["NIK"].ToString().Trim();
                        Nama_Atasan = sdr["Nama_Karyawan"].ToString().Trim();
                    }
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }
            }

            // tahap 1: tandai status cuti = setuju, tanggal persetujuan hari ini
            Status_Cuti = "Disetujui";
            Tgl_Setuju = DateTime.Now;
            Session["Status_Cuti"] = Status_Cuti;
            ApprovedBy = Nama_Atasan;

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
                
                return View("ApprovalTahunan", model);
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
                    return View("ApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalTahunan", model);
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
                cmd.Parameters.Add("@Tgl_Setuju", SqlDbType.DateTime).Value = Tgl_Setuju;
                cmd.Parameters.Add("@Status_Cuti", SqlDbType.NVarChar).Value = Status_Cuti;
                cmd.Parameters.Add("@Approved_By", SqlDbType.NVarChar).Value = Nama_Atasan;
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
                    return View("ApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pembacaan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }

                // tahap 5: tambah jumlah cuti tahunan yang telah diambil sesuai masa cuti
                Cuti_Tahunan += Masa_Cuti;

                // tahap 6: kurangi jatah cuti tahunan untuk karyawan ybs
                cmd = new SqlCommand("SP_Sisa_Cuti", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_Cuti", SqlDbType.NVarChar).Value = ID_Cuti;
                cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                cmd.Parameters.Add("@Jatah_Cuti", SqlDbType.Int).Value = Jatah_Cuti;
                cmd.Parameters.Add("@Total_Cuti", SqlDbType.Int).Value = Total_Cuti;
                cmd.Parameters.Add("@Cuti_Pribadi", SqlDbType.Int).Value = Cuti_Tahunan;
                cmd.Parameters.Add("@Cuti_Massal", SqlDbType.Int).Value = Cuti_Massal;
                cmd.Parameters.Add("@Sisa_Cuti", SqlDbType.Int).Value = Sisa_Cuti;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Cuti";

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
                        NIK_Atasan = sdr["NIK_Atasan"]?.ToString()?.Trim();
                        Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        NIK_Supervisor = sdr["NIK_Supervisor"]?.ToString()?.Trim();
                        Nama_Supervisor = sdr["Nama_Supervisor"] != DBNull.Value ? sdr["Nama_Supervisor"].ToString().Trim() : string.Empty;
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
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

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                }

                ViewBag.Privilege = Privilege;

                // temukan email atasan dari karyawan ybs
                using (var sqlconn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SM_Karyawan", sqlconn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK_Atasan;
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
                        return View("ApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya

                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        sqlconn.Close();
                    }
                }

                // temukan email atasan dari karyawan ybs
                if (!string.IsNullOrEmpty(Nama_Supervisor))
                {
                    using (var sqlconn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SM_Karyawan", sqlconn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK_Supervisor;
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
                            return View("ApprovalTahunan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya

                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("ApprovalTahunan", model);
                        }
                        finally
                        {
                            sdr.Close();
                            sqlconn.Close();
                        }
                    }
                }

                // tahap 7: pengiriman email pemberitahuan pada karyawan yang pengajuannya disetujui/ditolak
                // setting SMTP server
                try
                {
                    var kar = DB.TM_Karyawans.FirstOrDefault(x => x.Nama_Karyawan == ApprovedBy);
                    if (kar != null)
                    {
                        switch (kar.Jenis_Kelamin)
                        {
                            case "Pria":
                                GenderPrefix = "Bpk. ";
                                break;

                            case "Wanita":
                                GenderPrefix = "Ibu ";
                                break;

                            default:
                                GenderPrefix = " ";
                                break;
                        }
                    }
                    else
                    {
                        GenderPrefix = " ";
                    }

                    Msg = new StringBuilder();
                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                    Msg.Append("<br />");
                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                    Msg.Append("<br /><br />");
                    Msg.Append("Permohonan cuti tahunan dengan kode # " + ID_Cuti + " telah disetujui oleh " + GenderPrefix + ApprovedBy + " pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
                    if (ViewBag.Privilege == "Admin")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
                    }
                    else if (ViewBag.Privilege == "Supervisor")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
                    }
                    else
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
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

                    if (!string.IsNullOrEmpty(Email_Supervisor))
                    {
                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email, Email_Supervisor);
                    }
                    else
                    {
                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                    }
                }
                catch (SmtpException smtpex)
                {
                    ViewBag.Message = smtpex.Message;
                    return RedirectToAction("ApprovalError", "Notif");
                }
                catch (Exception e)
                {
                    ViewBag.Message = e.Message;
                    return RedirectToAction("ApprovalError", "Notif");
                }

                return RedirectToAction("ApprovalSukses", "Notif");
            }
        }
        #endregion

        #region Proses Persetujuan Multi Select
        public ActionResult SetujuMSCT(String[] arg, CutiModel model)
        {
            ViewData["Selection"] = "Standard";
            Status_Cuti = "Disetujui";
            Tgl_Setuju = DateTime.Now;
            ApprovedBy = User.Identity.Name;
            Session["Status_Cuti"] = Status_Cuti;

            int i = 0;

            // mendapatkan alamat email berdasarkan nama atasan
            if (!string.IsNullOrEmpty(model.Pemberi))
            {
                Nama_Atasan = model.Pemberi.ToString().Trim();
            }

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
                        return View("ApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    Email_Perusahaan = (from Kar in DB.TM_Karyawans
                                        where Kar.NIK == NIK
                                        select Kar.Email_Perusahaan).ToString() ?? string.Empty;
                    Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

                    Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).FirstOrDefault() ?? string.Empty;

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

                    // pembuatan pesan e-mail untuk bawahan
                    EmailRequest = new MailMessage();
                    EmailRequest.From = new MailAddress("noreply@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
                    EmailRequest.To.Add(new MailAddress(Email_Temp, Email_Temp));
                    EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Persetujuan Cuti";
                    EmailRequest.IsBodyHtml = true;

                    // definisi body dasar email
                    var Msg = new StringBuilder();
                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                    Msg.Append("<br />");
                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                    Msg.Append("<br /><br />");
                    Msg.Append("Permohonan cuti tahunan dengan kode # " + ID_Cuti + " telah disetujui oleh (" + GenderPrefix + ApprovedBy + ") pada tanggal " + Tgl_Setuju.ToLongDateString() + ".<br />");
                    if (ViewBag.Privilege == "Manager")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
                    }
                    else if (ViewBag.Privilege == "Supervisor")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
                    }
                    else
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
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

                    // kosongkan kedua variabel email
                    Email_Perusahaan = "";
                    Email = "";

                    EmailRequest.Body = Msg.ToString();

                    // tahap 2: update riwayat cuti
                    // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
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
                        return View("ApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
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

                    try
                    {
                        if (arg.Length <= 3)
                        {
                            Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                        }
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
                    //if (i % 3 == 0)
                    //    Thread.Sleep(10000);
                }
            }

            return RedirectToAction("MultiApprovalSukses", "Notif");
        }
        #endregion

        #endregion

        #region Penolakan Cuti Khusus
        // Bagian script engine untuk proses penolakan cuti khusus
        public ActionResult TolakCK(string id, CutiModel model)
        {
            // tahap 1: tandai status cuti = ditolak
            Status_Cuti = "Ditolak";
            RejectedBy = User.Identity.Name;
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
                return View("ApprovalKhusus", model);
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
                    return View("ApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
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
                    return View("ApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
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
                        Nama_Supervisor = sdr["Nama_Supervisor"] != DBNull.Value ? sdr["Nama_Supervisor"].ToString().Trim() : string.Empty;
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database

                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalKhusus", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
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
                        return View("ApprovalKhusus", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya

                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalKhusus", model);
                    }
                    finally
                    {
                        sdr.Close();
                        sqlconn.Close();
                    }
                }

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
                            return View("ApprovalKhusus", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya

                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("ApprovalKhusus", model);
                        }
                        finally
                        {
                            sdr.Close();
                            sqlconn.Close();
                        }
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
                    Msg.Append("Permohonan cuti khusus dengan kode # " + ID_Cuti + " telah ditolak oleh atasan (" + RejectedBy + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                    if (ViewBag.Privilege == "Admin")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
                    }
                    else if (ViewBag.Privilege == "Supervisor")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
                    }
                    else
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
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
                    if (!string.IsNullOrEmpty(Email_Perusahaan) && !string.IsNullOrEmpty(Email))
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
                    return RedirectToAction("ApprovalError", "Notif");
                }
                catch (Exception e)
                {
                    ViewBag.Message = e.Message;
                    return RedirectToAction("ApprovalError", "Notif");
                }

                // tampilkan konfirmasi penolakan cuti
                return RedirectToAction("ApprovalReject", "Notif");
            }
        }
        #endregion

        #region Penolakan Cuti Khusus (Multi Select)
        public ActionResult TolakMSCK(String[] arg, String res, CutiModel model)
        {
            ViewData["Selection"] = "Standard";
            Status_Cuti = "Ditolak";
            RejectedBy = User.Identity.Name;
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

            // mendapatkan alamat email berdasarkan nama atasan
            if (!string.IsNullOrEmpty(model.Pemberi))
            {
                Nama_Atasan = model.Pemberi.ToString().Trim();
            }

            Email_Atasan = ((from Kar in DB.TM_Karyawans
                             where Kar.Nama_Atasan == Nama_Atasan
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Atasan == Nama_Atasan
                                                              select Kar.Email)).ToString().Trim() ?? string.Empty;

            Email_Supervisor = ((from Kar in DB.TM_Karyawans
                                 where Kar.Nama_Supervisor == Nama_Supervisor
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Supervisor == Nama_Supervisor
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
                        return View("ApprovalKhusus", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalKhusus", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    Email_Perusahaan = (from Kar in DB.TM_Karyawans
                                        where Kar.NIK == NIK
                                        select Kar.Email_Perusahaan).ToString() ?? string.Empty;
                    Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

                    Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).FirstOrDefault() ?? string.Empty;

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

                    // pembuatan pesan e-mail untuk bawahan
                    EmailRequest = new MailMessage();
                    EmailRequest.From = new MailAddress("noreply@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
                    EmailRequest.To.Add(new MailAddress(Email_Temp, Email_Temp));
                    EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Persetujuan Cuti";
                    EmailRequest.IsBodyHtml = true;

                    // definisi body dasar email
                    var Msg = new StringBuilder();
                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                    Msg.Append("<br />");
                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                    Msg.Append("<br /><br />");
                    Msg.Append("Permohonan cuti khusus dengan kode # " + ID_Cuti + " telah ditolak oleh atasan (" + RejectedBy + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                    if (ViewBag.Privilege == "Manager")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
                    }
                    else if (ViewBag.Privilege == "Supervisor")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
                    }
                    else
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
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

                    // kosongkan kedua variabel email
                    Email_Perusahaan = "";
                    Email = "";

                    EmailRequest.Body = Msg.ToString();

                    // tahap 2: update riwayat cuti
                    // disertai dengan penghapusan data pada seluruh tabel temporer dengan ID cuti yang sama
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
                        return View("ApprovalKhusus", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalKhusus", model);
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

                    try
                    {
                        SMTPServer.Send(EmailRequest);
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
            }

            return RedirectToAction("MultiApprovalSukses", "Notif");
        }

        #endregion

        #region Penolakan Cuti Tahunan
        // Bagian script engine untuk proses penolakan cuti tahunan
        public ActionResult TolakCT(string id, CutiModel model)
        {
            // tahap 1: tandai status cuti = ditolak
            Status_Cuti = "Ditolak";
            RejectedBy = User.Identity.Name;
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
                return View("ApprovalTahunan", model);
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
                    return View("ApprovalTahunan", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalTahunan", model);
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
                        Nama_Supervisor = sdr["Nama_Supervisor"] != DBNull.Value ? sdr["Nama_Supervisor"].ToString().Trim() : string.Empty;
                        Privilege = sdr["Privilege"].ToString().Trim();
                    }
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

                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalTahunan", model);
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
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
                        return View("ApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya

                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        sqlconn.Close();
                    }
                }

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
                            return View("ApprovalTahunan", model);
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya

                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("ApprovalTahunan", model);
                        }
                        finally
                        {
                            sdr.Close();
                            sqlconn.Close();
                        }
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
                    Msg.Append("Permohonan cuti tahunan dengan kode # " + ID_Cuti + " telah ditolak oleh atasan (" + RejectedBy + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                    if (ViewBag.Privilege == "Admin")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
                    }
                    else if (ViewBag.Privilege == "Supervisor")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
                    }
                    else
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
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

                    if (!string.IsNullOrEmpty(Email_Supervisor))
                    {
                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email, Email_Supervisor);
                    }
                    else
                    {
                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                    }
                }
                catch (SmtpException smtpex)
                {
                    ViewBag.Message = smtpex.Message;
                    return RedirectToAction("ApprovalError", "Notif");
                }
                catch (Exception e)
                {
                    ViewBag.Message = e.Message;
                    return RedirectToAction("ApprovalError", "Notif");
                }


                // tampilkan konfirmasi penolakan cuti
                return RedirectToAction("ApprovalReject", "Notif");
            }
        }
        #endregion

        #region Penolakan Cuti Tahunan (Multi Select)
        public ActionResult TolakMSCT(String[] arg, String res, CutiModel model)
        {
            ViewData["Selection"] = "Standard";
            Status_Cuti = "Ditolak";
            RejectedBy = User.Identity.Name;
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

            // mendapatkan alamat email berdasarkan nama atasan
            if (!string.IsNullOrEmpty(model.Pemberi))
            {
                Nama_Atasan = model.Pemberi.ToString().Trim();
            }

            Email_Atasan = ((from Kar in DB.TM_Karyawans
                             where Kar.Nama_Atasan == Nama_Atasan
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Atasan == Nama_Atasan
                                                              select Kar.Email)).ToString().Trim() ?? string.Empty;

            Email_Supervisor = ((from Kar in DB.TM_Karyawans
                                 where Kar.Nama_Supervisor == Nama_Supervisor
                             select Kar.Email_Perusahaan) ?? (from Kar in DB.TM_Karyawans
                                                              where Kar.Nama_Supervisor == Nama_Supervisor
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
                        return View("ApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    Email_Perusahaan = (from Kar in DB.TM_Karyawans
                                        where Kar.NIK == NIK
                                        select Kar.Email_Perusahaan).ToString() ?? string.Empty;
                    Email = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Email).ToString() ?? string.Empty;

                    Privilege = (from Kar in DB.TM_Karyawans where Kar.NIK == NIK select Kar.Privilege).FirstOrDefault() ?? string.Empty;

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

                    // pembuatan pesan e-mail untuk bawahan
                    EmailRequest = new MailMessage();
                    EmailRequest.From = new MailAddress("noreply@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
                    EmailRequest.To.Add(new MailAddress(Email_Temp, Email_Temp));
                    EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Persetujuan Cuti";
                    EmailRequest.IsBodyHtml = true;

                    // definisi body dasar email
                    var Msg = new StringBuilder();
                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                    Msg.Append("<br />");
                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                    Msg.Append("<br /><br />");
                    Msg.Append("Permohonan cuti tahunan dengan kode # " + ID_Cuti + " telah ditolak oleh atasan (" + RejectedBy + ") pada tanggal " + string.Format("{0:dd MMMM yyyy}", DateTime.Now.Date) + ".<br />");
                    if (ViewBag.Privilege == "Manager")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Manager/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
                    }
                    else if (ViewBag.Privilege == "Supervisor")
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
                    }
                    else
                    {
                        Msg.Append("Mohon segera melakukan pengecekan pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
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
                        return View("ApprovalTahunan", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal saat memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("ApprovalTahunan", model);
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

                    try
                    {
                        if (!string.IsNullOrEmpty(Email_Supervisor))
                        {
                            Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email, Email_Supervisor);
                        }
                        else
                        {
                            Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                        }

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
            }

            return RedirectToAction("MultiApprovalSukses", "Notif");
        }

        #endregion

        #region Pembatalan Cuti Khusus
        public ActionResult BatalKhusus(string id)
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
                                return View("BatalKhusus", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("BatalKhusus", model);
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
                                return View("BatalKhusus", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("BatalKhusus", model);
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
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
                                    sqlconn.Close();
                                }
                            }

                            // tahap 2: susun stored procedure
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
                                    if (ViewBag.Privilege == "Admin")
                                    {
                                        Msg.Append("Cuti khusus anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan.");
                                    }
                                    else if (ViewBag.Privilege == "Supervisor")
                                    {
                                        Msg.Append("Cuti khusus anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan.");
                                    }
                                    else
                                    {
                                        Msg.Append("Cuti khusus anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan.");
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
                                    return RedirectToAction("BatalCutiSukses", "Notif");
                                }
                                catch (Exception e)
                                {
                                    ViewBag.Message = e.Message;
                                    return RedirectToAction("BatalCutiSukses", "Notif");
                                }
                                return RedirectToAction("BatalCutiSukses", "Notif");
                            }
                            catch (SqlException ex)
                            {
                                // jika terdapat gangguan koneksi database
                                ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                                SetPrivilege();
                                return View("BatalKhusus", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("BatalKhusus", model);
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                    else
                    {
                        return View();
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
            return PartialView("_BatalKhusus", model.ToList());
        }

        #endregion

        #region Pembatalan Cuti Tahunan
        public ActionResult BatalTahunan(string id)
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
                                
                                return View("BatalTahunan", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                
                                return View("BatalTahunan", model);
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
                                

                                return View("BatalTahunan", model);
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                

                                return View("BatalTahunan", model);
                            }
                            finally
                            {
                                sdr.Close();
                                conn.Close();
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
                                    sqlconn.Close();
                                }
                            }

                            // tahap 2: susun stored procedure
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
                                    if (ViewBag.Privilege == "Admin")
                                    {
                                        Msg.Append("Cuti tahunan anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Manager/Riwayat</a>");
                                    }
                                    else if (ViewBag.Privilege == "Supervisor")
                                    {
                                        Msg.Append("Cuti tahunan anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://" + ServerIP + "/Supervisor/Riwayat\">http://" + ServerIP + "/Supervisor/Riwayat</a>");
                                    }
                                    else
                                    {
                                        Msg.Append("Cuti tahunan anda dengan nomor ID # " + ID_Cuti + " telah dibatalkan. Silakan cek status cuti anda pada alamat berikut ini: <a href=\"http://" + ServerIP + "/User/Riwayat\">http://" + ServerIP + "/User/Riwayat</a>");
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
                                    return RedirectToAction("BatalCutiSukses", "Notif");
                                }
                                catch (Exception e)
                                {
                                    ViewBag.Message = e.Message;
                                    return RedirectToAction("BatalCutiSukses", "Notif");
                                }
                                return RedirectToAction("BatalCutiSukses", "Notif");
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
                        return View();
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
            return PartialView("_BatalTahunan", model.ToList());
        }

        #endregion

        #endregion

        #region Pengaturan Mutasi Karyawan

        #region Mutasi Karyawan Level Staf
        public ActionResult MutasiStaf(MutasiModel model)
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

                        model.Tgl_Mutasi = DateTime.Now.Date;
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
                Perusahaan = model.Perusahaan.ToString().Trim();
                Cabang = model.Cabang.ToString().Trim();
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

                if (!String.IsNullOrEmpty(model.Staf_Ganti))
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

                    /*
                    if (!string.IsNullOrEmpty(ID_Mutasi))
                    {
                        ID_Baru = Convert.ToInt64(ID_Mutasi);
                    }
                    else
                    {
                        ID_Baru = 0;
                    }

                    ID_Baru++;
                    ID_Mutasi = ID_Baru.ToString("000000000000");
                    */

                    // buat nomor mutasi
                    int seed = 0;

                    String no_mutasi = DB.TM_Mutasis.Max(x => x.ID_Mutasi);
                    if (String.IsNullOrEmpty(no_mutasi))
                    {
                        seed = 1;
                    }
                    else
                    {
                        DateTime tgl = DateTime.ParseExact(no_mutasi.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                        if (tgl != DateTime.Now.Date)
                        {
                            seed = 1;
                        }
                        else
                        {
                            seed = Convert.ToInt32(no_mutasi.Substring(8, 4));
                            seed++;
                        }
                        // seed = Convert.ToInt32(no_mutasi.Substring(8, 4));
                    }

                    ID_Mutasi = String.Format("{0:yyyyMMdd}", model.Tgl_Mutasi) + String.Format("{0:0000}", seed);

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
                    cmd = new SqlCommand("SM_Mutasi", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Mutasi", SqlDbType.NVarChar).Value = ID_Mutasi;
                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
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

                    if (!String.IsNullOrEmpty(model.Staf_Ganti))
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

                        // modifikasi tanggal perubahan sebelum membuat pesan email
                        var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == model.NIK);
                        if (karyawan != null)
                        {
                            karyawan.ModifiedBy = User.Identity.Name;
                            karyawan.ModifiedDate = DateTime.Now;
                            DB.SaveChanges();
                        }

                        try
                        {
                            // setting SMTP server
                            Msg = new StringBuilder();
                            Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                            Msg.Append("<br />");
                            Msg.Append("Yth. " + Nama_Karyawan + ",");
                            Msg.Append("<br /><br />");
                            Msg.Append("Anda telah melakukan proses mutasi karyawan dengan rincian sebagai berikut: <br />");
                            if (!string.IsNullOrEmpty(model.Perusahaan_Baru) && Perusahaan_Baru != Perusahaan)
                            {
                                Msg.Append("Perusahaan Baru: " + Perusahaan_Baru + "<br />");
                            }
                            else if (!string.IsNullOrEmpty(model.Cabang_Baru) && Cabang_Baru != Cabang)
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
                            else if (!string.IsNullOrEmpty(model.Atasan_Baru) && Atasan_Baru != Nama_Atasan)
                            {
                                Msg.Append("Atasan Baru: " + Atasan_Baru + "<br />");
                            }
                            Msg.Append("<br /><br />");
                            Msg.Append("Tanggal Mutasi: " + string.Format("{0:dd MMMM yyyy}", Tgl_Mutasi));
                            Msg.Append("<br /><br />");
                            Msg.Append("Keterangan Mutasi: " + Keterangan);
                            Msg.Append("<br /><br />");
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
                            return RedirectToAction("MutasiError", "Notif");
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = e.Message;
                            return RedirectToAction("MutasiError", "Notif");
                        }

                        return RedirectToAction("MutasiSukses", "Notif");
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

        #region Mutasi Karyawan Level Manajer/Supervisor
        public ActionResult MutasiAtasan(MutasiModel model)
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

                        model.NIK = string.Empty;
                        model.GantiAtasan = true;
                        model.Tgl_Mutasi = DateTime.Now.Date;

                        // bersihkan daftar NIK yang pernah diisi
                        Session["ListNIK"] = null;
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
        public ActionResult MutasiAtasan(MutasiModel model, string url)
        {
            var listNIK = Session["ListNIK"] as List<KaryawanModel>;

            // update 220615: cukup menggunakan NIK atasan saja
            string nikAtasan = model.NIK;
            string namaAtasan = Providers.GetKaryawanByNIK(nikAtasan)?.FirstOrDefault()?.Nama_Karyawan;
            string nikPengganti = model.Atasan_Baru;
            string namaPengganti = Providers.GetKaryawanByNIK(nikPengganti)?.FirstOrDefault()?.Nama_Karyawan;

            using (var context = new HRISContext())
            {
                try
                {
                    if (listNIK != null && listNIK.Count > 0)
                    {
                        foreach (var items in listNIK)
                        {
                            var entry = context.TM_Karyawans.FirstOrDefault(x => x.NIK == items.NIK);
                            if (entry != null)
                            {
                                // periksa terlebih dahulu posisi atasan ybs adalah manajer atau supervisor
                                if (!string.IsNullOrWhiteSpace(entry.NIK_Atasan) && entry.NIK_Atasan.Equals(nikAtasan))
                                {
                                    entry.NIK_Atasan = nikPengganti;
                                    entry.Nama_Atasan = namaPengganti;
                                }
                                else if (!string.IsNullOrWhiteSpace(entry.NIK_Supervisor) && entry.NIK_Supervisor.Equals(nikAtasan))
                                {
                                    entry.NIK_Supervisor = nikPengganti;
                                    entry.Nama_Supervisor = namaPengganti;
                                }

                                entry.ModifiedBy = User.Identity.Name;
                                entry.ModifiedDate = model.Tgl_Mutasi;
                            }
                        }
                    }
                    else
                    {
                        ViewData["ErrorMsg"] = "Anda belum memilih karyawan untuk proses mutasi atasan.";
                        SetPrivilege();
                        return View(model);
                    }

                    var uuid = Guid.NewGuid();

                    // simpan juga pada tabel history mutasi yang sudah ada sebelumnya
                    var mutasi = new TM_Mutasi
                    {
                        ID_Mutasi = uuid.ToString(),
                        NIK = nikAtasan,
                        Nama_Karyawan = namaAtasan,
                        NIK_Atasan_Baru = nikPengganti,
                        Atasan_Baru = namaPengganti,
                        Tgl_Mutasi = model.Tgl_Mutasi
                    };

                    context.TM_Mutasis.Add(mutasi);

                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["ErrorMsg"] = "Terjadi kegagalan dalam proses mutasi atasan. Pesan: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View(model);
                }
            }

            #region Tidak digunakan (update 220615)
            //if (!string.IsNullOrEmpty(model.NIK))
            //{
            //    // tahap 1: transfer model ke variabel kendali
            //    NIK = model.NIK.ToString().Trim();
            //    Nama_Karyawan = model.Nama_Karyawan.ToString().Trim();
            //    Perusahaan = model.Perusahaan.ToString().Trim();
            //    Cabang = model.Cabang.ToString().Trim();
            //    Jabatan = model.Jabatan.ToString().Trim();
            //    Departemen = model.Departemen.ToString().Trim();
            //    Nama_Atasan = model.Nama_Atasan.ToString().Trim();

            //    if (!string.IsNullOrEmpty(model.Nama_Supervisor))
            //    {
            //        Nama_Supervisor = model.Nama_Supervisor.ToString().Trim();
            //    }

            //    Tgl_Mutasi = (DateTime)Convert.ToDateTime(model.Tgl_Mutasi);

            //    // periksa keberadaan input pada perusahaan, cabang, jabatan, departemen & atasan
            //    if (!string.IsNullOrEmpty(model.Perusahaan_Baru))
            //    {
            //        Perusahaan_Baru = model.Perusahaan_Baru.ToString().Trim();
            //    }
            //    else
            //    {
            //        // pertahankan perusahaan asal
            //        Perusahaan_Baru = Perusahaan;
            //    }

            //    if (!string.IsNullOrEmpty(model.Cabang_Baru))
            //    {
            //        Cabang_Baru = model.Cabang_Baru.ToString().Trim();
            //    }
            //    else
            //    {
            //        // pertahankan cabang asal
            //        Cabang_Baru = Cabang;
            //    }

            //    if (!string.IsNullOrEmpty(model.Jabatan_Baru))
            //    {
            //        // jika ada jabatan baru
            //        Jabatan_Baru = model.Jabatan_Baru.ToString().Trim();
            //    }
            //    else
            //    {
            //        // pertahankan jabatan lama
            //        Jabatan_Baru = Jabatan;
            //    }

            //    if (!string.IsNullOrEmpty(model.Departemen_Baru))
            //    {
            //        Departemen_Baru = model.Departemen_Baru.ToString().Trim();
            //    }
            //    else
            //    {
            //        // pertahankan departemen lama
            //        Departemen_Baru = Departemen;
            //    }

            //    if (!string.IsNullOrEmpty(model.Atasan_Baru))
            //    {
            //        Atasan_Baru = model.Atasan_Baru.ToString().Trim();
            //    }
            //    else
            //    {
            //        // pertahankan atasan lama
            //        Atasan_Baru = Nama_Atasan;
            //    }

            //    if (!string.IsNullOrEmpty(model.Supervisor_Baru))
            //    {
            //        Supervisor_Baru = model.Supervisor_Baru.ToString().Trim();
            //    }
            //    else if (!string.IsNullOrEmpty(model.Nama_Supervisor))
            //    {
            //        Supervisor_Baru = Nama_Supervisor;
            //    }

            //    if (!string.IsNullOrEmpty(model.Atasan_Ganti))
            //    {
            //        Atasan_Ganti = model.Atasan_Ganti.ToString().Trim();
            //    }
            //    else
            //    {
            //        Atasan_Ganti = Nama_Karyawan;
            //    }

            //    if (!String.IsNullOrEmpty(model.Atasan_Asal))
            //    {
            //        Atasan_Asal = model.Atasan_Asal.ToString().Trim();
            //    }

            //    if (!string.IsNullOrEmpty(model.Keterangan))
            //    {
            //        Keterangan = model.Keterangan?.ToString().Trim();
            //    }

            //    // tentukan privilege asal dari karyawan ybs
            //    using (var conn = new SqlConnection(connstring))
            //    {
            //        cmd = new SqlCommand("SM_Karyawan", conn);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
            //        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

            //        try
            //        {
            //            if (conn.State != ConnectionState.Open) { conn.Open(); }
            //            sdr = cmd.ExecuteReader();
            //            while (sdr.Read())
            //            {
            //                Email = sdr["Email"].ToString().Trim();
            //                Email_Perusahaan = sdr["Email_Perusahaan"].ToString().Trim();
            //                Privilege_S = sdr["Privilege"].ToString().Trim();
            //            }
            //        }
            //        catch (SqlException ex)
            //        {
            //            // jika terdapat gangguan koneksi database
            //            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat pencarian data. Pesan: \"" + ex.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        catch (Exception e)
            //        {
            //            // jika terdapat gangguan lainnya
            //            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pembacaan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        finally
            //        {
            //            sdr.Close();
            //            conn.Close();
            //        }

            //        // tahap 2: tentukan ID atau nomor pengenal mutasi
            //        cmd = new SqlCommand("SM_Mutasi", conn);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "ID Baru";

            //        try
            //        {
            //            if (conn.State != ConnectionState.Open) { conn.Open(); }
            //            sdr = cmd.ExecuteReader();
            //            while (sdr.Read())
            //            {
            //                ID_Mutasi = sdr["ID_Baru"].ToString().Trim();
            //            }
            //        }
            //        catch (SqlException ex)
            //        {
            //            // jika terdapat gangguan koneksi database
            //            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat pencarian data. Pesan: \"" + ex.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        catch (Exception e)
            //        {
            //            // jika terdapat gangguan lainnya
            //            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pembacaan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        finally
            //        {
            //            sdr.Close();
            //            conn.Close();
            //        }

            //        /*
            //        if (!string.IsNullOrEmpty(ID_Mutasi))
            //        {
            //            ID_Baru = Convert.ToInt64(ID_Mutasi);
            //        }
            //        else
            //        {
            //            ID_Baru = 0;
            //        }

            //        ID_Baru++;
            //        ID_Mutasi = ID_Baru.ToString("000000000000");
            //        */

            //        // buat nomor mutasi
            //        int seed = 0;

            //        String no_mutasi = DB.TM_Mutasis.Max(x => x.ID_Mutasi);
            //        if (String.IsNullOrEmpty(no_mutasi))
            //        {
            //            seed = 1;
            //        }
            //        else
            //        {
            //            DateTime tgl = DateTime.ParseExact(no_mutasi.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
            //            if (tgl != DateTime.Now.Date)
            //            {
            //                seed = 1;
            //            }
            //            else
            //            {
            //                seed = Convert.ToInt32(no_mutasi.Substring(8, 4));
            //                seed++;
            //            }
            //            // seed = Convert.ToInt32(no_mutasi.Substring(8, 4));
            //        }

            //        ID_Mutasi = String.Format("{0:yyyyMMdd}", model.Tgl_Mutasi) + String.Format("{0:0000}", seed);

            //        switch (Jabatan_Baru)
            //        {
            //            case "President Director":
            //                Privilege = "Admin";
            //                break;
            //            case "Komisaris":
            //            case "BOD":
            //            case "Operation Manager":
            //            case "General Manager":
            //            case "Deputy Operation Manager":
            //            case "Manager":
            //            case "Sales Manager":
            //            case "Kepala Cabang":
            //            case "Kepala Bengkel":
            //                Privilege = "Manager";
            //                break;
            //            case "Supervisor":
            //            case "Supervisor Admin Area":
            //            case "Supervisor Service Area":
            //                Privilege = "Supervisor";
            //                break;
            //            default:
            //                Privilege = "Staff";
            //                break;
            //        }

            //        // tahap 3: susun stored procedure
            //        cmd = new SqlCommand("SM_Mutasi", conn);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add("@ID_Mutasi", SqlDbType.NVarChar).Value = ID_Mutasi;
            //        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
            //        cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Karyawan;
            //        cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
            //        cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
            //        cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
            //        cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
            //        cmd.Parameters.Add("@Tgl_Mutasi", SqlDbType.DateTime).Value = Tgl_Mutasi;
            //        cmd.Parameters.Add("@Privilege", SqlDbType.NVarChar).Value = Privilege_S;
            //        cmd.Parameters.Add("@Privilege_Baru", SqlDbType.NVarChar).Value = Privilege;
            //        cmd.Parameters.Add("@Atasan_Asal", SqlDbType.NVarChar).Value = Atasan_Asal;
            //        cmd.Parameters.Add("@Atasan_Ganti", SqlDbType.NVarChar).Value = Atasan_Ganti;
            //        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Manager";

            //        // periksa keberadaan input pada perusahaan, cabang, jabatan, departemen & atasan
            //        // isian berikut ini semuanya opsional
            //        if (!string.IsNullOrEmpty(model.Perusahaan_Baru))
            //        {
            //            Perusahaan_Baru = model.Perusahaan_Baru.ToString().Trim();
            //            cmd.Parameters.Add("@Perusahaan_Baru", SqlDbType.NVarChar).Value = Perusahaan_Baru;
            //        }

            //        if (!string.IsNullOrEmpty(model.Cabang_Baru))
            //        {
            //            Cabang_Baru = model.Cabang_Baru.ToString().Trim();
            //            cmd.Parameters.Add("@Cabang_Baru", SqlDbType.NVarChar).Value = Cabang_Baru;
            //        }

            //        if (!string.IsNullOrEmpty(model.Jabatan_Baru))
            //        {
            //            Jabatan_Baru = model.Jabatan_Baru.ToString().Trim();
            //            cmd.Parameters.Add("@Jabatan_Baru", SqlDbType.NVarChar).Value = Jabatan_Baru;
            //        }

            //        if (!string.IsNullOrEmpty(model.Departemen_Baru))
            //        {
            //            Departemen_Baru = model.Departemen_Baru.ToString().Trim();
            //            cmd.Parameters.Add("@Departemen_Baru", SqlDbType.NVarChar).Value = Departemen_Baru;
            //        }

            //        if (!string.IsNullOrEmpty(model.Atasan_Baru))
            //        {
            //            Atasan_Baru = model.Atasan_Baru.ToString().Trim();
            //            cmd.Parameters.Add("@Atasan_Baru", SqlDbType.NVarChar).Value = Atasan_Baru;
            //        }

            //        if (!string.IsNullOrEmpty(model.Supervisor_Baru))
            //        {
            //            Supervisor_Baru = model.Supervisor_Baru.ToString().Trim();
            //            cmd.Parameters.Add("@Supervisor_Baru", SqlDbType.NVarChar).Value = Supervisor_Baru;
            //        }

            //        if (!string.IsNullOrEmpty(model.Keterangan))
            //        {
            //            Keterangan = model.Keterangan.ToString().Trim();
            //            cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
            //        }

            //        try
            //        {
            //            if (conn.State != ConnectionState.Open) { conn.Open(); }
            //            cmd.ExecuteNonQuery();

            //            // modifikasi tanggal perubahan sebelum membuat pesan email
            //            var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == model.NIK);
            //            if (karyawan != null)
            //            {
            //                karyawan.ModifiedBy = User.Identity.Name;
            //                karyawan.ModifiedDate = DateTime.Now;
            //                DB.SaveChanges();
            //            }

            //            try
            //            {
            //                // setting SMTP server
            //                Msg = new StringBuilder();
            //                Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
            //                Msg.Append("<br />");
            //                Msg.Append("Yth. " + Nama_Karyawan + ",");
            //                Msg.Append("<br /><br />");
            //                Msg.Append("Anda telah melakukan proses mutasi karyawan dengan rincian sebagai berikut: <br />");
            //                if (!string.IsNullOrEmpty(model.Perusahaan_Baru) && Perusahaan_Baru != Perusahaan)
            //                {
            //                    Msg.Append("Perusahaan Baru: " + Perusahaan_Baru + "<br />");
            //                }
            //                else if (!string.IsNullOrEmpty(model.Cabang_Baru) && Cabang_Baru != Cabang)
            //                {
            //                    Msg.Append("Cabang Baru: " + Cabang_Baru + "<br />");
            //                }
            //                else if (!string.IsNullOrEmpty(model.Jabatan_Baru) && Jabatan_Baru != Jabatan)
            //                {
            //                    Msg.Append("Jabatan Baru: " + Jabatan_Baru + "<br />");
            //                }
            //                else if (!string.IsNullOrEmpty(model.Departemen_Baru) && Departemen_Baru != Departemen)
            //                {
            //                    Msg.Append("Departemen Baru: " + Departemen_Baru + "<br />");
            //                }
            //                else if (!string.IsNullOrEmpty(Atasan_Baru) && Atasan_Baru != Nama_Atasan)
            //                {
            //                    Msg.Append("Atasan Baru: " + Atasan_Baru + "<br />");
            //                }
            //                Msg.Append("<br /><br />");
            //                Msg.Append("Tanggal Mutasi: " + string.Format("{0:dd MMMM yyyy}", Tgl_Mutasi));
            //                Msg.Append("<br /><br />");
            //                Msg.Append("Keterangan Mutasi: " + Keterangan);
            //                Msg.Append("<br /><br />");
            //                Msg.Append("Mohon untuk segera memberitahukan isi surat ini kepada atasan anda.");
            //                Msg.Append("<br /><br />");
            //                Msg.Append("Atas perhatiannya kami mengucapkan terima kasih.");
            //                Msg.Append("<br /><br /><br /><br />");
            //                Msg.Append("Jakarta, " + string.Format("{0:dd MMMM yyyy}", DateTime.Now));
            //                Msg.Append("<br /><br />");
            //                Msg.Append("Hormat kami,");
            //                Msg.Append("<br /><br />");
            //                Msg.Append("<br /><br />");
            //                Msg.Append("<u>Human Resources Department ARISTA Group</u>");
            //                Msg.Append("</span>");

            //                // baca email mana yang valid
            //                if (!string.IsNullOrEmpty(Email_Perusahaan))
            //                {
            //                    Email_Temp = Email_Perusahaan;
            //                }
            //                else
            //                {
            //                    Email_Temp = Email;
            //                }

            //                Helpers.SendMutasi(Msg.ToString(), Email_Temp, Email_Perusahaan, Email);
            //            }
            //            catch (SmtpException smtpex)
            //            {
            //                ViewBag.Message = smtpex.Message;
            //                return RedirectToAction("MutasiError", "Notif");
            //            }
            //            catch (Exception e)
            //            {
            //                ViewBag.Message = e.Message;
            //                return RedirectToAction("MutasiError", "Notif");
            //            }

            //            return RedirectToAction("MutasiSukses", "Notif");
            //        }
            //        catch (SqlException ex)
            //        {
            //            // jika terdapat gangguan koneksi database
            //            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        catch (Exception e)
            //        {
            //            // jika terdapat gangguan lainnya
            //            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        finally
            //        {
            //            conn.Close();
            //        }
            //    }
            //}
            //else
            //{
            //    SetPrivilege();
            //    using (var conn = new SqlConnection(connstring))
            //    {
            //        cmd = new SqlCommand("SM_Karyawan", conn);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK.ToString().Trim();
            //        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select NIK";

            //        try
            //        {
            //            if (conn.State != ConnectionState.Open) { conn.Open(); }
            //            sdr = cmd.ExecuteReader();
            //            while (sdr.Read())
            //            {
            //                model.NIK = sdr["NIK"].ToString().Trim();
            //                model.Nama_Karyawan = sdr["Nama_Karyawan"].ToString().Trim();
            //                model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
            //                model.Cabang = sdr["Cabang"].ToString().Trim();
            //                model.Jabatan = sdr["Jabatan"].ToString().Trim();
            //                model.Departemen = sdr["Departemen"].ToString().Trim();
            //                model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
            //            }
            //        }
            //        catch (SqlException ex)
            //        {
            //            // jika terdapat gangguan koneksi database
            //            ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat pencarian data. Pesan: \"" + ex.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        catch (Exception e)
            //        {
            //            // jika terdapat gangguan lainnya
            //            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pembacaan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
            //            SetPrivilege();
            //            return View(model);
            //        }
            //        finally
            //        {
            //            sdr.Close();
            //            conn.Close();
            //        }
            //    }
            //    return View(model);
            //}
            #endregion

            return RedirectToAction("MutasiSukses", "Notif");
        }

        public PartialViewResult GetListBawahan(MutasiModel model, string NIK)
        {
            if (string.IsNullOrWhiteSpace(NIK))
            {
                NIK = Request.Params["NIK"].ToString();
            }

            using (var DB = new HRISContext())
            {
                model.ListBawahan = Providers.GetListBawahan(NIK).Select(x => new KaryawanModel
                {
                    NIK = x.NIK,
                    Nama_Karyawan = x.Nama_Karyawan,
                    Perusahaan = x.Perusahaan,
                    Cabang = x.Cabang,
                    Jabatan = x.Jabatan,
                    Departemen = x.Departemen,
                    NIK_Atasan = x.NIK_Atasan,
                    Nama_Atasan = x.Nama_Atasan,
                    NIK_Supervisor = x.NIK_Supervisor,
                    Nama_Supervisor = x.Nama_Supervisor
                }).ToList();
            }

            // cek status konfirmasi grid
            bool confirmed = Convert.ToBoolean(Request.Params["UserConfirmed"] ?? "false");
            model.Konfirmasi = confirmed;

            if (!confirmed)
            {
                // jika tidak berstatus terkonfirmasi, bersihkan daftar NIK yang pernah ada
                Session["ListNIK"] = null;
            }

            return PartialView("_PanelBawahanInfo", model);
        }

        public PartialViewResult GetAtasanBaru(MutasiModel model, string NIK)
        {
            if (string.IsNullOrWhiteSpace(NIK))
            {
                NIK = Request.Params["NIK"].ToString();
            }

            return PartialView("_AtasanPengganti", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetNamaAtasan(string NIK_Awal, string NIK_Baru)
        {
            var namaAwal = Providers.GetKaryawanByNIK(NIK_Awal)?.FirstOrDefault()?.Nama_Karyawan;
            var namaBaru = Providers.GetKaryawanByNIK(NIK_Baru)?.FirstOrDefault()?.Nama_Karyawan;

            var data = new
            {
                NamaAsal = namaAwal,
                NamaBaru = namaBaru
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult SetKonfirmasi(string[] args, MutasiModel model)
        {
            // pastikan bahwa list dibersihkan terlebih dahulu sebelum digunakan
            Session["ListNIK"] = null;

            var listNIK = new List<KaryawanModel>();
            
            if (args != null && args.Length > 0)
            {
                foreach (string nik in args)
                {
                    listNIK.Add(new KaryawanModel { NIK = nik });
                }
            }

            Session["ListNIK"] = listNIK;

            #if DEBUG
                var storedValues = Session["ListNIK"] as List<KaryawanModel>;
                foreach (var item in storedValues)
                {
                    Debug.WriteLine(item.NIK);
                }

            #endif

            var response = listNIK.Select(x => x.NIK).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        /*
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
        */
        #endregion

        #region Mutasi Khusus Kepala Cabang
        // update 160217: mutasi kepala cabang terdiri dari 3 jenis
        // 1. mutasi ka-cab dengan pertukaran antar cabang (kacab cab. A => kacab cab. B, kacab cab. B => kacab cab. A)
        // 2. mutasi ka-cab tanpa pertukaran antar cabang (berantai)
        // 3. mutasi ka-cab dengan resign ka-cab yang digantikan

        public ActionResult MutasiKacab()
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
                    record = Request.Cookies["LoginID"];
                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(record.Values["NIK"]))
                        {
                            NIK = record.Values["NIK"].ToString().Trim();
                        }
                    }

                    var model = new MutasiKCBModel();
                    if (Session["Mutasi"] == null)
                    {
                        model.Tipe = "Pertukaran";
                        Session["Mutasi"] = model.Tipe;
                    }
                    else
                    {
                        model.Tipe = Session["Mutasi"].ToString().Trim();
                    }

                    model.Perusahaan = DB.TM_Karyawans.Where(x => x.NIK == NIK).Select(x => x.Perusahaan).FirstOrDefault();
                    model.Tgl_Mutasi = DateTime.Now.Date;

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
        public ActionResult MutasiKacab(MutasiKCBModel model)
        {
            SetPrivilege();
            if (!String.IsNullOrEmpty(model.Perusahaan))
            {
                String[] nomor = new String[5];
                String[] cabang = new String[5];

                int i = 0, j = 0;

                // periksa setiap isian
                foreach (var items in model.ListKaryawan)
                {
                    nomor[i] = items.NIK;
                    i++;
                }

                foreach (var items in model.ListCabang)
                {
                    cabang[j] = items.Nama_Cabang;
                    j++;
                }

                if (Helpers.IsEmptyArray<String>(nomor))
                {
                    ViewData["ErrorMsg"] = "Proses mutasi kepala cabang minimal memerlukan 1 kepala cabang.";
                    return View(model);
                }

                for (int n = 0; n < 5; n++)
                {
                    String no_urut = nomor[n];
                    String cabang_urut = cabang[n];
                    String kacab = DB.TM_Karyawans.Where(x => x.NIK == no_urut).Select(x => x.Nama_Karyawan).FirstOrDefault();
                    var datakar = DB.TM_Karyawans.Where(x => x.NIK == no_urut).FirstOrDefault();

                    // ganti nama atasan yang sama dengan atasan yang baru
                    if (!String.IsNullOrEmpty(kacab))
                    {
                        try
                        {
                            var matched = DB.TM_Karyawans.Where(x => x.Cabang == cabang_urut && (x.Tgl_Resign == null || x.Status_Kerja == "Aktif")).ToList();
                            if (matched != null)
                            {
                                foreach (var items in matched)
                                {
                                    items.Nama_Atasan = kacab;
                                    DB.SaveChanges();
                                }

                                // buat nomor mutasi
                                int seed = 0;

                                String no_mutasi = DB.TM_Mutasis.Max(x => x.ID_Mutasi);
                                if (String.IsNullOrEmpty(no_mutasi))
                                {
                                    seed = 1;
                                }
                                else
                                {
                                    DateTime tgl = DateTime.ParseExact(no_mutasi.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                                    if (tgl != DateTime.Now.Date)
                                    {
                                        seed = 1;
                                    }
                                    else
                                    {
                                        seed = Convert.ToInt32(no_mutasi.Substring(8, 4));
                                        seed++;
                                    }
                                    // seed = Convert.ToInt32(no_mutasi.Substring(8, 4));
                                }

                                var mutasi = new TM_Mutasi()
                                {
                                    ID_Mutasi = String.Format("{0:yyyyMMdd}", model.Tgl_Mutasi) + String.Format("{0:0000}", seed),
                                    NIK = no_urut,
                                    Nama_Karyawan = kacab,
                                    Perusahaan = model.Perusahaan,
                                    Cabang = datakar.Cabang,
                                    Jabatan = "Kepala Cabang",
                                    Departemen = datakar.Departemen,
                                    Cabang_Baru = cabang_urut,
                                    Tgl_Mutasi = model.Tgl_Mutasi,
                                    Keterangan = "Mutasi Kepala Cabang"
                                };

                                DB.TM_Mutasis.Add(mutasi);
                                DB.SaveChanges();

                                // modifikasi tanggal perubahan sebelum membuat pesan email
                                var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == no_urut);
                                if (karyawan != null)
                                {
                                    karyawan.ModifiedBy = User.Identity.Name;
                                    karyawan.ModifiedDate = DateTime.Now;
                                    DB.SaveChanges();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Proses mutasi menghasilkan kesalahan sebagai berikut: " + e.Message;
                            return View(model);
                        }
                    }

                    if (n >= 4)
                    {
                        return RedirectToAction("MutasiSukses", "Notif");
                    }
                }
            }

            return View(model);
        }

        public PartialViewResult GetMutasiCabang()
        {
            String perusahaan = (Request.Params["Perusahaan"] ?? String.Empty).ToString();
            return PartialView("_PanelKacab", new MutasiKCBModel() { Perusahaan = perusahaan });
        }

        #endregion

        #endregion

        #region Pengaturan Resign Karyawan

        #region Pengajuan Resign Karyawan
        public ActionResult Resignation()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                var model = new ResignModel();
                if (ViewBag.Privilege == "Admin")
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
                Perusahaan = model.Perusahaan.ToString().Trim();
                Cabang = model.Cabang.ToString().Trim();
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
                    cmd.Parameters.Add("@Perusahaan", SqlDbType.NVarChar).Value = Perusahaan;
                    cmd.Parameters.Add("@Cabang", SqlDbType.NVarChar).Value = Cabang;
                    cmd.Parameters.Add("@Jabatan", SqlDbType.NVarChar).Value = Jabatan;
                    cmd.Parameters.Add("@Departemen", SqlDbType.NVarChar).Value = Departemen;
                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                    cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
                    cmd.Parameters.Add("@Tgl_Resign", SqlDbType.DateTime).Value = Tgl_Resign;
                    cmd.Parameters.Add("@Alasan", SqlDbType.NVarChar).Value = Alasan;
                    cmd.Parameters.Add("@Status_Resign", SqlDbType.NVarChar).Value = "Resigned";
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
                            Msg.Append("Mohon segera memproses persetujuan resign dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalResign\">http://" + ServerIP + "/Manager/ApprovalResign</a>.");
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
                            return RedirectToAction("ResignError", "Notif");
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = e.Message;
                            return RedirectToAction("ResignError", "Notif");
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

        public PartialViewResult GetResign(String NIK, ResignModel model)
        {
            SetPrivilege();
            // jika NIK tidak masuk melalui combo box, isi lewat model
            if (String.IsNullOrEmpty(NIK) && !String.IsNullOrEmpty(model.NIK))
            {
                NIK = model.NIK.ToString().Trim();
            }
            else if (String.IsNullOrEmpty(NIK) && String.IsNullOrEmpty(model.NIK))
            {
                NIK = String.Empty;
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
                        model.Perusahaan = sdr["Perusahaan"].ToString().Trim();
                        model.Cabang = sdr["Cabang"].ToString().Trim();
                        model.Jabatan = sdr["Jabatan"].ToString().Trim();
                        model.Departemen = sdr["Departemen"].ToString().Trim();
                        model.Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                        model.Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                    }

                    return PartialView("_PanelResignInfo", model);
                }
                catch (SqlException ex)
                {
                    ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pemuatan data karyawan. Silakan mencoba kembali. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return PartialView("_PanelResignInfo", model);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        #endregion

        #region Persetujuan Resign Karyawan
        public ActionResult ApprovalResign()
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

        public ActionResult ApprovalResignPartial()
        {
            var model = DB.TM_Resigns;
            return PartialView("_ApprovalResign", model.ToList());
        }

        public ActionResult SetujuResign(string id, ResignModel model)
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
                return View("ApprovalResign", model);
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
                    return View("ApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalResign", model);
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

                        Helpers.SendPersetujuan(Msg.ToString(), Email_Temp, Email_Atasan, Email_Perusahaan, Email);
                    }
                    catch (SmtpException smtpex)
                    {
                        ViewBag.Message = smtpex.Message;
                        return RedirectToAction("ResignError", "Notif");
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message = e.Message;
                        return RedirectToAction("ResignError", "Notif");
                    }
                }
                catch (SqlException ex)
                {
                    // jika terdapat gangguan koneksi database
                    ViewData["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalResign", model);
                }
                finally
                {
                    conn.Close();
                }
            }
            return RedirectToAction("ResignApproved", "Notif");
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
                return View("ApprovalResign", model);
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
                    return View("ApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalResign", model);
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
                    return View("ApprovalResign", model);
                }
                catch (Exception e)
                {
                    // jika terdapat gangguan lainnya
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengisian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                    SetPrivilege();
                    return View("ApprovalResign", model);
                }
                finally
                {
                    conn.Close();
                }
            }
            return RedirectToAction("ResignBatal", "Notif");
        }
        #endregion

        #endregion

        #region Pengaturan Sistem
        public ActionResult Setting()
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

        [HttpPost]
        public ActionResult Setting(SettingModel model)
        {
            string ApplyTheme;
            if (!string.IsNullOrEmpty(model.ThemeName))
            {
                // konversi tema terlebih dahulu
                ApplyTheme = model.ThemeName.ToString().Trim();
                switch (ApplyTheme)
                {
                    case "ARISTA Default":
                        model.ThemeName = "Aqua";
                        break;
                    case "ARISTA Red":
                        model.ThemeName = "RedWine";
                        break;
                    case "ARISTA Lime":
                        model.ThemeName = "SoftOrange";
                        break;
                    case "ARISTA Light":
                        model.ThemeName = "Glass";
                        break;
                    case "ARISTA Standard":
                        model.ThemeName = "DevEx";
                        break;
                }

                // pemilihan tema tampilan
                record = new HttpCookie("theme");
                record.Value = model.ThemeName.ToString().Trim();

                // konversi ulang ke format yang dapat dibaca user
                ApplyTheme = model.ThemeName.ToString().Trim();

                switch (ApplyTheme)
                {
                    case "Aqua":
                        model.ThemeName = "ARISTA Default";
                        break;
                    case "RedWine":
                        model.ThemeName = "ARISTA Red";
                        break;
                    case "SoftOrange":
                        model.ThemeName = "ARISTA Lime";
                        break;
                    case "Glass":
                        model.ThemeName = "ARISTA Light";
                        break;
                    case "DevEx":
                        model.ThemeName = "ARISTA Standard";
                        break;
                    default:
                        model.ThemeName = "ARISTA Default";
                        break;
                }

                ViewData["Success"] = "Pengaturan berhasil disimpan.";
                SetPrivilege();
                return View(model);
            }
            return View(model);
        }
        #endregion

        #region Email Counter
        public ActionResult EmailCounter()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                var model = new ProgCounter();
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_Email_Counter", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();

                        while (sdr.Read())
                        {
                            Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                            Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        }

                        model.TotalCount = Total_Count;
                        model.DailyCount = Counter;
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan dalam memperoleh data dari database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam memproses data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View(model);
                    }
                    finally
                    {
                        if (!sdr.IsClosed) sdr.Close();
                        conn.Close();
                    }
                }

                return View(model);
            }
        }
        #endregion

        #region Reset Master Password
        public ActionResult ResetMaster()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("SessionLogin", "Home");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetMaster(PasswordModel model)
        {
            SetPrivilege();
            if (!String.IsNullOrEmpty(model.NIK))
            {
                Password = FormsAuthentication.HashPasswordForStoringInConfigFile(model.NIK, "SHA1");

                using (var DB = new HRISContext())
                {
                    var akun = DB.TM_Karyawans.FirstOrDefault(x => x.NIK.Equals(model.NIK));

                    if (akun != null)
                    {
                        akun.Password = Password;
                        DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["ErrorMsg"] = "NIK yang anda masukkan tidak valid. Periksa kembali isian NIK anda.";
                    }
                }

                return RedirectToAction("EditSukses", "Notif");
            }
            return View(model);
        }
        #endregion

        #region Pengumuman User
        public ActionResult Pengumuman()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                Session["Anno"] = "New";
                // periksa privilege user
                if (ViewBag.Privilege == "Admin")
                {
                    // tanggal pengumuman default hari ini
                    var model = new AnnoModel();
                    model.Tgl_Anno = DateTime.Now.Date;

                    // sebelum memunculkan view page, lakukan data flushing
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Announcement", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Flush";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pembacaan data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View("Pengumuman", model);
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pemeriksaan data. Pesan: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View("Pengumuman", model);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }

                    return View("Pengumuman", model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        public ActionResult Pengumuman(AnnoModel model)
        {
            if (ModelState.IsValid && Session["Anno"].ToString() == "New")
            {
                Jenis_Anno = model.Jenis_Anno.ToString().Trim();
                Tgl_Pengajuan = (DateTime)model.Tgl_Anno;
                Keterangan = model.Keterangan.ToString().Trim();

                // pemeriksaan untuk dua pengumuman atau lebih pada hari yang sama
                try
                {
                    var DataAnno = DB.TM_Pengumumans.Where(t => t.Tgl_Anno.Value.Day == Tgl_Pengajuan.Day && t.Tgl_Anno.Value.Month == Tgl_Pengajuan.Month && t.Tgl_Anno.Value.Year == Tgl_Pengajuan.Year).ToArray();

                    if (DataAnno.Length >= 1)
                    {
                        ViewData["ErrorMsg"] = "Sistem mendeteksi bahwa terdapat pengumuman lainnya pada tanggal yang anda pilih. Silakan tentukan tanggal lainnya yang berbeda.";
                        SetPrivilege();
                        return View("Pengumuman", model);
                    }
                }
                catch (Exception e)
                {
                    LastData = "";
                    ViewBag.Message = e.Message;
                }

                // pencarian ID pengumuman terbaru
                try
                {
                    LastData = (from Anno in DB.TM_Pengumumans
                                select Anno.ID_Daftar).Max().ToString().Trim();
                }
                catch (Exception e)
                {
                    LastData = "";
                    ViewBag.Message = e.Message;
                }

                if (!string.IsNullOrEmpty(LastData))
                {
                    ID_Baru = Convert.ToInt64(LastData);
                    ID_Baru++;
                    ID_Pesan = ID_Baru.ToString("00000000");
                }
                else
                {
                    ID_Pesan = "00000001";
                }

                // masukkan ke database
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_Announcement", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // ID diisi secara otomatis
                    cmd.Parameters.Add("@ID_Daftar", SqlDbType.NVarChar).Value = ID_Pesan;
                    cmd.Parameters.Add("@Jenis_Anno", SqlDbType.NVarChar).Value = Jenis_Anno;
                    cmd.Parameters.Add("@Tgl_Anno", SqlDbType.DateTime).Value = Tgl_Pengajuan;
                    cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("AddSukses", "Notif");
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pengisian data ke database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("Pengumuman", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pemuatan data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("Pengumuman", model);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else if (ModelState.IsValid && Session["Anno"].ToString() == "Edit")
            {
                Jenis_Anno = model.Jenis_Anno.ToString().Trim();
                Tgl_Pengajuan = (DateTime)model.Tgl_Anno;

                // menghindari injeksi XSS
                Keterangan = model.Keterangan.ToString().Trim();

                // pemeriksaan untuk dua pengumuman atau lebih pada hari yang sama
                try
                {
                    var DataAnno = DB.TM_Pengumumans.Where(t => t.Tgl_Anno.Value.Day == Tgl_Pengajuan.Day && t.Tgl_Anno.Value.Month == Tgl_Pengajuan.Month && t.Tgl_Anno.Value.Year == Tgl_Pengajuan.Year).ToArray();

                    if (DataAnno.Length <= 0)
                    {
                        ViewData["ErrorMsg"] = "Pengumuman yang akan diedit pada tanggal yang ditentukan tidak tersedia.";
                        Session["Anno"] = "New";
                        SetPrivilege();
                        return View("Pengumuman", model);
                    }
                }
                catch (Exception e)
                {
                    ID_Pesan = "";
                    ViewData["ErrorMsg"] = "Terjadi masalah dalam pemuatan data pengumuman. Silakan mencoba lagi. Pesan: \"" + e.Message + "\"";
                    Session["Anno"] = "New";
                    return View("Pengumuman", model);
                }

                // pencarian ID pengumuman terbaru
                try
                {
                    ID_Pesan = (from Anno in DB.TM_Pengumumans
                                where Anno.Tgl_Anno.Value.Day == DateTime.Now.Day && Anno.Tgl_Anno.Value.Month == DateTime.Now.Month && Anno.Tgl_Anno.Value.Year == DateTime.Now.Year
                                select Anno.ID_Daftar).Max().ToString().Trim();
                }
                catch (Exception e)
                {
                    ID_Pesan = "";
                    ViewData["ErrorMsg"] = "Terjadi masalah dalam pemuatan data pengumuman. Silakan mencoba lagi. Pesan: \"" + e.Message + "\"";
                    Session["Anno"] = "New";
                    return View("Pengumuman", model);
                }

                // masukkan ke database
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_Announcement", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // ID diisi secara otomatis
                    cmd.Parameters.Add("@ID_Daftar", SqlDbType.NVarChar).Value = ID_Pesan;
                    cmd.Parameters.Add("@Jenis_Anno", SqlDbType.NVarChar).Value = Jenis_Anno;
                    cmd.Parameters.Add("@Tgl_Anno", SqlDbType.DateTime).Value = Tgl_Pengajuan;
                    cmd.Parameters.Add("@Keterangan", SqlDbType.NVarChar).Value = Keterangan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        return RedirectToAction("EditSukses", "Notif");
                    }
                    catch (SqlException ex)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pengubahan data dalam database. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return View("Pengumuman", model);
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengubahan data. Pesan: \"" + e.Message + "\"";
                        SetPrivilege();
                        return View("Pengumuman", model);
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
                return View("Pengumuman", model);
            }
        }

        public ActionResult EditPengumuman()
        {
            // query dari database
            SetPrivilege();
            Session["Anno"] = "Edit";

            var model = new AnnoModel();

            // pencarian pada database
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_Announcement", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                try
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    sdr = cmd.ExecuteReader();
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            Jenis_Anno = sdr["Jenis_Anno"].ToString().Trim();
                            Tgl_Pengajuan = Convert.ToDateTime(sdr["Tgl_Anno"].ToString());
                            Keterangan = sdr["Keterangan"].ToString().Trim();
                        }
                    }
                    else
                    {
                        ViewData["ErrorMsg"] = "Data pengumuman untuk tanggal hari ini belum tersedia. Silakan membuat pengumuman terlebih dahulu.";
                        model.Tgl_Anno = DateTime.Now.Date;
                        Session["Anno"] = "New";
                        SetPrivilege();
                        return View("Pengumuman", model);
                    }
                }
                catch (SqlException ex)
                {
                    ViewData["ErrorMsg"] = "Terjadi masalah dalam proses pembacaan data. Pesan: \"" + ex.Message + "\"";
                    Session["Anno"] = "New";
                    SetPrivilege();
                    return View("Pengumuman", model);
                }
                catch (Exception e)
                {
                    ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pemeriksaan data. Pesan: \"" + e.Message + "\"";
                    Session["Anno"] = "New";
                    SetPrivilege();
                    return View("Pengumuman", model);
                }
                finally
                {
                    conn.Close();
                }

                model.Jenis_Anno = Jenis_Anno;
                model.Tgl_Anno = Tgl_Pengajuan;
                model.Keterangan = Keterangan;

                return View("Pengumuman", model);
            }
        }
        #endregion

        // Bagian khusus pengajuan mutasi karyawan (PMK)
        #region Pencatatan PMK
        public ActionResult CreateMutasi()
        {
            var model = new PMKModel();

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();

                // buat nilai default
                model.Tgl_PMK = DateTime.Now;
                model.Tgl_Efektif = DateTime.Now;



                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMutasi(PMKModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // buat nomor PMK sesuai jenisnya



                    model.Created_By = User.Identity.Name;
                    model.Created_Date = DateTime.Now;
                    model.Modified_By = User.Identity.Name;
                    model.Modified_Date = DateTime.Now;

                    // buat sebuah objek tabel
                    var pmk = new TM_List_PMK()
                    {
                        No_PMK = model.No_PMK,
                        Tgl_PMK = model.Tgl_PMK,
                        Jenis_PMK = model.Jenis_PMK,
                        Tgl_Efektif = model.Tgl_Efektif,
                        Alasan = model.Alasan,
                        Notes = model.Notes,
                        Created_By = model.Created_By,
                        Created_Date = model.Created_Date,
                        Modified_By = model.Modified_By,
                        Modified_Date = model.Modified_Date
                    };

                    // bagian pengisian kolom-kolom yang memiliki kondisi tertentu


                    // lakukan insert data setelah memastikan mapping nilai
                    DB.TM_List_PMKs.Add(pmk);
                    DB.SaveChanges();

                    // redirect sesudah proses insert berhasil

                }
                catch (SqlException ex)
                {
                    ViewData["ErrorMsg"] = "";
                    return View(model);
                }
                catch (Exception e)
                {
                    ViewData["ErrorMsg"] = "";
                    return View(model);
                }
            }

            return View(model);
        }
        #endregion

        /*
         *  Bagian khusus untuk redirect ke area pengajuan dan laporan SPPD (perjalanan dinas)
         *  Nama metode action di sini bergantung dari nama metode action di area target
         */

        #region Formulir SPPD (Hanya Redirect)
        public ActionResult FormPengajuanSPD()
        {
            return RedirectToAction("PengajuanSPD", "Home", new { area = "SPPD" });
        }

        public ActionResult FormLaporanSPD()
        {
            return RedirectToAction("LaporanSPD", "Home", new { area = "SPPD" });
        }
        #endregion

        /*
         *  Bagian form selain SPPD
         *  - Under Construction -
         */

        #region Form Lainnya
        public ActionResult FormIzin()
        {
            // setting izin
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormDuka()
        {
            // setting untuk form sumbangan dukacita (kematian)
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormLahir()
        {
            // setting untuk form sumbangan kelahiran anak
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormNikah()
        {
            // setting untuk form sumbangan pernikahan ybs
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormInsentif()
        {
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormUMTL()
        {
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormLemburDrv()
        {
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormUbahData()
        {
            return RedirectToAction("UnderConstruction", "Notif");
        }

        public ActionResult FormPermintaanTK()
        {
            return RedirectToAction("UnderConstruction", "Notif");
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

        public bool HasValues(string source, string word)
        {
            return source.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion
	}
}