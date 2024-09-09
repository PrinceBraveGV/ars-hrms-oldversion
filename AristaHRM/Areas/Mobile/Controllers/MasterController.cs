using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using System.Threading;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using DevExpress.XtraPrinting;
using AristaHRM.Models;

namespace AristaHRM.Areas.Mobile.Controllers
{
    [RouteArea("Mobile")]
    [RoutePrefix("Master")]
    public class MasterController : Controller
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
        public string Nama_Perusahaan, Nama_Cabang, Jabatan, Departemen;
        public string Nama_Atasan, Nama_Supervisor;
        public string Privilege, Privilege_At;
        public string Parameter;
        public string ID_Cuti, Jenis_Cuti;
        public string Email_HRD1, Email_HRD2, Email_HRD3;
        public string Email, Email_Atasan, Email_Supervisor;
        public string Keperluan;
        public string ID_Daftar, Nama_Event, Jenis_Event;
        public string ServerIP = Providers.GetServerIP();

        /* Variabel angka */
        public double Selisih_Awal, Selisih_Akhir;
        public int Masa_Cuti;
        public int Total_CM;
        public int Counter, Total_Count;

        /* Variabel tanggal */
        public DateTime Tgl_Masuk, Tgl_Resign;
        public DateTime Tgl_Awal, Tgl_Akhir;
        public DateTime Tgl_Cuti_Massal;

        #endregion

        /* Variabel objek */
        HttpCookie record;
        GridViewSettings settings; // setting output dari grid view
        public StringBuilder Msg;
        public MailMessage EmailRequest;
        public SmtpClient SMTPServer;

        #endregion

        /* BEGIN */

        #region Menu Master Data
        public ActionResult MasterData()
        {
            return View("MobileMasterData");
        }
        #endregion

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
                if (ViewBag.Privilege == "Admin")
                {
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
                var modeltemp = items;
                if (modelitem != null)
                {
                    modelitem.ModifiedBy = User.Identity.Name;
                    modelitem.ModifiedDate = DateTime.Now;
                    this.UpdateModel(modelitem);

                    DB.SaveChanges();

                    ViewData["EditSuccess"] = "Data karyawan '" + items.Nama_Karyawan + "' sudah di-update";
                }
                // kasus khusus jika NIK yang diganti, tetapi nama karyawan tetap (hasil pencarian NIK yang diubah = null)
                else if (modelname != null)
                {
                    // mengingat bahwa yang diganti adalah primary key, hapus data lama terlebih dahulu
                    model.Remove(modelname);
                    DB.SaveChanges();

                    // selanjutnya, dengan NIK yang berbeda, tambahkan karyawan ybs dalam daftar 
                    model.Add(modeltemp);
                    DB.SaveChanges();
                    ViewData["EditSuccess"] = "Data karyawan '" + items.Nama_Karyawan + "' sudah di-update";
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
            }
            return PartialView("_MobileKaryawan", model.ToList());
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
                        using (var conn = new SqlConnection(connstring))
                        {
                            // cek apakah NIK tsb adalah admin
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
            return PartialView("_MobileKaryawan", model.ToList());
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
                if (ViewBag.Privilege == "Admin")
                {
                    return View("MobilePerusahaan");
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
            return PartialView("_MobilePerusahaan", model.ToList());
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
            return PartialView("_MobilePerusahaan", model.ToList());
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
            return PartialView("_MobilePerusahaan", model.ToList());
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
                if (ViewBag.Privilege == "Admin")
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
                                return View("MobileCabang");
                            }
                            catch (Exception e)
                            {
                                // jika terdapat gangguan lainnya
                                ViewData["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                                SetPrivilege();
                                return View("MobileCabang");
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }

                    return View("MobileCabang");
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
            return PartialView("_MobileCabangGrid", model.ToList());
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
            return PartialView("_MobileCabangGrid", model.ToList());
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
            return PartialView("_MobileCabangGrid", model.ToList());
        }
        #endregion

        #region Data Riwayat Cuti
        public ActionResult Riwayat(CutiModel model, string id)
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

                    if (Session["ErrorMsg"] != null)
                    {
                        ViewData["ErrorMsg"] = Session["ErrorMsg"];
                    }

                    if (Session["Success"] != null)
                    {
                        ViewData["Success"] = Session["Success"];
                    }

                    return View("MobileRiwayat", model);
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

            if (User.Identity.Name != "SISTEM")
            {
                if (User.Identity.Name != "ADMIN")
                {
                    var model = (from Riwayat in DB.TM_Riwayats
                                 where (Riwayat.Nama_Karyawan == User.Identity.Name || Riwayat.Atasan == User.Identity.Name)
                                 select Riwayat);

                    return PartialView("_MobileRiwayat", model.ToList());
                }
                else
                {
                    var model = DB.TM_Riwayats;
                    return PartialView("_MobileRiwayat", model.ToList());
                }
            }
            else
            {
                var model = DB.TM_Riwayats;
                return PartialView("_MobileRiwayat", model.ToList());
            }
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
                    ViewData["EditSuccess"] = "Data riwayat cuti nomor '" + items.ID_Cuti + "' a. n. " + items.Nama_Karyawan + " sudah di-update";
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
            }
            return PartialView("_MobileRiwayat", model.ToList());
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
                        ViewData["EditSuccess"] = "Data riwayat cuti terpilih #" + ID_Cuti + " sudah dihapus";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = "Terjadi kesalahan saat proses sebagai berikut: \"" + e.Message + "\"";
                }
            }
            return PartialView("_MobileRiwayat", model.ToList());
        }

        public ActionResult CheckForm(FindModel model)
        {
            if (Session["EditSuccess"] != null)
            {
                ViewData["EditSuccess"] = Session["EditSuccess"].ToString();
                Session["EditSuccess"] = null;
                return View("MobileCheckForm", model);
            }
            return View("MobileCheckForm");
        }

        [HttpPost]
        public ActionResult CheckForm(FindModel model, string url)
        {
            if (!string.IsNullOrEmpty(model.Nama_Karyawan))
            {
                string Atasan = model.Nama_Karyawan.ToString().Trim();
                CheckApproval(Atasan);
                Thread.Sleep(5000);
                Session["EditSuccess"] = @"Proses pemeriksaan selesai. Email konfirmasi telah dikirim kepada atasan bersangkutan.";
                // <script type='text/javascript'>alert('" + script + "');');</script>
                return RedirectToAction("CheckForm", "Master");
            }
            else
            {
                ViewData["EditError"] = "Nama atasan yang diperiksa tidak boleh kosong.";
                return View("MobileCheckForm");
            }
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
                if (ViewBag.Privilege == "Admin")
                {
                    var model = new FindModel();
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
                                ViewData["Catatan"] = "Sisa cuti pada tabel belum termasuk potongan cuti massal tahun " + DateTime.Now.Year.ToString() + " sebanyak " + Total_CM.ToString() + " hari sebelum periode berjalan selesai.";
                            }
                            else
                            {
                                ViewData["Catatan"] = "Sisa cuti pada tabel sudah termasuk potongan cuti massal tahun " + DateTime.Now.Year.ToString() + " sebelum periode berjalan selesai.";
                            }
                        }
                        return View("MobileSisaTahunan", model);
                    }
                    else
                    {
                        SetPrivilege();
                        return View("MobileSisaTahunan", model);
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
            return PartialView("_MobileSisaTahunan", model.ToList());
        }

        // bagian isian untuk karyawan tertentu
        [HttpPost]
        public ActionResult SisaTahunan(FindModel model)
        {
            if (!string.IsNullOrEmpty(model.NIK))
            {
                NIK = model.NIK.ToString().Trim();
                Session["NIK"] = NIK;
                ViewData["NIK"] = NIK;

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

                        // bugfix 150920: sistem membedakan 2 nama karyawan yang sama berdasarkan privilege
                        // hati-hati: untuk nama "Sri Maryati" cek apakah privilegenya staff atau supervisor
                        if (Nama_Karyawan == "Sri Maryati" && Privilege == "Staff")
                        {
                            NIK = "00100";
                        }
                        else if (Nama_Karyawan == "Sri Maryati" && Privilege == "Supervisor")
                        {
                            NIK = "00095";
                        }

                        // reserved untuk nama karyawan lain yang namanya sama namun berbeda privilege
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
                }

                SetPrivilege();
                return View("MobileSisaTahunan", model);
            }
            else
            {
                return View("MobileSisaTahunan", model);
            }
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
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    return View("MobileMutasi");
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
            return PartialView("_MobileDataMutasi", model.ToList());
        }

        #endregion

        /*
         *  Bagian ekspor data grid
         *  Ekspor dilakukan dengan mengambil layout grid view
         *  Data dimasukkan sesuai dengan format yang dipilih
         */

        #region Definisi Grid
        /* BEGIN DEF GRID VIEW DATA */

        #region Data Karyawan
        public GridViewSettings GetDataKaryawanSettings()
        {
            // definisi tabel data karyawan
            settings = new GridViewSettings();
            settings.Name = "EksporKaryawan";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "DataKaryawanPartial" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Laporan Data Karyawan";
            settings.SettingsExport.PageHeader.Font.Name = "Calibri";
            settings.SettingsExport.PageHeader.Font.Size = 14;
            settings.SettingsExport.PageHeader.Font.Bold = true;
            settings.SettingsExport.FileName = "ReportKaryawan";
            settings.SettingsExport.Styles.Default.Font.Name = "Calibri";
            settings.SettingsExport.Styles.Default.Font.Size = 11;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.RenderBrick = (s, e) =>
            {
                if (e.RowType != GridViewRowType.Data) return;
                if (e.RowType == GridViewRowType.Data)
                {
                    if (e.GetValue("Status_Kerja").ToString().Trim().Contains("Tidak"))
                        e.BrickStyle.BackColor = System.Drawing.Color.Red;
                    else
                        e.BrickStyle.BackColor = System.Drawing.Color.White;
                }
            };
            settings.KeyFieldName = "NIK";
            settings.Columns.Add("NIK");
            settings.Columns.Add("Nama_Karyawan");
            settings.Columns.Add("Password");
            settings.Columns.Add("Jenis_Kelamin");
            settings.Columns.Add("Tempat_Lahir");
            settings.Columns.Add("Tgl_Lahir").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Provinsi");
            settings.Columns.Add("Kota");
            settings.Columns.Add("Alamat");
            settings.Columns.Add("Agama");
            settings.Columns.Add("Status_Nikah");
            settings.Columns.Add("Email");
            settings.Columns.Add("Email_Perusahaan");
            settings.Columns.Add("Perusahaan");
            settings.Columns.Add("Cabang");
            settings.Columns.Add("Jabatan");
            settings.Columns.Add("Departemen");
            settings.Columns.Add("Nama_Atasan");
            settings.Columns.Add("Nama_Supervisor");
            settings.Columns.Add("Tgl_Masuk").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Tgl_Resign").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Status_Karyawan");
            settings.Columns.Add("Status_Kerja");
            settings.Columns.Add("Email_Valid");
            settings.Columns.Add("Pembuat");
            settings.Columns.Add("Privilege");
            settings.Columns.Add("Last_Login").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Last_Session").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Aktif_Login");
            return settings;
        }
        #endregion

        #region Data Perusahaan
        public GridViewSettings GetDataPerusahaanSettings()
        {
            settings = new GridViewSettings();
            settings.Name = "EksporDataPerusahaan";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "PerusahaanPartial" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Data Perusahaan";
            settings.SettingsExport.FileName = "ReportPerusahaan";
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.KeyFieldName = "Kode_Perusahaan";
            settings.Columns.Add("Kode_Perusahaan");
            settings.Columns.Add("Nama_Perusahaan");
            settings.Columns.Add("Kode_Singkat");
            return settings;
        }
        #endregion

        #region Data Cabang
        public GridViewSettings GetDataCabangSettings()
        {
            settings = new GridViewSettings();
            settings.Name = "EksporDataCabang";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "DataCabangPartial" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Data Cabang";
            settings.SettingsExport.FileName = "ReportCabang";
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.KeyFieldName = "Kode_Cabang";
            settings.Columns.Add("Kode_Cabang");
            settings.Columns.Add("Kode_Singkat");
            settings.Columns.Add("Nama_Perusahaan");
            settings.Columns.Add("Nama_Cabang");
            settings.Columns.Add("Lokasi");
            settings.Columns.Add("Status_Aktif");
            return settings;
        }
        #endregion

        #region Data Riwayat Cuti
        public GridViewSettings GetDataRiwayatSettings()
        {
            settings = new GridViewSettings();
            settings.Name = "EksporDataRiwayat";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "RiwayatPartial" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Laporan Riwayat Cuti Karyawan";
            settings.SettingsExport.PageHeader.Font.Name = "Calibri";
            settings.SettingsExport.PageHeader.Font.Size = 14;
            settings.SettingsExport.PageHeader.Font.Bold = true;
            settings.SettingsExport.FileName = "ReportRiwayatCuti";
            settings.SettingsExport.Styles.Default.Font.Name = "Calibri";
            settings.SettingsExport.Styles.Default.Font.Size = 11;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.RenderBrick = (s, e) =>
            {
                if (e.RowType != GridViewRowType.Data) return;
                if (e.RowType == GridViewRowType.Data)
                {
                    if (e.GetValue("Status_Cuti").ToString().Trim().Equals("Ditolak") || e.GetValue("Status_Cuti").ToString().Trim().Equals("Dibatalkan"))
                        e.BrickStyle.BackColor = System.Drawing.Color.Red;
                    else if (e.GetValue("Status_Cuti").ToString().Trim().Equals("Tunggu"))
                        e.BrickStyle.BackColor = System.Drawing.Color.Yellow;
                    else
                        e.BrickStyle.BackColor = System.Drawing.Color.White;
                }
            };
            settings.KeyFieldName = "ID_Cuti";
            settings.Columns.Add("ID_Cuti");
            settings.Columns.Add("NIK");
            settings.Columns.Add("Nama_Karyawan");
            settings.Columns.Add("Jenis_Cuti");
            settings.Columns.Add("Masa_Cuti");
            settings.Columns.Add("Tgl_Mulai").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Tgl_Selesai").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Tgl_Pengajuan").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Tgl_Setuju").PropertiesEdit.DisplayFormatString = "dd-MM-yyyy";
            settings.Columns.Add("Keperluan");
            settings.Columns.Add("Pemberi");
            settings.Columns.Add("Status_Cuti");
            settings.Columns.Add("Keterangan");
            settings.Columns.Add("Tahun_Cuti");
            settings.Columns.Add("Alasan");
            settings.Columns.Add("Batal");
            settings.Columns.Add("Alasan_Batal");
            return settings;
        }
        #endregion

        #region Data Sisa Cuti Tahunan
        public GridViewSettings GetSisaTahunanSettings()
        {
            settings = new GridViewSettings();
            settings.Name = "EksporDataSCT";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "SisaTahunanPartial" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Laporan Sisa Cuti Tahunan";
            settings.SettingsExport.FileName = "ReportSisaCuti";
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.KeyFieldName = "ID_Cuti";
            settings.Columns.Add("ID_Cuti").Visible = false;
            settings.Columns.Add("NIK");
            settings.Columns.Add("Nama_Karyawan", "Nama Karyawan");
            settings.Columns.Add("Periode_Awal", "Awal Periode").PropertiesEdit.DisplayFormatString = "dd MMMM yyyy";
            settings.Columns.Add("Periode_Akhir", "Akhir Periode").PropertiesEdit.DisplayFormatString = "dd MMMM yyyy";
            settings.Columns.Add("Jatah_Cuti", "Jatah Cuti");
            settings.Columns.Add("Total_Cuti", "Total Cuti");
            settings.Columns.Add("Cuti_Pribadi", "Cuti Pribadi");
            settings.Columns.Add("Cuti_Khusus", "Cuti Khusus");
            settings.Columns.Add("Cuti_Massal", "Cuti Massal");
            settings.Columns.Add("Cuti_Hangus", "Cuti Hangus");
            settings.Columns.Add("Tahun_Hangus", "Tahun Hangus");
            settings.Columns.Add("Sisa_Cuti", "Sisa Cuti");

            return settings;
        }
        #endregion

        #region Data Cuti Massal
        public GridViewSettings GetDataCMSettings()
        {
            settings = new GridViewSettings();
            settings.Name = "EksporDataCM";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "CutiMassalPusat" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Data Cuti Massal";
            settings.SettingsExport.FileName = "ReportCutiMassal";
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.KeyFieldName = "ID_Daftar";

            return settings;
        }
        #endregion

        #region Data Hari Libur
        public GridViewSettings GetDataHLSettings()
        {
            settings = new GridViewSettings();
            settings.Name = "EksporDataHL";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "Kalender" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Data Hari Libur Nasional";
            settings.SettingsExport.FileName = "ReportHariLibur";
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.KeyFieldName = "ID_Daftar";

            return settings;
        }
        #endregion

        #region Data Riwayat Mutasi
        public GridViewSettings GetDataMutasiSettings()
        {
            settings = new GridViewSettings();
            settings.Name = "EksporDataMutasi";
            settings.CallbackRouteValues = new { Controller = "Master", Action = "DataMutasiPartial" };
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.ExportSelectedRowsOnly = false;
            settings.SettingsExport.PageHeader.Center = "Laporan Mutasi Karyawan";
            settings.SettingsExport.FileName = "ReportMutasi";
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.SettingsExport.Landscape = true;
            settings.KeyFieldName = "ID_Mutasi";
            settings.Columns.Add("ID_Mutasi");
            settings.Columns.Add("NIK");
            settings.Columns.Add("Nama_Karyawan");
            settings.Columns.Add("Perusahaan");
            settings.Columns.Add("Cabang");
            settings.Columns.Add("Jabatan");
            settings.Columns.Add("Departemen");
            settings.Columns.Add("Perusahaan_Baru");
            settings.Columns.Add("Cabang_Baru");
            settings.Columns.Add("Jabatan_Baru");
            settings.Columns.Add("Departemen_Baru");
            settings.Columns.Add("Atasan_Baru");
            settings.Columns.Add("Supervisor_Baru");
            settings.Columns.Add("Atasan_Ganti");
            settings.Columns.Add("Tgl_Mutasi");
            settings.Columns.Add("Keterangan");

            return settings;
        }

        #endregion

        /* END DEF GRID VIEW DATA */
        #endregion

        #region Ekspor Data Grid
        /* BEGIN EXPT GRID VIEW */

        #region Ekspor Grid Karyawan
        public ActionResult EksporKaryawanPDF()
        {
            var model = DB.TM_Karyawans.ToList();
            SetPrivilege();

            if (ViewBag.Privilege == "Manager")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Atasan == User.Identity.Name
                         select Karyawan).ToList();
            }
            else if (ViewBag.Privilege == "Supervisor")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Supervisor == User.Identity.Name
                         select Karyawan).ToList();
            }

            return GridViewExtension.ExportToPdf(GetDataKaryawanSettings(), model);
        }

        public ActionResult EksporKaryawanXLS()
        {
            var model = DB.TM_Karyawans.ToList();
            SetPrivilege();

            if (ViewBag.Privilege == "Manager")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Atasan == User.Identity.Name
                         select Karyawan).ToList();
            }
            else if (ViewBag.Privilege == "Supervisor")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Supervisor == User.Identity.Name
                         select Karyawan).ToList();
            }

            var exportopt = new XlsExportOptionsEx();
            exportopt.ExportType = DevExpress.Export.ExportType.WYSIWYG;
            // exportopt.CustomizeCell += new DevExpress.Export.CustomizeCellEventHandler(CustomizeCellColor);
            return GridViewExtension.ExportToXls(GetDataKaryawanSettings(), model, true, exportopt);
        }

        public ActionResult EksporKaryawanXLSX()
        {
            var model = DB.TM_Karyawans.ToList();
            SetPrivilege();

            if (ViewBag.Privilege == "Manager")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Atasan == User.Identity.Name
                         select Karyawan).ToList();
            }
            else if (ViewBag.Privilege == "Supervisor")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Supervisor == User.Identity.Name
                         select Karyawan).ToList();
            }

            var exportopt = new XlsxExportOptionsEx();
            exportopt.ExportType = DevExpress.Export.ExportType.WYSIWYG;
            // exportopt.CustomizeCell += new DevExpress.Export.CustomizeCellEventHandler(CustomizeCellColor);
            return GridViewExtension.ExportToXlsx(GetDataKaryawanSettings(), model, true, exportopt);
        }

        public ActionResult EksporKaryawanRTF()
        {
            var model = DB.TM_Karyawans.ToList();
            SetPrivilege();

            if (ViewBag.Privilege == "Manager")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Atasan == User.Identity.Name
                         select Karyawan).ToList();
            }
            else if (ViewBag.Privilege == "Supervisor")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Supervisor == User.Identity.Name
                         select Karyawan).ToList();
            }
            return GridViewExtension.ExportToRtf(GetDataKaryawanSettings(), model);
        }

        public ActionResult EksporKaryawanCSV()
        {
            var model = DB.TM_Karyawans.ToList();
            SetPrivilege();

            if (ViewBag.Privilege == "Manager")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Atasan == User.Identity.Name
                         select Karyawan).ToList();
            }
            else if (ViewBag.Privilege == "Supervisor")
            {
                model = null;
                model = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Nama_Supervisor == User.Identity.Name
                         select Karyawan).ToList();
            }
            return GridViewExtension.ExportToCsv(GetDataKaryawanSettings(), model);
        }
        #endregion

        #region Ekspor Grid Perusahaan
        public ActionResult EksporPerusahaanPDF()
        {
            var model = DB.TM_Perusahaans;
            return GridViewExtension.ExportToPdf(GetDataPerusahaanSettings(), model.ToList());
        }

        public ActionResult EksporPerusahaanXLS()
        {
            var model = DB.TM_Perusahaans;
            return GridViewExtension.ExportToXls(GetDataPerusahaanSettings(), model.ToList());
        }

        public ActionResult EksporPerusahaanXLSX()
        {
            var model = DB.TM_Perusahaans;
            return GridViewExtension.ExportToXlsx(GetDataPerusahaanSettings(), model.ToList());
        }

        public ActionResult EksporPerusahaanRTF()
        {
            var model = DB.TM_Perusahaans;
            return GridViewExtension.ExportToRtf(GetDataPerusahaanSettings(), model.ToList());
        }

        public ActionResult EksporPerusahaanCSV()
        {
            var model = DB.TM_Perusahaans;
            return GridViewExtension.ExportToCsv(GetDataPerusahaanSettings(), model.ToList());
        }
        #endregion

        #region Ekspor Grid Cabang
        public ActionResult EksporCabangPDF()
        {
            var model = DB.TM_Cabangs;
            return GridViewExtension.ExportToPdf(GetDataCabangSettings(), model.ToList());
        }

        public ActionResult EksporCabangXLS()
        {
            var model = DB.TM_Cabangs;
            return GridViewExtension.ExportToXls(GetDataCabangSettings(), model.ToList());
        }

        public ActionResult EksporCabangXLSX()
        {
            var model = DB.TM_Cabangs;
            return GridViewExtension.ExportToXlsx(GetDataCabangSettings(), model.ToList());
        }

        public ActionResult EksporCabangRTF()
        {
            var model = DB.TM_Cabangs;
            return GridViewExtension.ExportToXlsx(GetDataCabangSettings(), model.ToList());
        }

        public ActionResult EksporCabangCSV()
        {
            var model = DB.TM_Cabangs;
            return GridViewExtension.ExportToCsv(GetDataCabangSettings(), model.ToList());
        }
        #endregion

        #region Ekspor Grid Riwayat Cuti
        public ActionResult EksporRiwayatPDF()
        {
            var model = DB.TM_Riwayats;
            return GridViewExtension.ExportToPdf(GetDataRiwayatSettings(), model.ToList());
        }

        public ActionResult EksporRiwayatXLS()
        {
            var model = DB.TM_Riwayats;
            var exportopt = new XlsExportOptionsEx();
            exportopt.ExportType = DevExpress.Export.ExportType.WYSIWYG;
            // exportopt.CustomizeCell += new DevExpress.Export.CustomizeCellEventHandler(CustomizeCellColor);
            return GridViewExtension.ExportToXls(GetDataRiwayatSettings(), model.ToList(), true, exportopt);
        }

        public ActionResult EksporRiwayatXLSX()
        {
            var model = DB.TM_Riwayats;
            var exportopt = new XlsxExportOptionsEx();
            exportopt.ExportType = DevExpress.Export.ExportType.WYSIWYG;
            // exportopt.CustomizeCell += new DevExpress.Export.CustomizeCellEventHandler(CustomizeCellColor);
            return GridViewExtension.ExportToXlsx(GetDataRiwayatSettings(), model.ToList(), true, exportopt);
        }

        public ActionResult EksporRiwayatRTF()
        {
            var model = DB.TM_Riwayats;
            return GridViewExtension.ExportToXlsx(GetDataRiwayatSettings(), model.ToList());
        }

        public ActionResult EksporRiwayatCSV()
        {
            var model = DB.TM_Riwayats;
            return GridViewExtension.ExportToCsv(GetDataRiwayatSettings(), model.ToList());
        }
        #endregion

        #region Ekspor Grid Sisa Cuti Tahunan
        public ActionResult EksporSisaPDF()
        {
            var model = DB.TM_Cutis;
            return GridViewExtension.ExportToPdf(GetSisaTahunanSettings(), model.ToList());
        }

        public ActionResult EksporSisaXLS()
        {
            var model = DB.TM_Cutis;
            return GridViewExtension.ExportToXls(GetSisaTahunanSettings(), model.ToList());
        }

        public ActionResult EksporSisaXLSX()
        {
            var model = DB.TM_Cutis;
            return GridViewExtension.ExportToXlsx(GetSisaTahunanSettings(), model.ToList());
        }

        public ActionResult EksporSisaRTF()
        {
            var model = DB.TM_Cutis;
            return GridViewExtension.ExportToXlsx(GetSisaTahunanSettings(), model.ToList());
        }

        public ActionResult EksporSisaCSV()
        {
            var model = DB.TM_Cutis;
            return GridViewExtension.ExportToCsv(GetSisaTahunanSettings(), model.ToList());
        }
        #endregion

        #region Ekspor Grid Cuti Massal
        public ActionResult EksporCMPDF()
        {
            var model = DB.TM_List_Cutis;
            return GridViewExtension.ExportToPdf(GetDataCMSettings(), model.ToList());
        }
        #endregion

        #region Ekspor Grid Data Mutasi
        public ActionResult EksporMutasiPDF()
        {
            var model = DB.TM_Mutasis;
            return GridViewExtension.ExportToPdf(GetDataMutasiSettings(), model.ToList());
        }

        public ActionResult EksporMutasiXLS()
        {
            var model = DB.TM_Mutasis;
            return GridViewExtension.ExportToXls(GetDataMutasiSettings(), model.ToList());
        }

        public ActionResult EksporMutasiXLSX()
        {
            var model = DB.TM_Mutasis;
            return GridViewExtension.ExportToXlsx(GetDataMutasiSettings(), model.ToList());
        }

        public ActionResult EksporMutasiRTF()
        {
            var model = DB.TM_Mutasis;
            return GridViewExtension.ExportToXlsx(GetDataMutasiSettings(), model.ToList());
        }

        public ActionResult EksporMutasiCSV()
        {
            var model = DB.TM_Mutasis;
            return GridViewExtension.ExportToCsv(GetDataMutasiSettings(), model.ToList());
        }
        #endregion

        /* END EXPT GRID VIEW */
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

        #region Periksa Persetujuan Cuti
        public ActionResult CheckApproval(string Atasan)
        {
            // populasi data cuti yang belum disetujui
            var DataRiwayat = (from Riwayat in DB.TM_Riwayats
                               where Riwayat.Status_Cuti == "Tunggu" && Riwayat.Pemberi == Atasan
                               select Riwayat).ToArray();
            var i = 0; // counter email

            foreach (TM_Riwayat Riwayats in DataRiwayat)
            {
                // pastikan variabel kosong sebelum proses dimulai
                Nama_Atasan = "";
                Nama_Supervisor = "";

                // pindahkan seluruh elemen data riwayat ke variabel string
                NIK = Riwayats.NIK.ToString().Trim();
                Nama_Karyawan = Riwayats.Nama_Karyawan.ToString().Trim();
                Nama_Atasan = Riwayats.Pemberi.ToString().Trim();
                Jenis_Cuti = Riwayats.Jenis_Cuti.ToString().Trim();
                Keperluan = Riwayats.Keperluan.ToString().Trim();

                // untuk data angka dan tanggal wajib menggunakan explicit cast
                Masa_Cuti = (int)Riwayats.Masa_Cuti;
                Tgl_Masuk = (DateTime)Riwayats.Tgl_Pengajuan;
                Tgl_Awal = (DateTime)Riwayats.Tgl_Mulai;
                Tgl_Akhir = (DateTime)Riwayats.Tgl_Selesai;

                // temukan nama supervisor dari karyawan ybs
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
                            if (!Convert.IsDBNull(sdr["Nama_Supervisor"]))
                            {
                                Nama_Supervisor = sdr["Nama_Supervisor"].ToString().Trim();
                            }
                            if (!Convert.IsDBNull(sdr["Email_Perusahaan"]))
                                Email = sdr["Email_Perusahaan"].ToString().Trim();
                            else
                                Email = sdr["Email"].ToString().Trim();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // jika terdapat gangguan koneksi database
                        Session["ErrorMsg"] = "Terjadi masalah koneksi database saat memproses pencarian data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return RedirectToAction("Riwayat", "Master");
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        Session["ErrorMsg"] = "Terjadi masalah yang tak dikenal selama proses pencarian data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return RedirectToAction("Riwayat", "Master");
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // temukan email atasan dari karyawan ybs
                    cmd = new SqlCommand("SM_Karyawan", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Atasan;
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                    try
                    {
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
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
                        Session["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                        SetPrivilege();
                        return RedirectToAction("Riwayat", "Master");
                    }
                    catch (Exception e)
                    {
                        // jika terdapat gangguan lainnya
                        Session["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                        SetPrivilege();
                        return RedirectToAction("Riwayat", "Master");
                    }
                    finally
                    {
                        sdr.Close();
                        conn.Close();
                    }

                    // jika tidak ada supervisor dari karyawan ybs, lewati prosedur ini
                    if (!string.IsNullOrEmpty(Nama_Supervisor))
                    {
                        // temukan email supervisor dari karyawan ybs
                        cmd = new SqlCommand("SM_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = Nama_Supervisor;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
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
                            Session["ErrorMsg"] = "Terjadi kegagalan koneksi database saat memproses data. Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return RedirectToAction("Riwayat", "Master");
                        }
                        catch (Exception e)
                        {
                            // jika terdapat gangguan lainnya
                            Session["ErrorMsg"] = "Terjadi masalah tak dikenal dalam proses pengambilan data. Segera hubungi admin/support dengan pesan berikut ini: \"" + e.Message + "\"";
                            SetPrivilege();
                            return RedirectToAction("Riwayat", "Master");
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }
                    }
                }

                // kirim email ke atasan terlebih dahulu
                try
                {
                    Msg = new StringBuilder();
                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                    Msg.Append("<br />");
                    Msg.Append("Yth. " + Nama_Atasan + ",");
                    Msg.Append("<br /><br />");
                    if (Jenis_Cuti == "Cuti Tahunan")
                    {
                        Msg.Append("Anda masih mempunyai permohonan cuti tahunan dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Awal) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Akhir) + " dengan keperluan '" + Keperluan + "' yang belum disetujui.<br /><br />");
                        if (Privilege_At == "Admin")
                            Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Admin/ApprovalTahunan\">http://" + ServerIP + "/Admin/ApprovalTahunan</a>.");
                        else if (Privilege_At == "Manager")
                            Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalTahunan\">http://" + ServerIP + "/Manager/ApprovalTahunan</a>.");
                        else
                            Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalTahunan\">http://" + ServerIP + "/Manager/ApprovalTahunan</a>.");
                    }
                    else
                    {
                        Msg.Append("Anda masih mempunyai permohonan cuti khusus " + Jenis_Cuti.ToLower() + " dari karyawan dengan NIK " + NIK + " atas nama " + Nama_Karyawan + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Awal) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Akhir) + " dengan keperluan '" + Keperluan + "' yang belum disetujui.<br /><br />");
                        if (Privilege_At == "Admin")
                            Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Admin/ApprovalKhusus\">http://" + ServerIP + "/Admin/ApprovalKhusus</a>.");
                        else if (Privilege_At == "Manager")
                            Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalKhusus\">http://" + ServerIP + "/Manager/ApprovalKhusus</a>.");
                        else
                            Msg.Append("Mohon segera memproses persetujuan cuti dari karyawan yang bersangkutan dengan menuju tautan/link berikut ini: <a href=\"http://" + ServerIP + "/Manager/ApprovalKhusus\">http://" + ServerIP + "/Manager/ApprovalKhusus</a>.");
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
                    Session["ErrorMsg"] = "Telah terjadi kegagalan dalam pengiriman email ke " + Nama_Atasan + " dengan pesan kesalahan: \"" + smtpex.Message + "\"";
                    SetPrivilege();
                    return RedirectToAction("Riwayat", "Master");
                }
                catch (Exception e)
                {
                    Session["ErrorMsg"] = "Telah terjadi kegagalan dalam pengiriman email ke " + Nama_Atasan + " dengan pesan kesalahan: \"" + e.Message + "\"";
                    SetPrivilege();
                    return RedirectToAction("Riwayat", "Master");
                }

                // terakhir: kirim email pemberitahuan ke karyawan ybs
                try
                {
                    Msg = new StringBuilder();
                    Msg.Append("<span style=\"font-family:Arial, Helvetica, sans-serif; font-size:12px\">");
                    Msg.Append("<br />");
                    Msg.Append("Yth. " + Nama_Karyawan + ",");
                    Msg.Append("<br /><br />");
                    if (Jenis_Cuti == "Cuti Tahunan")
                    {
                        Msg.Append("Anda masih mempunyai permohonan cuti tahunan selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Awal) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Akhir) + " dengan keperluan '" + Keperluan + "' yang belum disetujui oleh atasan anda.<br /><br />");
                        Msg.Append("Mohon segera menghubungi atasan anda untuk melengkapi proses persetujuan cuti anda.");
                    }
                    else
                    {
                        Msg.Append("Anda masih mempunyai permohonan cuti khusus " + Jenis_Cuti.ToLower() + " selama " + Masa_Cuti + " hari dari tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Awal) + " sampai tanggal " + string.Format("{0:dd MMMM yyyy}", Tgl_Akhir) + " dengan keperluan '" + Keperluan + "' yang belum disetujui oleh atasan anda.<br /><br />");
                        Msg.Append("Mohon segera menghubungi atasan anda untuk melengkapi proses persetujuan cuti anda.");
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
                    Msg.Append("Alamat email atasan yang dapat dihubungi: " + Email_Atasan + ", " + Email_Supervisor);
                    Msg.Append("</span>");

                    Helpers.SendTunggu(Msg.ToString(), Email);

                    i++;
                    if (i % 2 == 0)
                    {
                        Thread.Sleep(10000); // delay 30 detik per 2 pengajuan = 4 email terkirim
                    }

                    // untuk pengiriman selanjutnya, bagian atasan & supervisor dikosongkan
                    Nama_Atasan = "";
                    Nama_Supervisor = "";
                }
                catch (SmtpException smtpex)
                {
                    Session["ErrorMsg"] = "Telah terjadi kegagalan dalam pengiriman email ke " + Nama_Karyawan + " dengan pesan kesalahan: \"" + smtpex.Message + "\"";
                    SetPrivilege();
                    return RedirectToAction("Riwayat", "Master");
                }
                catch (Exception e)
                {
                    Session["ErrorMsg"] = "Telah terjadi kegagalan dalam pengiriman email ke " + Nama_Karyawan + " dengan pesan kesalahan: \"" + e.Message + "\"";
                    SetPrivilege();
                    return RedirectToAction("Riwayat", "Master");
                }
            }

            return RedirectToAction("CheckForm", "Master");
        }
        #endregion
	}
}