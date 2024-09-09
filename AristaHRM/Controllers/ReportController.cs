using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AristaHRM.Models;
using AristaHRM.Reports;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;

namespace AristaHRM.Controllers
{
    [RoutePrefix("Report")]
    public class ReportController : Controller
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

        // variabel konteks - langsung dideklarasikan
        public HRISContext DB = new HRISContext();

        #region Variabel Model
        // model karyawan
        public string NIK;
        public string Nama_Karyawan;
        public string Perusahaan, Cabang, Jabatan, Departemen;
        public string Nama_Atasan, Nama_Supervisor, Nama_Advisor;
        public string Privilege;
        public string Parameter;

        // variabel angka
        public double Selisih_Awal, Selisih_Akhir;
        public int Total_CM;

        // variabel tanggal
        public DateTime Tgl_Masuk, Tgl_Resign;
        public DateTime Tgl_Awal, Tgl_Akhir;
        public DateTime Tgl_Cuti_Massal;

        #endregion

        // variabel lainnya
        public HttpCookie record;
        #endregion

        /* BEGIN */

        #region Laporan Data Karyawan
        /*
         *  Bagian daftar karyawan versi standar
         *  Seluruh profil karyawan ditampilkan per halaman berdasarkan kriteria pencarian
         */

        public ActionResult ReportKaryawan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager" || ViewBag.Privilege == "Supervisor")
                {
                    ViewData["ReportKaryawan"] = new ReportKaryawan();
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportKaryawanPartial()
        {
            ViewData["ReportKaryawan"] = new ReportKaryawan();
            return PartialView("_ReportKaryawan");
        }

        public ActionResult EksporKaryawan()
        {
            return DocumentViewerExtension.ExportTo(new ReportKaryawan());
        }

        /*
         *   Bagian list karyawan versi tabular penuh
         *   Data yang ditampilkan: NIK, nama karyawan, tanggal lahir, perusahaan, cabang, jabatan, departemen, nama atasan, tanggal awal masuk kerja
         */

        public ActionResult ReportListKaryawan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager" || ViewBag.Privilege == "Supervisor")
                {
                    ViewData["ReportListKaryawan"] = new ReportListKaryawan();
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportListKaryawanPartial()
        {
            ViewData["ReportListKaryawan"] = new ReportListKaryawan();
            return PartialView("_ReportListKaryawan");
        }

        public ActionResult EksporListKaryawan()
        {
            return DocumentViewerExtension.ExportTo(new ReportListKaryawan());
        }

        #endregion

        #region Laporan Data Perusahaan
        public ActionResult ReportPerusahaan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager" || ViewBag.Privilege == "Supervisor")
                {
                    ViewData["ReportPerusahaan"] = new ReportPerusahaan();
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportPerusahaanPartial()
        {
            ViewData["ReportPerusahaan"] = new ReportPerusahaan();
            return PartialView("_ReportPerusahaan");
        }

        public ActionResult EksporPerusahaan()
        {
            return DocumentViewerExtension.ExportTo(new ReportPerusahaan());
        }
        #endregion

        #region Laporan Data Cabang
        public ActionResult ReportCabang()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager" || ViewBag.Privilege == "Supervisor")
                {
                    ViewData["ReportCabang"] = new ReportCabang();
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportCabangPartial()
        {
            ViewData["ReportCabang"] = new ReportCabang();
            return PartialView("_ReportCabang");
        }

        public ActionResult EksporCabang()
        {
            return DocumentViewerExtension.ExportTo(new ReportCabang());
        }
        #endregion

        #region Laporan Data Cuti Keseluruhan
        public ActionResult ReportRiwayat()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager" || ViewBag.Privilege == "Supervisor")
                {
                    var report = new ReportRiwayat();

                    // jika privilege user adalah manajer, lakukan filtering berdasarkan nama atasan
                    if (ViewBag.Privilege == "Manager")
                    {
                        // query ViewBag data karyawan di sini
                        ViewBag.Nama_Atasan = User.Identity.Name;
                        Parameter Param1 = new Parameter() { Name = "NamaAtasan", Type = typeof(string), Value = ViewBag.Nama_Atasan, Visible = false };

                        report.FilterString = "Contains([Nama_Atasan], ?NamaAtasan)";
                    }
                    ViewData["ReportRiwayat"] = report;
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportRiwayatPartial()
        {
            var report = new ReportRiwayat();

            // jika privilege user adalah manajer, lakukan filtering berdasarkan nama atasan
            if (ViewBag.Privilege == "Manager")
            {
                // query ViewBag data karyawan di sini
                ViewBag.Nama_Atasan = User.Identity.Name;
                Parameter Param1 = new Parameter() { Name = "NamaAtasan", Type = typeof(string), Value = ViewBag.Nama_Atasan, Visible = false };

                report.FilterString = "Contains([Nama_Atasan], ?NamaAtasan)";
            }

            ViewData["ReportRiwayat"] = report;
            return PartialView("_ReportRiwayat");
        }

        public ActionResult EksporRiwayat()
        {
            return DocumentViewerExtension.ExportTo(new ReportRiwayat());
        }
        #endregion

        #region Laporan Data Cuti Perorangan
        public ActionResult ReportCuti()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager" || ViewBag.Privilege == "Supervisor")
                {
                    ViewData["ReportCuti"] = new ReportCuti();
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportCutiPartial()
        {
            ViewData["ReportCuti"] = new ReportCuti();
            return PartialView("_ReportCuti");
        }

        public ActionResult EksporCuti()
        {
            return DocumentViewerExtension.ExportTo(new ReportCuti());
        }
        #endregion

        #region Laporan Data Cuti Menunggu Persetujuan
        public ActionResult ReportTunggu()
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
                    var report = new ReportTunggu();
                    ViewData["ReportTunggu"] = report;
                    return View();
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportTungguPartial()
        {
            var report = new ReportTunggu();

            ViewData["ReportTunggu"] = report;
            return PartialView("_ReportTunggu");
        }

        public ActionResult EksporTunggu()
        {
            return DocumentViewerExtension.ExportTo(new ReportTunggu());
        }
        #endregion

        #region Laporan Sisa Cuti Tahunan
        public ActionResult ReportSisaTahunan()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager" || ViewBag.Privilege == "Supervisor")
                {
                    // hitung sisa cuti tahunan terlebih dahulu untuk NIK ybs
                    var model = new KaryawanModel();
                    record = Request.Cookies["LoginID"];
                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(record.Values["NIK"]))
                        {
                            NIK = record.Values["NIK"].ToString();
                        }

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
                            else if (Tgl_Masuk.Date <= DateTime.Now.Date && Tgl_Masuk.Month <= DateTime.Now.Month)
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
                        }
                        ViewData["ReportSisaCT"] = new ReportSisaCT();
                        return View(model);
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

        public ActionResult ReportSisaTahunanPartial()
        {
            ViewData["ReportSisaCT"] = new ReportSisaCT();
            return PartialView("_ReportSisaTahunan");
        }

        public ActionResult EksporSisaCT()
        {
            return DocumentViewerExtension.ExportTo(new ReportSisaCT());
        }

        [HttpPost]
        public ActionResult ReportSisaTahunan(KaryawanModel model)
        {
            if (!string.IsNullOrEmpty(model.NIK))
            {

                // susun stored procedure
                // mencari data karyawan yang sedang aktif login
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

                    NIK = model.NIK.ToString().Trim();
                    Tgl_Awal = Tgl_Masuk;
                    if (Tgl_Masuk.Month < DateTime.Now.Month)
                    {
                        Tgl_Akhir = Tgl_Masuk.AddYears((DateTime.Now.Year - Tgl_Masuk.Year + 1));
                    }
                    else if (Tgl_Masuk.Date <= DateTime.Now.Date && Tgl_Masuk.Month <= DateTime.Now.Month)
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
                }

                ViewData["ReportSisaCT"] = new ReportSisaCT();
                SetPrivilege();
                return View(model);
            }
            else
            {
                SetPrivilege();
                return View(model);
            }
        }

        #endregion

        #region Laporan Sisa Cuti Tahunan (Atasan & Bawahan)
        public ActionResult ReportSisaBawahan()
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
                    // bersihkan semua data dengan query penghapusan terlebih dahulu
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Hitung_Cuti_Tahunan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Delete";

                        // eksekusi parameter query di balik layar
                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        var model = new KaryawanModel();
                        record = Request.Cookies["LoginID"];
                        if (record != null)
                        {
                            if (!string.IsNullOrEmpty(record.Values["NIK"]))
                            {
                                NIK = record.Values["NIK"].ToString();
                            }

                            ViewData["NIK"] = NIK;

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
                            ViewData["ReportSisaCT"] = new ReportSisaCT();
                            return View(model);

                        }
                        else
                        {
                            return RedirectToAction("Forbidden", "Error");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        public ActionResult ReportSisaBawahan(KaryawanModel model)
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
                    record = Request.Cookies["LoginID"];
                    if (record != null)
                    {
                        // bersihkan semua data dengan query penghapusan terlebih dahulu
                        using (var conn = new SqlConnection(connstring))
                        {
                            cmd = new SqlCommand("SP_Hitung_Cuti_Tahunan", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Delete";

                            // eksekusi parameter query di balik layar
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                            conn.Close();

                            ViewData["NIK"] = NIK;

                            NIK = model.NIK.ToString().Trim();

                            // susun stored procedure
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

                            // tahap 1: susun array untuk semua karyawan bawahan yang aktif
                            var DataKaryawan = (from Karyawan in DB.TM_Karyawans
                                                where (Karyawan.Nama_Atasan == Nama_Karyawan || Karyawan.Nama_Karyawan == Nama_Karyawan || Karyawan.Nama_Advisor == Nama_Karyawan) && Karyawan.Status_Kerja == "Aktif"
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

                        ViewData["ReportSisaCT"] = new ReportSisaCT();
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("Forbidden", "Error");
                    }
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        public ActionResult ReportSisaBawahanPartial()
        {
            ViewData["ReportSisaCTB"] = new ReportSisaCTB();
            return PartialView("_ReportSisaBawahan");
        }

        public ActionResult EksporSisaCTB()
        {
            return DocumentViewerExtension.ExportTo(new ReportSisaCTB());
        }
        #endregion

        #region Riwayat Pengajuan Cuti
        public ActionResult RiwayatPengajuan(FindModel model)
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
                    // penulisan data ke database riwayat pengajuan cuti
                    ViewData["Tahun"] = DateTime.Now.Year;
                    Session["Tahun"] = ViewData["Tahun"].ToString();

                    model.String_Akhir = DateTime.Now.Year.ToString();

                    // set stored procedure
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Riwayat_Pengajuan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = DateTime.Now.Year;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Cuti Khusus";

                        try
                        {
                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data dari database. Pesan: \"" + ex.Message + "\"";
                            return View(ViewData);
                        }
                        finally
                        {
                            conn.Close();
                        }

                        cmd = new SqlCommand("SP_Riwayat_Pengajuan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Tahun_Cuti", SqlDbType.Int).Value = DateTime.Now.Year;
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

                    // deklarasi model
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult RiwayatPengajuan(FindModel se, string returnUrl)
        {
            // penulisan data ke database riwayat pengajuan cuti
            string Tahun_Cuti = se.String_Akhir.ToString();
            ViewData["Tahun"] = Tahun_Cuti;
            Session["Tahun"] = ViewData["Tahun"].ToString();

            // set stored procedure
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_Riwayat_Pengajuan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
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

                cmd = new SqlCommand("SP_Riwayat_Pengajuan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
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

        public ActionResult RiwayatPengajuanPartial()
        {
            return PartialView("_RiwayatPengajuan");
        }
        #endregion

        #region Riwayat Cuti Manajer & Karyawan Bawahan
        public ActionResult RiwayatManajer(FindModel model)
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
                    // penulisan data ke database riwayat pengajuan cuti
                    ViewData["Tahun"] = DateTime.Now.Year;
                    Session["Tahun"] = ViewData["Tahun"].ToString();

                    model.String_Akhir = DateTime.Now.Year.ToString();

                    // set stored procedure
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Riwayat_User", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                        cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = User.Identity.Name;
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
                        cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = User.Identity.Name;
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
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult RiwayatManajer(FindModel se, string returnUrl)
        {
            // penulisan data ke database riwayat pengajuan cuti
            string Tahun_Cuti = se.String_Akhir.ToString();
            ViewData["Tahun"] = Tahun_Cuti;
            Session["Tahun"] = ViewData["Tahun"].ToString();

            // set stored procedure
            using (var conn = new SqlConnection(connstring))
            {
                cmd = new SqlCommand("SP_Riwayat_User", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = User.Identity.Name;
                cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = User.Identity.Name;
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
                cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = User.Identity.Name;
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

        public ActionResult RiwayatManajerPartial()
        {
            return PartialView("_RiwayatManajer");
        }
        #endregion

        // daerah khusus untuk laporan hasil PMK / pengajuan mutasi karyawan
        public ActionResult ReportPMK()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();

                // periksa privilege user yang aktif


            }


            return View();
        }

        public ActionResult ReportPMKPartial() 
        {
            string noPMK = (Session["NoPMK"] ?? String.Empty).ToString();
            DateTime? tglPMK = Convert.ToDateTime(Session["NoPMK"] ?? DateTime.Now.Date);
            string jenisPMK = (Session["JenisPMK"] ?? String.Empty).ToString();

            if (string.IsNullOrWhiteSpace(noPMK) || string.IsNullOrWhiteSpace(jenisPMK))
            {
                // nomor atau jenis PMK kosong, proses dibatalkan

            }

            // contoh sementara
            // nomor PMK akan digenerate dengan pola tertentu
            noPMK = "HRD-PMK-19-07-0001";
            jenisPMK = "Perubahan Status";

            var report = new ReportPMK(noPMK, tglPMK, jenisPMK);

            report.CreateDocument();
            report.PrintingSystem.Document.AutoFitToPagesWidth = 1;

            ViewData["ReportPMK"] = report;

            return PartialView("_ReportPMK");
        }

        public ActionResult EksporPMK()
        {
            return DocumentViewerExtension.ExportTo(new ReportPMK());
        }

        #region Data Statistik Karyawan
        public ActionResult StatKaryawan()
        {
            var model = new KaryawanStatModel();
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin" || ViewBag.Privilege == "Manager")
                {
                    // hitung statistik karyawan
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Status_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Count";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        // konversi data tabel ke model........
                        cmd = new SqlCommand("SP_Status_Karyawan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        if (sdr.HasRows)
                        {
                            while (sdr.Read())
                            {
                                // membaca seri 16 data cuti harian dan mingguan
                                Parameter = sdr["ID_Daftar"].ToString().Trim();

                                switch (Parameter)
                                {
                                    case "01": // total karyawan HO
                                        model.TotalPusat = sdr["Nilai"].ToString().Trim();
                                        break;
                                    case "02": // total karyawan HO yang aktif
                                        model.KarPusatAktif = sdr["Nilai"].ToString().Trim();
                                        break;
                                    case "03": // total karyawan HO yang resign
                                        model.KarPusatResign = sdr["Nilai"].ToString().Trim();
                                        break;
                                    case "04": // total karyawan cabang
                                        model.TotalCabang = sdr["Nilai"].ToString().Trim();
                                        break;
                                    case "05": // total karyawan cabang yang aktif
                                        model.KarCabangAktif = sdr["Nilai"].ToString().Trim();
                                        break;
                                    case "06": // total karyawan cabang yang resign
                                        model.KarCabangResign = sdr["Nilai"].ToString().Trim();
                                        break;
                                    default:
                                        Parameter = "00";
                                        break;
                                }
                            }
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
        #endregion

        #region Data Statistik Cuti
        public ActionResult StatCuti()
        {
            var model = new CutiStatModel();
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("UserSession", "Home", new { ReturnUrl = Request.Url.PathAndQuery.ToString() });
            }
            else
            {
                SetPrivilege();
                if (ViewBag.Privilege == "Admin")
                {
                    // hitung statistik cuti harian & mingguan
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Status_Cuti_Bawahan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Admin";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        // konversi data tabel ke model........
                        cmd = new SqlCommand("SP_Status_Cuti_Bawahan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        if (sdr.HasRows)
                        {
                            while (sdr.Read())
                            {
                                // membaca seri 16 data cuti harian dan mingguan
                                Parameter = sdr["ID_Daftar"].ToString().Trim();

                                switch (Parameter)
                                {
                                    case "01": // cuti tahunan harian keseluruhan
                                        model.CTHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "02": // cuti tahunan harian yang disetujui
                                        model.CTApproveHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "03": // cuti tahunan harian yang ditolak
                                        model.CTRejectHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "04": // cuti tahunan harian yang dibatalkan
                                        model.CTBatalHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "05": // cuti khusus harian keseluruhan
                                        model.CKHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "06":
                                        model.CKApproveHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "07":
                                        model.CKRejectHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "08":
                                        model.CKBatalHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "09":
                                        model.CTMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "10":
                                        model.CTApproveMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "11":
                                        model.CTRejectMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "12":
                                        model.CTBatalMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "13":
                                        model.CKMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "14":
                                        model.CKApproveMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "15":
                                        model.CKRejectMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "16":
                                        model.CKBatalMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    default:
                                        Parameter = "00";
                                        break;
                                }
                            }
                        }
                    }
                    return View(model);
                }
                else if (ViewBag.Privilege == "Manager")
                {
                    // untuk manager, nama diri sendiri bisa langsung diambil untuk definisi parameter pemberi cuti
                    Nama_Karyawan = User.Identity.Name;

                    // hitung statistik cuti harian & mingguan
                    using (var conn = new SqlConnection(connstring))
                    {
                        cmd = new SqlCommand("SP_Status_Cuti_Bawahan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Karyawan;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Manager";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        // konversi data tabel ke model........
                        cmd = new SqlCommand("SP_Status_Cuti_Bawahan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        if (sdr.HasRows)
                        {
                            while (sdr.Read())
                            {
                                // membaca seri 16 data cuti harian dan mingguan
                                Parameter = sdr["ID_Daftar"].ToString().Trim();

                                switch (Parameter)
                                {
                                    case "01": // cuti tahunan harian keseluruhan
                                        model.CTHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "02": // cuti tahunan harian yang disetujui
                                        model.CTApproveHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "03": // cuti tahunan harian yang ditolak
                                        model.CTRejectHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "04": // cuti tahunan harian yang dibatalkan
                                        model.CTBatalHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "05": // cuti khusus harian keseluruhan
                                        model.CKHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "06":
                                        model.CKApproveHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "07":
                                        model.CKRejectHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "08":
                                        model.CKBatalHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "09":
                                        model.CTMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "10":
                                        model.CTApproveMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "11":
                                        model.CTRejectMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "12":
                                        model.CTBatalMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "13":
                                        model.CKMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "14":
                                        model.CKApproveMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "15":
                                        model.CKRejectMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "16":
                                        model.CKBatalMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    default:
                                        Parameter = "00";
                                        break;
                                }
                            }
                        }
                    }
                    return View(model);
                }
                else if (ViewBag.Privilege == "Supervisor")
                {
                    /*
                     *  PERHATIAN:
                     *  Untuk supervisor, nama diri TIDAK dapat langsung diambil sebagai parameter nama atasan,
                     *  melainkan harus melalui manager terlebih dahulu sebagai pemberi cuti
                     *  
                     *  Mengingat ada supervisor yang memiliki nama yang sama dengan nama karyawan staff lain (kasus Sri Maryati),
                     *  NIK digunakan untuk mencari nama atasan dari supervisor ybs
                     */

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
                                Nama_Atasan = sdr["Nama_Atasan"].ToString().Trim();
                            }
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Terjadi kesalahan dalam memproses data. Pesan: \"" + ex.Message + "\"";
                            return View(model);
                        }
                        finally
                        {
                            sdr.Close();
                            conn.Close();
                        }

                        // hitung statistik cuti harian & mingguan
                        cmd = new SqlCommand("SP_Status_Cuti_Bawahan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Supervisor";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        // konversi data tabel ke model........
                        cmd = new SqlCommand("SP_Status_Cuti_Bawahan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";

                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                        sdr = cmd.ExecuteReader();
                        if (sdr.HasRows)
                        {
                            while (sdr.Read())
                            {
                                // membaca seri 16 data cuti harian dan mingguan
                                Parameter = sdr["ID_Daftar"].ToString().Trim();

                                switch (Parameter)
                                {
                                    case "01": // cuti tahunan harian keseluruhan
                                        model.CTHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "02": // cuti tahunan harian yang disetujui
                                        model.CTApproveHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "03": // cuti tahunan harian yang ditolak
                                        model.CTRejectHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "04": // cuti tahunan harian yang dibatalkan
                                        model.CTBatalHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "05": // cuti khusus harian keseluruhan
                                        model.CKHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "06":
                                        model.CKApproveHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "07":
                                        model.CKRejectHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "08":
                                        model.CKBatalHarian = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "09":
                                        model.CTMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "10":
                                        model.CTApproveMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "11":
                                        model.CTRejectMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "12":
                                        model.CTBatalMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "13":
                                        model.CKMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "14":
                                        model.CKApproveMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "15":
                                        model.CKRejectMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    case "16":
                                        model.CKBatalMingguan = sdr["Total_Cuti"].ToString().Trim();
                                        break;
                                    default:
                                        Parameter = "00";
                                        break;
                                }
                            }
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
}