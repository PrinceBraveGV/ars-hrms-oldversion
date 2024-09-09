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
using SqlBulkTools;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using AristaHRM.Models;

namespace AristaHRM.Controllers
{
    [RoutePrefix("Common")]
    public class CommonController : Controller
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
        public string Nama_Sumber, Nama_File, Ekstensi;
        public string Param_File;
        public HttpPostedFileBase UploadCtrl;

        #endregion

        #region Ekspor Data
        public ActionResult EksporData()
        {
            SetPrivilege();
            var ekspor = new UploadModel()
            {
                Parameter_File = ".XLSX"
            };

            return View(ekspor);
        }

        [HttpPost]
        public ActionResult EksporData(UploadModel model)
        {
            SetPrivilege();

            Object eksporlist;

            if (String.IsNullOrEmpty(model.Nama_File))
            {
                ViewData["ErrorMsg"] = "Nama file hasil ekspor harus diisi sebelum melanjutkan proses ekspor.";
                return View(model);
            }

            var tabel = model.Nama_Tabel.Trim();

            var file = Server.MapPath("~/Files/Export/" + model.Nama_File.Trim() + "." + model.Parameter_File.Trim());
            String mime = MimeMapping.GetMimeMapping(file);

            var list = new List<String>();

            try
            {
                var format = model.Parameter_File.Trim();

                var namespc = "AristaHRM.Models" + model.Nama_Tabel;

                var len = model.Nama_Tabel.Length - 4;

                var grid = new GridViewSettings();
                grid.Name = "ExportGrid";
                grid.SettingsExport.ExportSelectedRowsOnly = false;
                grid.SettingsExport.FileName = model.Nama_File.Trim();
                grid.SettingsExport.PaperKind = System.Drawing.Printing.PaperKind.A4;
                grid.SettingsExport.Landscape = true;

                // baca nama tabel untuk dijadikan nama kolom
                var collist = new List<String>();

                using (var DB = new HRISContext())
                {
                    try
                    {
                        collist = (from t in Type.GetType(namespc).GetProperties()
                                   select t.Name).ToList();
                        // collist sekarang berisi nama kolom dari tabel yang dipilih
                        // sehingga dapat dilakukan operasi foreach
                        foreach (String colname in collist)
                        {
                            if (colname.Contains("TM_") || colname.Contains("TT_"))
                                continue;
                            else
                            {
                                grid.Columns.Add(colname);
                                list.Add(colname);
                            }
                        }

                        switch (model.Nama_Tabel)
                        {
                            case "TM_Karyawan":
                                grid.KeyFieldName = "NIK";
                                break;
                            case "TM_Riwayat":
                                grid.KeyFieldName = "ID_Cuti";
                                break;
                            case "TM_Perusahaan":
                                grid.KeyFieldName = "Kode_Perusahaan";
                                break;
                            case "TM_Cabang":
                                grid.KeyFieldName = "Kode_Cabang";
                                break;
                            case "TM_List_Cuti":
                            case "TM_List_Libur":
                                grid.KeyFieldName = "ID_Daftar";
                                break;
                            default:
                                ViewData["ErrorMsg"] = "Tabel yang dipilih adalah tabel setting sistem yang tidak diperuntukkan untuk ekspor data.";
                                return View(model);
                        }
                    }
                    catch (Exception e)
                    {
                        ViewData["ErrorMsg"] = "Nama tabel yang ditentukan tidak ditemukan.";
                    }

                    eksporlist = null;

                    switch (tabel)
                    {
                        case "TM_Karyawan":
                            eksporlist = DB.TM_Karyawans.ToList();
                            break;
                        case "TM_Riwayat":
                            eksporlist = DB.TM_Riwayats.ToList();
                            break;
                        case "TM_Perusahaan":
                            eksporlist = DB.TM_Perusahaans.ToList();
                            break;
                        case "TM_Cabang":
                            eksporlist = DB.TM_Cabangs.ToList();
                            break;
                        case "TM_List_Cuti":
                            eksporlist = DB.TM_List_Cutis.ToList();
                            break;
                        case "TM_List_Libur":
                            eksporlist = DB.TM_List_Liburs.ToList();
                            break;
                        default:
                            ViewData["ErrorMsg"] = "Tabel yang dipilih adalah tabel setting sistem yang tidak diperuntukkan untuk ekspor data.";
                            return View(model);
                    }

                    ViewData["ErrorMsg"] = String.Empty;

                    switch (format)
                    {
                        case "XLS":
                            return GridViewExtension.ExportToXls(grid, eksporlist);
                        case "XLSX":
                            return GridViewExtension.ExportToXlsx(grid, eksporlist);
                        case "PDF":
                            return GridViewExtension.ExportToPdf(grid, eksporlist);
                        default:
                            goto case "XLSX";
                    }

                }
            }
            catch (Exception e)
            {
                ViewData["ErrorMsg"] = "Kesalahan berikut saat mengekspor data: \"" + e.Message + "\"";
                model.Parameter_File = ".XLSX";
            }

            return View(model);
        }
        #endregion

        #region Impor Data
        public ActionResult ImporData()
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
                    SetPrivilege();
                    Nama_Tabel = (Request.Params["Nama_Tabel"] ?? string.Empty).ToString();
                    return View(new UploadModel { Nama_Tabel = Nama_Tabel });
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }
        }

        [HttpPost]
        public ActionResult ImporData(UploadModel model)
        {
            if (ModelState.IsValid)
            {
                using (var conn = new SqlConnection(connstring))
                {
                    // tahap 1: transfer model ke variabel kendali
                    Nama_Sumber = model.Nama_Sumber.ToString().Trim();
                    Nama_Tabel = model.Nama_Tabel.ToString().Trim();
                    Nama_File = Server.MapPath(model.Nama_File);
                    if (!string.IsNullOrEmpty(model.Parameter_File))
                    {
                        Param_File = model.Parameter_File.ToString().Trim();
                    }

                    if (model.Pass_File != null)
                    {
                        Pass_File = model.Pass_File.ToString().Trim();
                    }

                    ds = new DataSet();
                    dt = new DataTable();

                    // tahap 2: terima stream isi berkas
                    // Perhatian: 
                    // Mengingat bahwa halaman ini melibatkan upload control HTML, pastikan master page yang berkait pada form upload ini wajib menggunakan parameter sbb:
                    // <form id="form1" enctype="multipart/form-data">
                    foreach (string file in Request.Files)
                    {
                        UploadCtrl = this.Request.Files[file];
                    }

                    // sintaks ini serupa dengan:
                    // for (String file : Request.Files)

                    // tahap 3: cek nama file yang diimpor terlebih dahulu
                    if ((UploadCtrl != null && UploadCtrl.ContentLength > 0) || !String.IsNullOrEmpty(Nama_File)) // UploadCtrl = nama field untuk upload file
                    {
                        if (!String.IsNullOrEmpty(UploadCtrl.FileName))
                        {
                            Ekstensi = Path.GetExtension(UploadCtrl.FileName);
                            PathFile = string.Format("{0}\\{1}", Server.MapPath("~/Files/Import"), UploadCtrl.FileName);
                            UploadCtrl.SaveAs(PathFile);
                        }
                        else
                        {
                            Ekstensi = Path.GetExtension(Nama_File);
                            PathFile = string.Format("{0}\\{1}", Server.MapPath("~/Files/Import"), Path.GetFileName(Nama_File));
                        }
                        if (Ekstensi == ".xls")
                        {
                            // jenis file: Excel 2003 ke bawah
                            // set string koneksi dengan Jet 4 OLEDB
                            oleconnstring = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + PathFile + "; Extended Properties=\"Excel 8.0; HDR=Yes; IMEX=2\"; Persist Security Info=False";
                            olecmdstring = "SELECT * FROM [" + Nama_Sumber + "$]";
                        }
                        else if (Ekstensi == ".xlsx")
                        {
                            // jenis file: Excel 2007 ke atas
                            // set string koneksi dengan ACE 12 OLEDB
                            oleconnstring = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + PathFile + "; Extended Properties=\"Excel 12.0; HDR=Yes; IMEX=2\"; Persist Security Info=False";
                            olecmdstring = "SELECT * FROM [" + Nama_Sumber + "$]";
                        }
                        else if (Ekstensi == ".mdb" || Ekstensi == ".mde")
                        {
                            // jenis file: Access 2003 ke bawah
                            // set string koneksi dengan Jet 4 OLEDB
                            oleconnstring = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + PathFile + "; Persist Security Info=False";
                            olecmdstring = "SELECT * FROM " + Nama_Sumber;
                        }
                        else if (Ekstensi == ".accdb" || Ekstensi == ".accde")
                        {
                            // jenis file: Access 2007 ke atas
                            // set string koneksi dengan ACE 12 OLEDB
                            oleconnstring = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + PathFile + "; Persist Security Info=False";
                            olecmdstring = "SELECT * FROM " + Nama_Sumber;
                        }
                        else
                        {
                            // ekstensi di luar ketentuan tidak dapat diproses
                            ViewData["ErrorMsg"] = "Ekstensi berkas/file yang diunggah tidak sesuai dengan ketentuan. Berkas/file yang sesuai untuk upload adalah ekstensi .xls/xlsx, .mdb/mde, .accdb/accde.";
                            SetPrivilege();
                            return View(model);
                        }

                        // hubungkan sistem ke file yang diunggah
                        oleconn = new OleDbConnection(oleconnstring);
                        olecmd = new OleDbCommand(olecmdstring, oleconn);
                        olecmd2 = new OleDbCommand(olecmdstring, oleconn);

                        // tahap 4: mencoba eksekusi koneksi
                        try
                        {
                            oleconn.Open();
                            oledr = olecmd.ExecuteReader();
                            oleda = new OleDbDataAdapter();
                            oleda.SelectCommand = olecmd2;
                            oleda.Fill(dt);

                            #region Email Import
                            // jika parameter yang digunakan adalah parameter email
                            if (Nama_Tabel == "TM_Karyawan" && string.Equals(Param_File, "Email", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // rutin impor email secara otomatis
                                /* BEGIN IMPORT */
                                while (oledr.Read())
                                {
                                    // perulangan dilakukan setiap kali membaca data dari data reader
                                    NIK = oledr["NIK"].ToString().Trim();
                                    Email = oledr["Email"].ToString().Trim();
                                    Email_Perusahaan = oledr["Email_Perusahaan"].ToString().Trim();

                                    cmd = new SqlCommand("SM_Karyawan", conn);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = Email;
                                    if (!string.IsNullOrEmpty(Email_Perusahaan))
                                    {
                                        cmd.Parameters.Add("@Email_Perusahaan", SqlDbType.NVarChar).Value = Email_Perusahaan;
                                    }
                                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Email";

                                    try
                                    {
                                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex)
                                    {
                                        ViewData["ErrorMsg"] = "Sistem mendeteksi gangguan koneksi pada basis data utama. Segera hubungi admin/support jika masalah berlanjut. Kode Pesan: \"" + ex.Message + "\"";
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    catch (Exception e)
                                    {
                                        ViewData["ErrorMsg"] = "Sistem mendeteksi kesalahan tak dikenal dalam proses impor data. Silakan hubungi admin/support dengan kode pesan berikut: \"" + e.Message + "\"";
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open) { conn.Close(); }
                                    }
                                }

                                if (!oledr.IsClosed) { oledr.Close(); }
                                if (oleconn.State == ConnectionState.Open) { oleconn.Close(); }
                                System.IO.File.Delete(PathFile);
                                return RedirectToAction("AddSukses", "Notif");
                                /* END IMPORT */
                            }
                            #endregion

                            else if ((Nama_Tabel == "TM_Karyawan") && string.Equals(Param_File, "Atasan", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // rutin impor nama atasan secara otomatis
                                /* BEGIN IMPORT */
                                while (oledr.Read())
                                {
                                    NIK = oledr["NIK"].ToString().Trim();
                                    Nama_Atasan = oledr["Nama_Atasan_Langsung"].ToString().Trim();
                                    Nama_Supervisor = oledr["Nama_Atasan_Dari_Atasan"].ToString().Trim();

                                    cmd = new SqlCommand("SM_Karyawan", conn);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = NIK;
                                    cmd.Parameters.Add("@Nama_Atasan", SqlDbType.NVarChar).Value = Nama_Atasan;
                                    if (!string.IsNullOrEmpty(Nama_Supervisor))
                                    {
                                        cmd.Parameters.Add("@Nama_Supervisor", SqlDbType.NVarChar).Value = Nama_Supervisor;
                                    }
                                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Atasan";

                                    try
                                    {
                                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex)
                                    {
                                        ViewData["ErrorMsg"] = "Sistem mendeteksi gangguan koneksi pada basis data utama. Segera hubungi admin/support jika masalah berlanjut. Kode Pesan: \"" + ex.Message + "\"";
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    catch (Exception e)
                                    {
                                        ViewData["ErrorMsg"] = "Sistem mendeteksi kesalahan tak dikenal dalam proses impor data. Silakan hubungi admin/support dengan kode pesan berikut: \"" + e.Message + "\"";
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open) { conn.Close(); }
                                    }
                                }

                                if (!oledr.IsClosed) { oledr.Close(); }
                                if (oleconn.State == ConnectionState.Open) { oleconn.Close(); }
                                System.IO.File.Delete(PathFile);
                                return RedirectToAction("AddSukses", "Notif");

                                /* END IMPORT */
                            }

                            sbc = new SqlBulkCopy(connstring);
                            sbc.DestinationTableName = Nama_Tabel;

                            // bugfix 151009: perubahan dari direct bulk copy ke datatable
                            // mengingat bahwa Excel berpotensi memiliki baris dengan seluruh kolom berisi null value,
                            // maka keberadaan null value diantisipasi dengan type cast & salinan ke datatable kemudian diteruskan ke fungsi bulk copy
                            dt = dt.Rows.Cast<DataRow>().Where(r => !r.ItemArray.All(f => f is System.DBNull || (f as string == null) || string.Compare((f as string).Trim(), string.Empty) == 0)).CopyToDataTable();

                            // hapus spasi berlebih sebelum bulk copy
                            foreach (DataColumn cols in dt.Columns)
                            {
                                cols.ColumnName = cols.ColumnName.Trim();
                            }

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                // update 160425: mengeliminasi kesalahan spasi berlebih di kiri & kanan dari user
                                // prosedur trim spasi untuk seluruh isi data tabel
                                /*
                                dt.AsEnumerable().ToList().ForEach(r =>
                                {
                                    var cells = r.ItemArray.ToList();
                                    r.ItemArray = cells.Select(x => x.ToString().Trim()).ToArray();
                                });
                                */

                                // perhatikan apakah ekstensi file adalah ekstensi milik MS Access
                                if (Ekstensi == ".mdb" || Ekstensi == ".mde" || Ekstensi == ".accdb" || Ekstensi == ".accde")
                                {
                                    #region Khusus File MS Access
                                    cmd = new SqlCommand("SP_Import_Access", conn);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("@Nama_Tabel_ACC", SqlDbType.NVarChar).Value = Nama_Sumber;
                                    cmd.Parameters.Add("@Path_File", SqlDbType.NVarChar).Value = PathFile;
                                    cmd.Parameters.Add("@Nama_Tabel", SqlDbType.NVarChar).Value = Nama_Tabel;

                                    if (Pass_File != null)
                                    {
                                        cmd.Parameters.Add("@Password_ACC", SqlDbType.NVarChar).Value = Pass_File;
                                    }

                                    switch (Ekstensi)
                                    {
                                        case ".mdb":
                                        case ".mde":
                                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Access 2003";
                                            break;

                                        case ".accdb":
                                        case ".accde":
                                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Access 2007";
                                            break;

                                        default:
                                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Access 2007";
                                            break;
                                    }

                                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                                    cmd.ExecuteNonQuery();

                                    if (Nama_Tabel == "TM_Karyawan")
                                    {
                                        ListKaryawan = new List<String>();

                                        // rutin pengisian password default secara otomatis
                                        cmd = new SqlCommand("SM_Karyawan", conn);
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Pass";

                                        try
                                        {
                                            if (conn.State != ConnectionState.Open) { conn.Open(); }
                                            sdr = cmd.ExecuteReader();
                                            while (sdr.Read())
                                            {
                                                ListKaryawan.Add(sdr["NIK"].ToString().Trim());
                                            }
                                        }
                                        catch (SqlException ex)
                                        {
                                            ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data. Pesan: \"" + ex.Message + "\"";
                                        }
                                        catch (Exception e)
                                        {
                                            ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal sebagai berikut: " + e.Message;
                                        }
                                        finally
                                        {
                                            sdr.Close();
                                            conn.Close();
                                        }

                                        DaftarKaryawan = ListKaryawan.ToArray<String>();

                                        for (int i = 0; i < DaftarKaryawan.Length; i++)
                                        {
                                            No_Induk = DaftarKaryawan[i].ToString().Trim();
                                            Password = FormsAuthentication.HashPasswordForStoringInConfigFile(No_Induk, "SHA1");

                                            // langsung tulis ke database
                                            cmd = new SqlCommand("SM_Karyawan", conn);
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = No_Induk;
                                            cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Password;
                                            cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Password";

                                            try
                                            {
                                                if (conn.State != ConnectionState.Open) { conn.Open(); }
                                                cmd.ExecuteNonQuery();
                                            }
                                            catch (SqlException ex)
                                            {
                                                ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data karyawan. Pesan: \"" + ex.Message + "\"";
                                                SetPrivilege();
                                                return View(model);
                                            }
                                            catch (Exception e)
                                            {
                                                ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses data karyawan sebagai berikut: " + e.Message;
                                                SetPrivilege();
                                                return View(model);
                                            }
                                            finally
                                            {
                                                conn.Close();
                                            }
                                        }
                                    }
                                    #endregion

                                    // sebelum tampilkan layar sukses, hapus dulu berkas di direktori sementara
                                    return RedirectToAction("AddSukses", "Notif");
                                }
                                else
                                {
                                    var bulk = new BulkOperations();
                                    // perbedaan kolom diselesaikan dengan column mapping
                                    switch (Nama_Tabel)
                                    {
                                        case "TM_Riwayat":
                                            // metode "bulk insert"
                                            dt.Columns.Add("CreatedBy", typeof(String));
                                            dt.Columns.Add("CreatedDate", typeof(DateTime));
                                            dt.Columns.Add("ModifiedBy", typeof(String));
                                            dt.Columns.Add("ModifiedDate", typeof(DateTime));

                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                dt.Rows[i]["CreatedBy"] = User.Identity.Name;
                                                dt.Rows[i]["CreatedDate"] = DateTime.Now;
                                                dt.Rows[i]["ModifiedBy"] = User.Identity.Name;
                                                dt.Rows[i]["ModifiedDate"] = DateTime.Now;
                                            }

                                            var listriwayat = dt.AsEnumerable().Select(x => new DataRiwayat()
                                            {
                                                ID_Cuti = x.Field<String>("ID_Cuti"),
                                                NIK = x.Field<String>("NIK"),
                                                Nama_Karyawan = x.Field<String>("Nama_Karyawan"),
                                                Jenis_Cuti = x.Field<String>("Jenis_Cuti"),
                                                Masa_Cuti = x.Field<int?>("Masa_Cuti"),
                                                Tgl_Mulai = x.Field<DateTime?>("Tgl_Mulai"),
                                                Tgl_Selesai = x.Field<DateTime?>("Tgl_Selesai"),
                                                Tgl_Pengajuan = x.Field<DateTime?>("Tgl_Pengajuan"),
                                                Tgl_Setuju = x.Field<DateTime?>("Tgl_Setuju"),
                                                Keperluan = x.Field<String>("Keperluan"),
                                                Pemberi = x.Field<String>("Pemberi"),
                                                Status_Cuti = x.Field<String>("Status_Cuti"),
                                                Keterangan = x.Field<String>("Keterangan"),
                                                Tahun_Cuti = x.Field<int?>("Tahun_Cuti"),
                                                CreatedBy = x.Field<String>("CreatedBy"),
                                                CreatedDate = x.Field<DateTime?>("CreatedDate"),
                                                ModifiedBy = x.Field<String>("ModifiedBy"),
                                                ModifiedDate = x.Field<DateTime?>("ModifiedDate")
                                            });

                                            bulk.Setup<DataRiwayat>()
                                                .ForCollection(listriwayat)
                                                .WithTable("TM_Riwayat")
                                                .AddAllColumns()
                                                .BulkInsert()
                                                .Commit(conn);

                                            // penyesuaian untuk tabel riwayat cuti
                                            /*
                                            sbc.ColumnMappings.Add("ID_Cuti", "ID_Cuti");
                                            sbc.ColumnMappings.Add("NIK", "NIK");
                                            sbc.ColumnMappings.Add("Nama_Karyawan", "Nama_Karyawan");
                                            sbc.ColumnMappings.Add("Jenis_Cuti", "Jenis_Cuti");
                                            sbc.ColumnMappings.Add("Masa_Cuti", "Masa_Cuti");
                                            sbc.ColumnMappings.Add("Tgl_Mulai", "Tgl_Mulai");
                                            sbc.ColumnMappings.Add("Tgl_Selesai", "Tgl_Selesai");
                                            sbc.ColumnMappings.Add("Tgl_Pengajuan", "Tgl_Pengajuan");
                                            sbc.ColumnMappings.Add("Tgl_Setuju", "Tgl_Setuju");
                                            sbc.ColumnMappings.Add("Keperluan", "Keperluan");
                                            sbc.ColumnMappings.Add("Pemberi", "Pemberi");
                                            sbc.ColumnMappings.Add("Status_Cuti", "Status_Cuti");
                                            sbc.ColumnMappings.Add("Keterangan", "Keterangan");
                                            sbc.ColumnMappings.Add("Tahun_Cuti", "Tahun_Cuti");

                                            sbc.WriteToServer(dt);
                                            */
                                            break;

                                        case "TM_Karyawan":
                                            // khusus untuk impor karyawan
                                            // metode yang digunakan adalah "bulk insert"

                                            dt.Columns.Add("Email_Valid", typeof(String));
                                            dt.Columns.Add("Pembuat", typeof(String));
                                            dt.Columns.Add("Privilege", typeof(String));
                                            dt.Columns.Add("Area_Kerja", typeof(String));
                                            dt.Columns.Add("CreatedBy", typeof(String));
                                            dt.Columns.Add("CreatedDate", typeof(DateTime));
                                            dt.Columns.Add("ModifiedBy", typeof(String));
                                            dt.Columns.Add("ModifiedDate", typeof(DateTime));

                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                dt.Rows[i]["Email_Valid"] = "1";
                                                dt.Rows[i]["Pembuat"] = "Admin";
                                                dt.Rows[i]["CreatedBy"] = User.Identity.Name;
                                                dt.Rows[i]["CreatedDate"] = DateTime.Now;
                                                dt.Rows[i]["ModifiedBy"] = User.Identity.Name;
                                                dt.Rows[i]["ModifiedDate"] = DateTime.Now;

                                                if (dt.Rows[i]["Jabatan"].ToString().Contains("Direktur") || dt.Rows[i]["Jabatan"].ToString().Contains("Director") || dt.Rows[i]["Jabatan"].ToString().Contains("Komisaris"))
                                                {
                                                    dt.Rows[i]["Privilege"] = "Admin";
                                                }
                                                else if (dt.Rows[i]["Jabatan"].ToString().Contains("Manager") || dt.Rows[i]["Jabatan"].ToString().Contains("Manajer"))
                                                {
                                                    dt.Rows[i]["Privilege"] = "Manager";
                                                }
                                                else if (dt.Rows[i]["Jabatan"].ToString().Contains("Supervisor") || dt.Rows[i]["Jabatan"].ToString().Contains("SPV"))
                                                {
                                                    dt.Rows[i]["Privilege"] = "Supervisor";
                                                }
                                                else
                                                {
                                                    dt.Rows[i]["Privilege"] = "Staff";
                                                }

                                                // setting area kerja
                                                if (!dt.Rows[i]["Cabang"].ToString().Contains("Pusat"))
                                                {
                                                    dt.Rows[i]["Area_Kerja"] = "Cabang";
                                                }
                                                else
                                                {
                                                    dt.Rows[i]["Area_Kerja"] = "Pusat";
                                                }
                                            }

                                            var listkar = dt.AsEnumerable().Select(x => new DataKaryawan()
                                            {
                                                NIK = x.Field<String>("NIK"),
                                                Nama_Karyawan = x.Field<String>("Nama_Karyawan"),
                                                Jenis_Kelamin = x.Field<String>("Jenis_Kelamin"),
                                                Tempat_Lahir = x.Field<String>("Tempat_Lahir"),
                                                Tgl_Lahir = x.Field<DateTime?>("Tgl_Lahir"),
                                                Alamat = x.Field<String>("Alamat"),
                                                Agama = x.Field<String>("Agama"),
                                                Status_Nikah = x.Field<String>("Status_Nikah"),
                                                Email = x.Field<String>("Email"),
                                                Email_Perusahaan = x.Field<String>("Email_Perusahaan"),
                                                Perusahaan = x.Field<String>("Perusahaan"),
                                                Cabang = x.Field<String>("Cabang"),
                                                Jabatan = x.Field<String>("Jabatan"),
                                                Departemen = x.Field<String>("Departemen"),
                                                Nama_Atasan = x.Field<String>("Nama_Atasan_Langsung"),
                                                Nama_Supervisor = x.Field<String>("Nama_Atasan_Dari_Atasan"),
                                                Tgl_Masuk = x.Field<DateTime?>("Tgl_Masuk"),
                                                Status_Karyawan = x.Field<String>("Status_Karyawan"),
                                                Status_Kerja = x.Field<String>("Status_Kerja"),
                                                Email_Valid = Convert.ToInt32(x.Field<String>("Email_Valid").ToString() ?? "1"),
                                                Pembuat = x.Field<String>("Pembuat"),
                                                Privilege = x.Field<String>("Privilege"),
                                                Area_Kerja = x.Field<String>("Area_Kerja"),
                                                CreatedBy = x.Field<String>("CreatedBy"),
                                                CreatedDate = x.Field<DateTime?>("CreatedDate"),
                                                ModifiedBy = x.Field<String>("ModifiedBy"),
                                                ModifiedDate = x.Field<DateTime?>("ModifiedDate")
                                            });

                                            if (Nama_Tabel == "TM_Trainee")
                                            {
                                                bulk.Setup<DataKaryawan>()
                                                    .ForCollection(listkar)
                                                    .WithTable("TM_Trainee")
                                                    .AddAllColumns()
                                                    .BulkInsert()
                                                    .Commit(conn);
                                            }
                                            else
                                            {
                                                bulk.Setup<DataKaryawan>()
                                                    .ForCollection(listkar)
                                                    .WithTable("TM_Karyawan")
                                                    .AddAllColumns()
                                                    .BulkInsert()
                                                    .Commit(conn);
                                            }

                                            // penyesuaian untuk tabel karyawan
                                            /*
                                            sbc.ColumnMappings.Add("NIK", "NIK");
                                            sbc.ColumnMappings.Add("Nama_Karyawan", "Nama_Karyawan");
                                            sbc.ColumnMappings.Add("Jenis_Kelamin", "Jenis_Kelamin");
                                            sbc.ColumnMappings.Add("Tempat_Lahir", "Tempat_Lahir");
                                            sbc.ColumnMappings.Add("Tgl_Lahir", "Tgl_Lahir");
                                            sbc.ColumnMappings.Add("Alamat", "Alamat");
                                            sbc.ColumnMappings.Add("Agama", "Agama");
                                            sbc.ColumnMappings.Add("Status_Nikah", "Status_Nikah");
                                            sbc.ColumnMappings.Add("Email", "Email");
                                            sbc.ColumnMappings.Add("Email_Perusahaan", "Email_Perusahaan");
                                            sbc.ColumnMappings.Add("Perusahaan", "Perusahaan");
                                            sbc.ColumnMappings.Add("Cabang", "Cabang");
                                            sbc.ColumnMappings.Add("Jabatan", "Jabatan");
                                            sbc.ColumnMappings.Add("Departemen", "Departemen");
                                            sbc.ColumnMappings.Add("Nama_Atasan_Langsung", "Nama_Atasan");
                                            sbc.ColumnMappings.Add("Nama_Atasan_Dari_Atasan", "Nama_Supervisor");
                                            sbc.ColumnMappings.Add("Tgl_Masuk", "Tgl_Masuk");
                                            sbc.ColumnMappings.Add("Status_Karyawan", "Status_Karyawan");
                                            sbc.ColumnMappings.Add("Status_Kerja", "Status_Kerja");
                                            sbc.ColumnMappings.Add("Pembuat", "Pembuat");
                                            sbc.ColumnMappings.Add("Privilege", "Privilege");
                                            // sbc.ColumnMappings.Add("Area_Kerja", "Area_Kerja");
                                             * */
                                            break;
                                    }

                                    // sbc.WriteToServer(dt);
                                }
                            }
                            else
                            {
                                // dt = dt.rows.cast<dataRow>.where(r -> !r.itemArray.all(f -> f instanceof dataNull || f partof String == null || String.compare(f partof String).trim(), String.empty) == 0)).copyToDataTable();
                                dt = dt.Rows.Cast<DataRow>().Where(r => !r.ItemArray.All(f => f is System.DBNull || (f as string == null) || string.Compare((f as string).Trim(), string.Empty) == 0)).CopyToDataTable();

                                // perbedaan kolom diselesaikan dengan column mapping
                                switch (Nama_Tabel)
                                {
                                    case "TM_Riwayat":
                                        // penyesuaian untuk tabel riwayat cuti
                                        sbc.ColumnMappings.Add("ID_Cuti", "ID_Cuti");
                                        sbc.ColumnMappings.Add("NIK", "NIK");
                                        sbc.ColumnMappings.Add("Nama_Karyawan", "Nama_Karyawan");
                                        sbc.ColumnMappings.Add("Jenis_Cuti", "Jenis_Cuti");
                                        sbc.ColumnMappings.Add("Masa_Cuti", "Masa_Cuti");
                                        sbc.ColumnMappings.Add("Tgl_Mulai", "Tgl_Mulai");
                                        sbc.ColumnMappings.Add("Tgl_Selesai", "Tgl_Selesai");
                                        sbc.ColumnMappings.Add("Tgl_Pengajuan", "Tgl_Pengajuan");
                                        sbc.ColumnMappings.Add("Tgl_Setuju", "Tgl_Setuju");
                                        sbc.ColumnMappings.Add("Keperluan", "Keperluan");
                                        sbc.ColumnMappings.Add("Pemberi", "Pemberi");
                                        sbc.ColumnMappings.Add("Status_Cuti", "Status_Cuti");
                                        sbc.ColumnMappings.Add("Keterangan", "Keterangan");
                                        sbc.ColumnMappings.Add("Tahun_Cuti", "Tahun_Cuti");

                                        sbc.WriteToServer(dt);
                                        break;

                                    case "TM_Karyawan":
                                        // khusus untuk impor karyawan
                                        // metode yang digunakan adalah "bulk insert"

                                        var bulk = new BulkOperations();

                                            dt.Columns.Add("Email_Valid", typeof(String));
                                            dt.Columns.Add("Pembuat", typeof(String));
                                            dt.Columns.Add("Privilege", typeof(String));
                                            dt.Columns.Add("Area_Kerja", typeof(String));
                                            dt.Columns.Add("CreatedBy", typeof(String));
                                            dt.Columns.Add("CreatedDate", typeof(DateTime));
                                            dt.Columns.Add("ModifiedBy", typeof(String));
                                            dt.Columns.Add("ModifiedDate", typeof(DateTime));

                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                dt.Rows[i]["Email_Valid"] = "1";
                                                dt.Rows[i]["Pembuat"] = "Admin";
                                                dt.Rows[i]["CreatedBy"] = User.Identity.Name;
                                                dt.Rows[i]["CreatedDate"] = DateTime.Now;
                                                dt.Rows[i]["ModifiedBy"] = User.Identity.Name;
                                                dt.Rows[i]["ModifiedDate"] = DateTime.Now;

                                                if (dt.Rows[i]["Jabatan"].ToString().Contains("Direktur") || dt.Rows[i]["Jabatan"].ToString().Contains("Director") || dt.Rows[i]["Jabatan"].ToString().Contains("Komisaris"))
                                                {
                                                    dt.Rows[i]["Privilege"] = "Admin";
                                                }
                                                else if (dt.Rows[i]["Jabatan"].ToString().Contains("Manager") || dt.Rows[i]["Jabatan"].ToString().Contains("Manajer"))
                                                {
                                                    dt.Rows[i]["Privilege"] = "Manager";
                                                }
                                                else if (dt.Rows[i]["Jabatan"].ToString().Contains("Supervisor") || dt.Rows[i]["Jabatan"].ToString().Contains("SPV"))
                                                {
                                                    dt.Rows[i]["Privilege"] = "Supervisor";
                                                }
                                                else
                                                {
                                                    dt.Rows[i]["Privilege"] = "Staff";
                                                }

                                                // setting area kerja
                                                if (!dt.Rows[i]["Cabang"].ToString().Contains("Pusat"))
                                                {
                                                    dt.Rows[i]["Area_Kerja"] = "Cabang";
                                                }
                                                else
                                                {
                                                    dt.Rows[i]["Area_Kerja"] = "Pusat";
                                                }
                                            }

                                            var listkar = dt.AsEnumerable().Select(x => new DataKaryawan()
                                            {
                                                NIK = x.Field<String>("NIK"),
                                                Nama_Karyawan = x.Field<String>("Nama_Karyawan"),
                                                Jenis_Kelamin = x.Field<String>("Jenis_Kelamin"),
                                                Tempat_Lahir = x.Field<String>("Tempat_Lahir"),
                                                Tgl_Lahir = x.Field<DateTime?>("Tgl_Lahir"),
                                                Alamat = x.Field<String>("Alamat"),
                                                Agama = x.Field<String>("Agama"),
                                                Status_Nikah = x.Field<String>("Status_Nikah"),
                                                Email = x.Field<String>("Email"),
                                                Email_Perusahaan = x.Field<String>("Email_Perusahaan"),
                                                Perusahaan = x.Field<String>("Perusahaan"),
                                                Cabang = x.Field<String>("Cabang"),
                                                Jabatan = x.Field<String>("Jabatan"),
                                                Departemen = x.Field<String>("Departemen"),
                                                Nama_Atasan = x.Field<String>("Nama_Atasan_Langsung"),
                                                Nama_Supervisor = x.Field<String>("Nama_Atasan_Dari_Atasan"),
                                                Tgl_Masuk = x.Field<DateTime?>("Tgl_Masuk"),
                                                Status_Karyawan = x.Field<String>("Status_Karyawan"),
                                                Status_Kerja = x.Field<String>("Status_Kerja"),
                                                Email_Valid = Convert.ToInt32(x.Field<String>("Email_Valid") ?? "1"),
                                                Pembuat = x.Field<String>("Pembuat"),
                                                Privilege = x.Field<String>("Privilege"),
                                                Area_Kerja = x.Field<String>("Area_Kerja"),
                                                CreatedBy = x.Field<String>("CreatedBy"),
                                                CreatedDate = x.Field<DateTime?>("CreatedDate"),
                                                ModifiedBy = x.Field<String>("ModifiedBy"),
                                                ModifiedDate = x.Field<DateTime?>("ModifiedDate")
                                            });

                                            if (Nama_Tabel == "TM_Trainee")
                                            {
                                                bulk.Setup<DataKaryawan>()
                                                    .ForCollection(listkar)
                                                    .WithTable("TM_Trainee")
                                                    .AddAllColumns()
                                                    .BulkInsert()
                                                    .Commit(conn);
                                            }
                                            else
                                            {
                                                bulk.Setup<DataKaryawan>()
                                                    .ForCollection(listkar)
                                                    .WithTable("TM_Karyawan")
                                                    .AddAllColumns()
                                                    .BulkInsert()
                                                    .Commit(conn);
                                            }

                                        /*
                                        sbc.ColumnMappings.Add("NIK", "NIK");
                                        sbc.ColumnMappings.Add("Nama_Karyawan", "Nama_Karyawan");
                                        sbc.ColumnMappings.Add("Jenis_Kelamin", "Jenis_Kelamin");
                                        sbc.ColumnMappings.Add("Tempat_Lahir", "Tempat_Lahir");
                                        sbc.ColumnMappings.Add("Tgl_Lahir", "Tgl_Lahir");
                                        sbc.ColumnMappings.Add("Alamat", "Alamat");
                                        sbc.ColumnMappings.Add("Agama", "Agama");
                                        sbc.ColumnMappings.Add("Status_Nikah", "Status_Nikah");
                                        sbc.ColumnMappings.Add("Email", "Email");
                                        sbc.ColumnMappings.Add("Email_Perusahaan", "Email_Perusahaan");
                                        sbc.ColumnMappings.Add("Perusahaan", "Perusahaan");
                                        sbc.ColumnMappings.Add("Cabang", "Cabang");
                                        sbc.ColumnMappings.Add("Jabatan", "Jabatan");
                                        sbc.ColumnMappings.Add("Departemen", "Departemen");
                                        sbc.ColumnMappings.Add("Nama_Atasan_Langsung", "Nama_Atasan");
                                        sbc.ColumnMappings.Add("Nama_Atasan_Dari_Atasan", "Nama_Supervisor");
                                        sbc.ColumnMappings.Add("Tgl_Masuk", "Tgl_Masuk");
                                        sbc.ColumnMappings.Add("Status_Karyawan", "Status_Karyawan");
                                        sbc.ColumnMappings.Add("Status_Kerja", "Status_Kerja");
                                        sbc.ColumnMappings.Add("Pembuat", "Pembuat");
                                        sbc.ColumnMappings.Add("Privilege", "Privilege");
                                        */
                                        break;
                                }

                                // sbc.WriteToServer(dt);
                            }

                            // jika tabel yang diimpor adalah tabel karyawan, set password default karyawan masing-masing secara otomatis
                            if (Nama_Tabel == "TM_Karyawan")
                            {
                                #region Pengisian Password Karyawan
                                ListKaryawan = new List<string>();

                                // rutin pengisian password default secara otomatis
                                cmd = new SqlCommand("SM_Karyawan", conn);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Pass";

                                try
                                {
                                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                                    sdr = cmd.ExecuteReader();
                                    while (sdr.Read())
                                    {
                                        ListKaryawan.Add(sdr["NIK"].ToString().Trim());
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data. Pesan: \"" + ex.Message + "\"";
                                }
                                catch (Exception e)
                                {
                                    ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal sebagai berikut: " + e.Message;
                                }
                                finally
                                {
                                    sdr.Close();
                                    conn.Close();
                                }

                                DaftarKaryawan = ListKaryawan.ToArray<string>();

                                for (int i = 0; i < DaftarKaryawan.Length; i++)
                                {
                                    No_Induk = DaftarKaryawan[i].ToString().Trim();
                                    Password = FormsAuthentication.HashPasswordForStoringInConfigFile(No_Induk, "SHA1");

                                    // langsung tulis ke database
                                    cmd = new SqlCommand("SM_Karyawan", conn);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = No_Induk;
                                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Password;
                                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Password";

                                    try
                                    {
                                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex)
                                    {
                                        ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data karyawan. Pesan: \"" + ex.Message + "\"";
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    catch (Exception e)
                                    {
                                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses data karyawan sebagai berikut: " + e.Message;
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    finally
                                    {
                                        conn.Close();
                                    }
                                }
                                #endregion
                            }
                            else if (Nama_Tabel == "TM_Trainee")
                            {
                                #region Pengisian Password Karyawan
                                ListKaryawan = new List<string>();

                                // rutin pengisian password default secara otomatis
                                cmd = new SqlCommand("SM_Trainee", conn);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select Pass";

                                try
                                {
                                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                                    sdr = cmd.ExecuteReader();
                                    while (sdr.Read())
                                    {
                                        ListKaryawan.Add(sdr["NIK"].ToString().Trim());
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data. Pesan: \"" + ex.Message + "\"";
                                }
                                catch (Exception e)
                                {
                                    ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal sebagai berikut: " + e.Message;
                                }
                                finally
                                {
                                    sdr.Close();
                                    conn.Close();
                                }

                                DaftarKaryawan = ListKaryawan.ToArray<string>();

                                for (int i = 0; i < DaftarKaryawan.Length; i++)
                                {
                                    No_Induk = DaftarKaryawan[i].ToString().Trim();
                                    Password = FormsAuthentication.HashPasswordForStoringInConfigFile(No_Induk, "SHA1");

                                    // langsung tulis ke database
                                    cmd = new SqlCommand("SM_Trainee", conn);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = No_Induk;
                                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Password;
                                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update Password";

                                    try
                                    {
                                        if (conn.State != ConnectionState.Open) { conn.Open(); }
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex)
                                    {
                                        ViewData["ErrorMsg"] = "Terjadi kesalahan dalam pemrosesan data karyawan. Pesan: \"" + ex.Message + "\"";
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    catch (Exception e)
                                    {
                                        ViewData["ErrorMsg"] = "Terjadi kesalahan tak dikenal dalam proses data karyawan sebagai berikut: " + e.Message;
                                        SetPrivilege();
                                        return View(model);
                                    }
                                    finally
                                    {
                                        conn.Close();
                                    }
                                }
                                #endregion
                            }

                            return RedirectToAction("AddSukses", "Notif");
                        }
                        catch (OleDbException ex)
                        {
                            ViewData["ErrorMsg"] = "Sistem mendeteksi gangguan koneksi pada berkas yang anda pakai untuk proses impor. Periksa kembali kelengkapan berkas dan ulangi proses impor. Kode Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View(model);
                        }
                        catch (SqlException ex)
                        {
                            ViewData["ErrorMsg"] = "Sistem mendeteksi gangguan koneksi pada basis data utama. Segera hubungi admin/support jika masalah berlanjut. Kode Pesan: \"" + ex.Message + "\"";
                            SetPrivilege();
                            return View(model);
                        }
                        catch (Exception e)
                        {
                            ViewData["ErrorMsg"] = "Sistem mendeteksi kesalahan tak dikenal dalam proses impor data. Silakan hubungi admin/support dengan kode pesan berikut: \"" + e.Message + "\"";
                            SetPrivilege();
                            return View(model);
                        }
                        finally
                        {
                            if (oledr != null)
                            {
                                if (!oledr.IsClosed)
                                    oledr.Close();
                            }
                            if (oleconn.State == ConnectionState.Open)
                            {
                                oleconn.Close();
                            }

                            if (System.IO.File.Exists(PathFile))
                            {
                                System.IO.File.Delete(PathFile);
                            }
                        }
                    }
                    else
                    {
                        ViewData["ErrorMsg"] = "File belum dipilih atau tidak ditemukan. Pastikan file yang dipilih ada pada direktori.";
                        SetPrivilege();
                        return View(model);
                    }
                }
            }
            else
            {
                Nama_Tabel = (Request.Params["Nama_Tabel"] ?? string.Empty).ToString();

                // tampilkan struktur tabel tujuan
                if (model.Nama_Tabel == null)
                {
                    ViewData["ErrorMsg"] = "Anda belum menentukan nama tabel tujuan atau target impor data.";
                }
                else
                {
                    Nama_Tabel = model.Nama_Tabel.ToString().Trim();
                    ViewData["Nama_Tabel_Tujuan"] = Nama_Tabel;
                }

                if (model.Nama_Sumber == null)
                {
                    ViewData["ErrorMsg"] = "Anda belum menentukan nama tabel atau worksheet sumber data.";
                }

                SetPrivilege();
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult UploadData(IEnumerable<UploadedFile> upd)
        {
            // mengingat fungsi ini hanya mengembalikan nama file sebagai callback client-side,
            // return harus diset ke null
            var files = UploadControlExtension.GetUploadedFiles("Upload", Helpers.ValidationSettings, Helpers.UploadComplete);
            return null;
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