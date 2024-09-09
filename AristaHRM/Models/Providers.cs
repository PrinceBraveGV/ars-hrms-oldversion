using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using DevExpress.Web;
using AristaHRM.Models.Absensi;

namespace AristaHRM.Models
{
    #region Kelas Provider
    /// <summary>
    /// Kelas provider untuk berbagai fungsi internal.
    /// </summary>
    public static class Providers
    {
        const String ContextKey = "DataContext";
        const String AbsenContextKey = "AbsenContext";
        const string SelectModeSessionKey = "4C0A9E6A-5D76-48F9-9086-CD5E9D481928";
        public static string DataHJ, DataUM;

        #region Profil Konteks Database
        /// <summary>
        /// Referensi konteks database sebagai perwakilan dari data aktual di SQL Server.
        /// </summary>
        public static HRISContext DB
        {
            get
            {
                if (HttpContext.Current.Items[ContextKey] == null)
                {
                    HttpContext.Current.Items[ContextKey] = new HRISContext();
                }
                return (HRISContext)HttpContext.Current.Items[ContextKey];
            }
        }
        #endregion

        #region Profil Konteks Database Luar
        public static AbsensiDataContext ADB
        {
            get
            {
                if (HttpContext.Current.Items[AbsenContextKey] == null)
                {
                    HttpContext.Current.Items[AbsenContextKey] = new AbsensiDataContext();
                }
                return (AbsensiDataContext)HttpContext.Current.Items[AbsenContextKey];
            }
        }
        #endregion

        #region Variabel Sesi
        static HttpSessionState Session
        {
            get
            {
                return HttpContext.Current.Session;
            }
        }

        // update 160128: manajemen sesi untuk pilihan multi-select dalam DX grid view
        #region Session Grid Selector
        public static GridViewSelectAllCheckBoxMode SelectAllMode
        {
            get
            {
                if (Session[SelectModeSessionKey] == null)
                    Session[SelectModeSessionKey] = GridViewSelectAllCheckBoxMode.Page; // standar 1 halaman dipilih semua
                return (GridViewSelectAllCheckBoxMode)Session[SelectModeSessionKey];
            }
            set
            {
                Session[SelectModeSessionKey] = value;
            }
        }
        #endregion

        #endregion

        #region List Tabel Keseluruhan
        /// <summary>
        /// Mendapatkan daftar semua tabel dari database.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TabelData> GetListTabel()
        {
            var tabel = new List<TabelData>();

            using (var DB = new HRISContext())
            {
                var metadata = ((IObjectContextAdapter)DB).ObjectContext.MetadataWorkspace;
                var listtabel = metadata.GetItemCollection(DataSpace.SSpace).GetItems<EntityContainer>()
                    .SingleOrDefault()
                    .BaseEntitySets.OfType<EntitySet>()
                    .Where(s => !s.MetadataProperties.Contains("Type") || s.MetadataProperties["Type"].ToString() == "Tables")
                    .OrderBy(s => s.Table);

                foreach (var data in listtabel)
                {
                    var nama = data.MetadataProperties.Contains("Table")
                        && data.MetadataProperties["Table"].Value != null
                        ? data.MetadataProperties["Table"].Value.ToString()
                        : data.Name;

                    var skema = data.MetadataProperties["Schema"].Value.ToString();

                    tabel.Add(new TabelData()
                    {
                        NamaTabel = nama,
                        Alias = nama.Substring(3, nama.Length - 3),
                        SkemaTabel = skema
                    });
                }
            }

            tabel = tabel.OrderBy(x => x.Alias).ToList();

            return tabel;
        }
        #endregion

        #region List Kolom dari Tabel yang Dipilih
        /// <summary>
        /// Mendapatkan daftar nama kolom beserta tipe datanya dari suatu tabel.
        /// </summary>
        /// <param name="Tabel">Nama tabel yang dipilih.</param>
        /// <returns></returns>
        public static IDictionary<String, Type> GetInfoKolom(String Tabel)
        {
            var kolom = new Dictionary<String, Type>();
            var dt = new DataTable();
            if (!String.IsNullOrEmpty(Tabel))
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    using (var cmd = new SqlCommand("SP_ListKolom", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nama_Tabel", SqlDbType.NVarChar).Value = Tabel;

                        conn.Open();

                        using (var sda = new SqlDataAdapter())
                        {
                            try
                            {
                                sda.SelectCommand = cmd;
                                sda.Fill(dt);

                                var tipe = new Type[dt.Rows.Count];
                                var listtipe = new String[dt.Rows.Count];

                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    listtipe[i] = dt.Rows[i]["DATA_TYPE"].ToString().ToLower();
                                }

                                int count = 0;
                                foreach (String tp in listtipe)
                                {
                                    switch (tp)
                                    {
                                        case "bit":
                                            tipe[count] = typeof(bool);
                                            break;
                                        case "int":
                                            tipe[count] = typeof(int);
                                            break;
                                        case "smallint":
                                            tipe[count] = typeof(short);
                                            break;
                                        case "bigint":
                                            tipe[count] = typeof(long);
                                            break;
                                        case "tinyint":
                                            tipe[count] = typeof(sbyte);
                                            break;
                                        case "float":
                                            tipe[count] = typeof(float);
                                            break;
                                        case "real":
                                            tipe[count] = typeof(double);
                                            break;
                                        case "binary":
                                        case "varbinary":
                                        case "image":
                                            tipe[count] = typeof(byte);
                                            break;
                                        case "date":
                                        case "time":
                                        case "smalldatetime":
                                        case "datetime":
                                        case "datetime2":
                                            tipe[count] = typeof(DateTime);
                                            break;
                                        case "text":
                                        case "ntext":
                                        case "varchar":
                                        case "nvarchar":
                                            tipe[count] = typeof(String);
                                            break;
                                        case "char":
                                        case "nchar":
                                            tipe[count] = typeof(char);
                                            break;
                                        case "smallmoney":
                                        case "money":
                                            tipe[count] = typeof(decimal);
                                            break;
                                    }
                                    count++;
                                }

                                int n = 0;
                                foreach (DataRow row in dt.Rows)
                                {
                                    kolom.Add(row.Field<String>("COLUMN_NAME"), tipe[n]);
                                    n++;
                                }
                            }
                            catch (Exception e)
                            {
                                Session["Error"] = "Kesalahan dalam memproses query: '" + e.Message + "'";
                            }
                            finally
                            {
                                if (conn.State == ConnectionState.Open)
                                {
                                    conn.Close();
                                }
                            }
                        }
                    }
                }
            }
            return kolom;
        }

        /// <summary>
        /// Mendapatkan semua nama kolom dalam suatu tabel.
        /// </summary>
        /// <param name="Tabel">Nama tabel yang dipilih.</param>
        /// <returns></returns>
        public static IEnumerable<String> GetListKolom(String Tabel)
        {
            var info = GetInfoKolom(Tabel);
            var daftar = new List<String>(info.Keys);
            return daftar.ToList();
        }
        #endregion

        #region Enumerasi tabel berikut kolom tabel
        public static IEnumerable<string> GetKolom(string Nama_Tabel)
        {
            List<string> Nama_Kolom = new List<string>();
            string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            if (!string.IsNullOrEmpty(Nama_Tabel))
            {
                using (var conn = new SqlConnection(connstring))
                {
                    var cmd = new SqlCommand("SP_List_Kolom", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Nama_Tabel", SqlDbType.NVarChar).Value = Nama_Tabel;
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    SqlDataReader sdr = cmd.ExecuteReader();
                    var dt = sdr.GetSchemaTable();
                    foreach (DataRow rows in dt.Rows)
                    {
                        Nama_Kolom.Add(rows.Field<string>("ColumnName"));
                    }
                }
            }
            return Nama_Kolom.ToList();
        }
        #endregion

        #region Enumerasi Data Tabel
        /*
         *  Enumerasi data tabel berhubungan dengan form isian
         *  Semua operasi query pada bagian ini dilakukan melalui LINQ query
         */

        #region Data Karyawan
        /// <summary>
        /// Memperoleh data karyawan secara umum.
        /// </summary>
        /// <returns>List[Karyawan]</returns>
        public static IEnumerable<TM_Karyawan> GetKaryawan()
        {
            return (from Karyawan in DB.TM_Karyawans
                    select Karyawan).ToList();
        }

        /// <summary>
        /// Memperoleh data karyawan berdasarkan NIK bersangkutan.
        /// </summary>
        /// <param name="NIK">Nomor Induk Karyawan</param>
        /// <returns>List[Karyawan]</returns>
        public static IEnumerable<TM_Karyawan> GetKaryawanByNIK(string NIK)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.NIK == NIK
                    select Karyawan).ToList();
        }

        /// <summary>
        /// Memperoleh data karyawan berdasarkan nama atasan utama.
        /// </summary>
        /// <param name="Nama_Atasan">Nama Atasan Utama</param>
        /// <returns>List[Karyawan]</returns>
        public static IEnumerable<TM_Karyawan> GetKaryawanByAtasan(string Nama_Atasan)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Nama_Atasan == Nama_Atasan || Karyawan.Nama_Karyawan == Nama_Atasan && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }

        /// <summary>
        /// Memperoleh data karyawan berdasarkan nama atasan utama dan departemen kerja.
        /// </summary>
        /// <param name="Nama_Atasan"></param>
        /// <param name="Departemen"></param>
        /// <returns></returns>
        public static IEnumerable<TM_Karyawan> GetKaryawanByAtasan(string Nama_Atasan, string Departemen)
        {
            if (Departemen == "Accounting")
            {
                return (from Karyawan in DB.TM_Karyawans
                        where (Karyawan.Nama_Atasan == Nama_Atasan || Karyawan.Nama_Karyawan == Nama_Atasan || Karyawan.Nama_Advisor == Nama_Atasan)
                        && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                        orderby Karyawan.NIK ascending
                        select Karyawan).ToList();
            }
            else
            {
                return (from Karyawan in DB.TM_Karyawans
                        where (Karyawan.Nama_Atasan == Nama_Atasan || Karyawan.Nama_Karyawan == Nama_Atasan) && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                        orderby Karyawan.NIK ascending
                        select Karyawan).ToList();
            }
        }

        public static IEnumerable<TM_Karyawan> GetKaryawanByNIKAtasan(string NIK_Atasan)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.NIK_Atasan == NIK_Atasan || Karyawan.NIK_Supervisor == NIK_Atasan) && Karyawan.Tgl_Resign == null
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }

        public static IEnumerable<SelectListItem> GetSelectListAtasan()
        {
            var query = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Privilege == "Manager" && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                         orderby Karyawan.NIK ascending
                         select Karyawan).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.NIK, Value = items.NIK });
            }

            return list;
        }

        public static IEnumerable<SelectListItem> GetSelectListAtasan(string Nama_Atasan)
        {
            var query = (from Karyawan in DB.TM_Karyawans
                         where (Karyawan.Nama_Atasan == Nama_Atasan || Karyawan.Nama_Karyawan == Nama_Atasan) && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                        orderby Karyawan.NIK ascending
                        select Karyawan).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.NIK, Value = items.NIK });
            }

            return list;
        }

        /// <summary>
        /// Memperoleh daftar nama karyawan berdasarkan nama supervisor/atasan kedua.
        /// </summary>
        /// <param name="Nama_Supervisor">Nama Atasan Kedua</param>
        /// <returns>List[Karyawan]</returns>

        public static IEnumerable<TM_Karyawan> GetKaryawanBySupervisor(string Nama_Supervisor)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Nama_Supervisor == Nama_Supervisor || Karyawan.Nama_Karyawan == Nama_Supervisor) && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                    orderby Karyawan.NIK ascending
                    select Karyawan).ToList();
        }

        /// <summary>
        /// Memperoleh daftar nama karyawan berdasarkan nama supervisor ybs.
        /// </summary>
        /// <param name="Nama_Supervisor"></param>
        /// <param name="NIK"></param>
        /// <returns></returns>
        public static IEnumerable<TM_Karyawan> GetKaryawanBySupervisor(string Nama_Supervisor, string NIK)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Nama_Supervisor == Nama_Supervisor || (Karyawan.Nama_Karyawan == Nama_Supervisor && Karyawan.NIK == NIK))
                    && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }

        public static IEnumerable<SelectListItem> GetSelectListSupervisor()
        {
            var query = (from Karyawan in DB.TM_Karyawans
                         where Karyawan.Privilege == "Supervisor" && Karyawan.Aktif_Login == true
                         orderby Karyawan.NIK ascending
                         select Karyawan).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.NIK, Value = items.NIK });
            }

            return list;
        }

        public static IEnumerable<SelectListItem> GetSelectListSupervisor(string Nama_Supervisor)
        {
            var query = (from Karyawan in DB.TM_Karyawans
                         where (Karyawan.Nama_Supervisor == Nama_Supervisor || Karyawan.Nama_Karyawan == Nama_Supervisor) && Karyawan.Aktif_Login == true && Karyawan.Tgl_Resign == null
                         orderby Karyawan.NIK ascending
                         select Karyawan).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.NIK, Value = items.NIK });
            }

            return list;
        }

        /// <summary>
        /// Mendapatkan daftar nama karyawan berdasarkan departemen saja.
        /// </summary>
        /// <param name="Nama_Karyawan"></param>
        /// <param name="Cabang"></param>
        /// <param name="Departemen"></param>
        /// <returns></returns>
        public static IEnumerable<TM_Karyawan> GetKaryawanByDepartemen(string Nama_Karyawan, string Departemen)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Departemen == Departemen && Karyawan.Nama_Karyawan != Nama_Karyawan
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }

        /// <summary>
        /// Mendapatkan daftar nama karyawan berdasarkan cabang dan departemen.
        /// </summary>
        /// <param name="Nama_Karyawan"></param>
        /// <param name="Cabang"></param>
        /// <param name="Departemen"></param>
        /// <returns></returns>
        public static IEnumerable<TM_Karyawan> GetKaryawanByDepartemen(string Nama_Karyawan, string Cabang, string Departemen)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Cabang == Cabang && Karyawan.Departemen == Departemen && Karyawan.Nama_Karyawan != Nama_Karyawan
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }

        public static IEnumerable<TM_Karyawan> GetListKaryawan()
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Tgl_Resign == null && Karyawan.NIK != "00000"
                    && Karyawan.NIK != "99999"
                    orderby Karyawan.NIK ascending
                    select Karyawan).ToList();
        }

        public static IEnumerable<SelectListItem> GetListKaryawan(String area)
        {
            var listkaryawan = (from Karyawan in DB.TM_Karyawans
                                where Karyawan.Tgl_Resign == null && Karyawan.NIK != "00000"
                                && Karyawan.NIK != "99999"
                                orderby Karyawan.NIK ascending
                                select Karyawan).ToList();

            var listitem = new List<SelectListItem>();
            foreach (var items in listkaryawan)
            {
                listitem.Add(new SelectListItem() { Text = items.NIK, Value = items.NIK });
            }

            return listitem;
        }

        public static IEnumerable<TM_Karyawan> GetNamaKaryawan()
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Tgl_Resign == null && Karyawan.NIK != "00000"
                    && Karyawan.NIK != "99999"
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }

        public static IEnumerable<TM_Karyawan> GetListAtasan()
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Privilege.Equals("Manager", StringComparison.OrdinalIgnoreCase) || Karyawan.Privilege.Equals("Supervisor", StringComparison.OrdinalIgnoreCase)
                    || Karyawan.Privilege.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                    && (Karyawan.Nama_Karyawan != "ADMIN" || Karyawan.Nama_Karyawan != "SISTEM")
                    select Karyawan).ToList();
        }

        public static IEnumerable<TM_Karyawan> GetListBawahan(string NIK, bool isSelected = false)
        {
            var namaKaryawan = GetKaryawanByNIK(NIK).FirstOrDefault()?.Nama_Karyawan;

            var list = (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.NIK_Atasan == NIK
                    || Karyawan.NIK_Supervisor == NIK)
                    && Karyawan.Tgl_Resign == null
                    orderby Karyawan.NIK ascending
                    select Karyawan).ToList();

            if (isSelected)
            {
                return list.Where(x => !x.NIK.Equals(NIK)).ToList();
            }

            return list;
        }

        public static IEnumerable<TM_Karyawan> GetKepalaCabang(String Perusahaan)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Jabatan.Contains("Kepala Cabang") || Karyawan.Privilege == "Manager") && Karyawan.Perusahaan == Perusahaan && Karyawan.Area_Kerja == "Cabang"
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }

        #endregion

        #region Data Jenis Kelamin
        /// <summary>
        /// Mendapatkan jenis kelamin/gender karyawan ybs.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetJenisKelamin()
        {
            return (from TM_Default in DB.TM_Defaults
                    where TM_Default.Kode_Jenis == "Kelamin"
                    select new
                    {
                        Kode_Jenis = TM_Default.Kode_Jenis,
                        Jenis_Isi = TM_Default.Jenis_Isi
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> GetJenisKelamin(String area)
        {
            var query = (from TM_Default in DB.TM_Defaults
                         where TM_Default.Kode_Jenis == "Kelamin"
                         select new
                         {
                             Kode_Jenis = TM_Default.Kode_Jenis,
                             Jenis_Isi = TM_Default.Jenis_Isi
                         }).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.Jenis_Isi, Value = items.Jenis_Isi });
            }

            return list;
        }
        #endregion

        #region Data Agama
        /// <summary>
        /// Mendapatkan jenis agama yang dianut karyawan ybs.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetAgama()
        {
            return (from TM_Default in DB.TM_Defaults
                    where TM_Default.Kode_Jenis == "Agama"
                    select new
                    {
                        Kode_Jenis = TM_Default.Kode_Jenis,
                        Jenis_Isi = TM_Default.Jenis_Isi
                    }).ToList();
        }
        #endregion

        #region Data Provinsi
        /// <summary>
        /// Mendapatkan data provinsi. 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetProvinsi()
        {
            return (from Prov in DB.TM_Wilayahs
                    where Prov.Jenis_Wilayah == "Provinsi"
                    select new
                    {
                        ID_Wilayah = Prov.ID_Wilayah,
                        Nama_Wilayah = Prov.Nama_Wilayah
                    }).ToList();
        }

        public static IEnumerable GetKota()
        {
            return (from Prov in DB.TM_Wilayahs
                    where Prov.Jenis_Wilayah == "Kota"
                    orderby Prov.Nama_Wilayah ascending
                    select new
                    {
                        ID_Wilayah = Prov.ID_Wilayah,
                        Nama_Wilayah = Prov.Nama_Wilayah
                    }).ToList();
        }

        #endregion

        #region Data Status Pernikahan
        /// <summary>
        /// Mendapatkan status pernikahan dari karyawan ybs.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetStatus()
        {
            return (from TM_Default in DB.TM_Defaults
                    where TM_Default.Kode_Jenis == "Status"
                    select new
                    {
                        Kode_Jenis = TM_Default.Kode_Jenis,
                        Jenis_Isi = TM_Default.Jenis_Isi
                    }).ToList();
        }
        #endregion

        #region Data Perusahaan
        /// <summary>
        /// Mendapatkan nama perusahaan.
        /// </summary>
        /// <returns>List[Perusahaan]</returns>
        public static IEnumerable GetPerusahaan()
        {
            return (from TM_Perusahaan in DB.TM_Perusahaans
                    select new
                    {
                        Kode_Perusahaan = TM_Perusahaan.Kode_Perusahaan,
                        Nama_Perusahaan = TM_Perusahaan.Nama_Perusahaan
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> GetPerusahaan(String area)
        {
            var query = (from TM_Perusahaan in DB.TM_Perusahaans
                         select new
                         {
                             Kode_Perusahaan = TM_Perusahaan.Kode_Perusahaan,
                             Nama_Perusahaan = TM_Perusahaan.Nama_Perusahaan
                         }).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.Nama_Perusahaan, Value = items.Nama_Perusahaan });
            }

            return list;
        }
        #endregion

        #region Data Cabang
        public static IEnumerable GetCabang()
        {
            return (from TM_Cabang in DB.TM_Cabangs
                    orderby TM_Cabang.Kode_Cabang ascending
                    select new
                    {
                        Nama_Cabang = TM_Cabang.Nama_Cabang
                    }).ToList();
        }

        /// <summary>
        /// Memperoleh data cabang berdasarkan data perusahaan sebagai parameter.
        /// </summary>
        /// <param name="Nama_Perusahaan"></param>
        /// <returns>List Cabang</returns>

        public static IEnumerable<TM_Cabang> GetCabangByPerusahaan(string Nama_Perusahaan)
        {
            var DaftarCabang = (from Cabang in DB.TM_Cabangs
                                where Cabang.Nama_Perusahaan == Nama_Perusahaan
                                select new CabangModel()
                                {
                                    Kode_Cabang = Cabang.Kode_Cabang,
                                    Nama_Cabang = Cabang.Nama_Cabang
                                }).ToArray();

            List<TM_Cabang> list = new List<TM_Cabang>();
            for (int i = 0; i < DaftarCabang.Length; i++)
            {
                list.Add(new TM_Cabang { Kode_Cabang = DaftarCabang[i].Kode_Cabang, Nama_Cabang = DaftarCabang[i].Nama_Cabang.ToString() });
            }
            return list;
        }
        #endregion

        #region Data Jabatan
        public static IEnumerable GetJabatan()
        {
            return (from TM_Default in DB.TM_Defaults
                    where TM_Default.Kode_Jenis == "Jabatan"
                    orderby TM_Default.Jenis_Isi
                    select new
                    {
                        Kode_Jenis = TM_Default.Kode_Jenis,
                        Jenis_Isi = TM_Default.Jenis_Isi
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> GetJabatan(String area)
        {
            var query = (from TM_Default in DB.TM_Defaults
                         where TM_Default.Kode_Jenis == "Jabatan"
                         orderby TM_Default.Jenis_Isi
                         select new
                         {
                             Kode_Jenis = TM_Default.Kode_Jenis,
                             Jenis_Isi = TM_Default.Jenis_Isi
                         }).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.Jenis_Isi, Value = items.Jenis_Isi });
            }

            return list;
        }

        public static IEnumerable GetMutasiJabatan()
        {
            return (from TM_Default in DB.TM_Defaults
                    where TM_Default.Kode_Jenis == "Jabatan" && (TM_Default.Jenis_Isi != "President Director" && TM_Default.Jenis_Isi != "BOD")
                    orderby TM_Default.Jenis_Isi
                    select new
                    {
                        Jenis_Isi = TM_Default.Jenis_Isi
                    }).ToList();
        }

        public static IEnumerable GetJabatanCabang()
        {
            var query = (from TM_Default in DB.TM_Defaults.AsEnumerable()
                    where TM_Default.Kode_Jenis == "Jabatan"
                    && (TM_Default.Jenis_Isi.Contains("Admin")
                    || TM_Default.Jenis_Isi.Contains("Marketing")
                    || TM_Default.Jenis_Isi.Contains("Sales")
                    || TM_Default.Jenis_Isi.Contains("Service"))
                    orderby TM_Default.Jenis_Isi
                    select new
                    {
                        Kode_Jenis = TM_Default.Kode_Jenis,
                        Jenis_Isi = TM_Default.Jenis_Isi
                    }).ToList();
            return query;
        }
        #endregion

        #region Data Departemen
        public static IEnumerable GetDepartemen()
        {
            return (from TM_Default in DB.TM_Defaults
                    where TM_Default.Kode_Jenis == "Departemen"
                    select new
                    {
                        Kode_Jenis = TM_Default.Kode_Jenis,
                        Jenis_Isi = TM_Default.Jenis_Isi
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> GetDepartemen(String area)
        {
            var query = (from TM_Default in DB.TM_Defaults
                         where TM_Default.Kode_Jenis == "Departemen"
                         select new
                         {
                             Kode_Jenis = TM_Default.Kode_Jenis,
                             Jenis_Isi = TM_Default.Jenis_Isi
                         }).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.Jenis_Isi, Value = items.Jenis_Isi });
            }

            return list;
        }

        public static IEnumerable GetDepartemenCabang()
        {
            var listDept = new List<string>() { "ADM", "MKT", "SLS", "SVC" };
            return (from Departemen in DB.TM_Departemens
                    where listDept.Contains(Departemen.Singkatan)
                    select new
                    {
                        ID_Dept = Departemen.ID_Dept,
                        Nama_Dept = Departemen.Nama_Dept,
                        Singkatan = Departemen.Singkatan
                    }).ToList();
        }
        #endregion

        #region Data Nama Atasan (Manajer)
        /// <summary>
        /// Memperoleh data nama atasan secara umum berdasarkan status karyawan.
        /// </summary>
        /// <returns>List Nama Atasan</returns>
        public static IEnumerable<TM_Karyawan> GetAtasan(string NIK = null, bool isSelected = false)
        {
            var list = (from Karyawan in DB.TM_Karyawans
                    where ((Karyawan.Privilege == "Admin" || Karyawan.Privilege == "Manager" || Karyawan.Privilege == "Supervisor") && (Karyawan.Nama_Karyawan != "Admin" && Karyawan.Nama_Karyawan != "SISTEM") && Karyawan.Status_Kerja == "Aktif" && Karyawan.Tgl_Resign == null)
                    orderby Karyawan.Nama_Karyawan
                    select Karyawan).ToList();

            if (!string.IsNullOrWhiteSpace(NIK) && isSelected)
            {
                return list.Where(x => !x.NIK.Equals(NIK)).ToList();
            }

            return list;
        }

        /// <summary>
        /// Memperoleh data nama atasan yang menjadi manajer dari departemen terkait sebagai parameter.
        /// </summary>
        /// <param name="Departemen"></param>
        /// <returns>List Nama Atasan</returns>
        public static IEnumerable GetAtasanByPerusahaan(string Perusahaan, string Departemen)
        {
            if (Departemen == "HRD" || Departemen == "GA" || Departemen == "Legal")
            {
                return (from Karyawan in DB.TM_Karyawans
                        where (Karyawan.Departemen == "HR & GA" && (Karyawan.Privilege == "Admin" || Karyawan.Privilege == "Manager" && (Karyawan.Nama_Karyawan != "Admin" && Karyawan.Nama_Karyawan != "SISTEM" && Karyawan.Status_Kerja == "Aktif" && Karyawan.Tgl_Resign == null)))
                        && Karyawan.Perusahaan == Perusahaan
                        orderby Karyawan.Nama_Karyawan
                        select new
                        {
                            Nama_Atasan = Karyawan.Nama_Karyawan
                        }).ToList();
            }

            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Departemen == Departemen && (Karyawan.Privilege == "Admin" || Karyawan.Privilege == "Manager" && (Karyawan.Nama_Karyawan != "Admin" && Karyawan.Nama_Karyawan != "SISTEM" && Karyawan.Status_Kerja == "Aktif" && Karyawan.Tgl_Resign == null)))
                    && Karyawan.Perusahaan == Perusahaan
                    orderby Karyawan.Nama_Karyawan
                    select new
                    {
                        Nama_Atasan = Karyawan.Nama_Karyawan
                    }).ToList();
        }

        /// <summary>
        /// Memperoleh data nama atasan yang menjadi manajer dari departemen terkait sebagai parameter.
        /// </summary>
        /// <param name="Departemen"></param>
        /// <returns>List Nama Atasan</returns>
        public static IEnumerable GetAtasanByDepartemen(string Departemen)
        {
            if (Departemen == "HRD" || Departemen == "GA" || Departemen == "Legal")
            {
                return (from Karyawan in DB.TM_Karyawans
                        where (Karyawan.Departemen == "HR & GA" && (Karyawan.Privilege == "Admin" || Karyawan.Privilege == "Manager" && (Karyawan.Nama_Karyawan != "Admin" && Karyawan.Nama_Karyawan != "SISTEM" && Karyawan.Status_Kerja == "Aktif" && Karyawan.Aktif_Login == true)))
                        orderby Karyawan.Nama_Karyawan
                        select new
                        {
                            NIK_Atasan = Karyawan.NIK,
                            Nama_Atasan = Karyawan.Nama_Karyawan
                        }).ToList();
            }

            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Departemen == Departemen && (Karyawan.Privilege == "Admin" || Karyawan.Privilege == "Manager" && (Karyawan.Nama_Karyawan != "Admin" && Karyawan.Nama_Karyawan != "SISTEM" && Karyawan.Status_Kerja == "Aktif" && Karyawan.Aktif_Login == true)))
                    orderby Karyawan.Nama_Karyawan
                    select new
                    {
                        NIK_Atasan = Karyawan.NIK,
                        Nama_Atasan = Karyawan.Nama_Karyawan
                    }).ToList();
        }

        public static IEnumerable GetAdvisor()
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Jabatan.Contains("Director") || Karyawan.Jabatan.Contains("Operation") || Karyawan.Jabatan.Contains("General")) && Karyawan.Aktif_Login == true
                    orderby Karyawan.Nama_Karyawan
                    select new
                    {
                        NIK = Karyawan.NIK,
                        Nama_Karyawan = Karyawan.Nama_Karyawan
                    }).ToList();
        }

        /// <summary>
        /// Memperoleh data nama supervisor dari departemen terkait sebagai parameter.
        /// </summary>
        /// <param name="Departemen"></param>
        /// <returns>List Nama Supervisor</returns>
        public static IEnumerable GetSupervisorByPerusahaan(string Perusahaan, string Departemen)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Departemen == Departemen && Karyawan.Jabatan == "Supervisor" && Karyawan.Status_Kerja == "Aktif")
                    && Karyawan.Perusahaan == Perusahaan
                    select new
                    {
                        NIK_Supervisor = Karyawan.NIK,
                        Nama_Supervisor = Karyawan.Nama_Karyawan
                    }).ToList();
        }

        /// <summary>
        /// Memperoleh data nama supervisor dari departemen terkait sebagai parameter.
        /// </summary>
        /// <param name="Departemen"></param>
        /// <returns>List Nama Supervisor</returns>
        public static IEnumerable GetSupervisorByDepartemen(string Departemen)
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Departemen == Departemen && Karyawan.Jabatan == "Supervisor" && Karyawan.Status_Kerja == "Aktif" && Karyawan.Aktif_Login == true)
                    select new
                    {
                        NIK_Supervisor = Karyawan.NIK,
                        Nama_Supervisor = Karyawan.Nama_Karyawan
                    }).ToList();
        }

        #endregion

        #region Data Nama Kepala Cabang
        public static IEnumerable GetKaCab()
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Privilege == "Manager" && (Karyawan.Area_Kerja == "Cabang" || Karyawan.Jabatan == "Kepala Cabang")
                    orderby Karyawan.Nama_Karyawan ascending
                    select Karyawan).ToList();
        }
        #endregion

        #region Data Nama Atasan (Supervisor)
        /// <summary>
        /// Mendapatkan nama atasan kedua secara umum berdasarkan status karyawan.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetSupervisor()
        {
            return (from TM_Karyawan in DB.TM_Karyawans
                    where TM_Karyawan.Privilege == "Supervisor" && TM_Karyawan.Status_Kerja == "Aktif"
                    select new
                    {
                        NIK = TM_Karyawan.NIK,
                        Nama_Karyawan = TM_Karyawan.Nama_Karyawan
                    }).ToList();
        }

        public static IEnumerable GetAtasanKedua()
        {
            return (from Karyawan in DB.TM_Karyawans
                    where Karyawan.Nama_Supervisor != string.Empty && Karyawan.Nama_Supervisor != "-" && Karyawan.Nama_Supervisor != null && Karyawan.Status_Kerja == "Aktif"
                    select new
                    {
                        NIK = Karyawan.NIK,
                        Nama_Karyawan = Karyawan.Nama_Supervisor
                    }).ToList();
        }
        #endregion

        #region Data Status Karyawan
        public static IEnumerable GetStatusKaryawan()
        {
            return (from Status_Kar in DB.TM_Defaults
                    where Status_Kar.Kode_Jenis == "Status_Karyawan"
                    select new
                    {
                        Kode_Jenis = Status_Kar.Kode_Jenis,
                        Jenis_Isi = Status_Kar.Jenis_Isi
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> GetStatusKaryawan(String area)
        {
            var query = (from Status_Kar in DB.TM_Defaults
                         where Status_Kar.Kode_Jenis == "Status_Karyawan"
                         select new
                         {
                             Kode_Jenis = Status_Kar.Kode_Jenis,
                             Jenis_Isi = Status_Kar.Jenis_Isi
                         }).ToList();

            var list = new List<SelectListItem>();

            foreach (var items in query)
            {
                list.Add(new SelectListItem() { Text = items.Jenis_Isi, Value = items.Jenis_Isi });
            }

            return list;
        }
        #endregion

        #region Data Jenis Cuti
        /// <summary>
        /// Memperoleh data jenis cuti secara umum.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetJenisCuti()
        {
            return (from TblDefault in DB.TM_Defaults
                    where TblDefault.Kode_Jenis == "Jenis_Cuti"
                    orderby TblDefault.Jenis_Isi ascending
                    select new
                    {
                        Jenis_Isi = TblDefault.Jenis_Isi
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> GetJenisCuti(String NIK)
        {
            var DataKaryawan = (from Karyawan in DB.TM_Karyawans
                                where Karyawan.NIK == NIK
                                select new
                                {
                                    Nama_Karyawan = Karyawan.Nama_Karyawan,
                                    Status_Nikah = Karyawan.Status_Nikah,
                                    Agama = Karyawan.Agama,
                                    Jenis_Kelamin = Karyawan.Jenis_Kelamin
                                }).ToArray();

            string Nama_Karyawan = DataKaryawan[0].Nama_Karyawan.ToString().Trim();
            string Status_Nikah = DataKaryawan[0].Status_Nikah.ToString().Trim();
            string Agama = DataKaryawan[0].Agama;
            string Jenis_Kelamin = DataKaryawan[0].Jenis_Kelamin.ToString().Trim();

            var DataJenis = (from TblDefault in DB.TM_Defaults
                             where TblDefault.Kode_Jenis == "Jenis_Cuti" && ((TblDefault.Keterangan == Jenis_Kelamin && TblDefault.Tambahan == Status_Nikah) || TblDefault.Keterangan == Agama.Trim() || TblDefault.Keterangan == "Cuti_Khusus" || TblDefault.Keterangan == "Cuti_Tahunan" || ((TblDefault.Keterangan == null || TblDefault.Keterangan == Agama.Trim()) && TblDefault.Tambahan == Status_Nikah))
                             orderby TblDefault.Jenis_Isi
                             select new
                             {
                                 Kode_Jenis = TblDefault.Kode_Jenis,
                                 Jenis_Isi = TblDefault.Jenis_Isi
                             }).ToList();

            var listitem = new List<SelectListItem>();

            foreach (var items in DataJenis)
            {
                listitem.Add(new SelectListItem() { Text = items.Jenis_Isi, Value = items.Jenis_Isi });
            }

            return listitem;
        }

        /// <summary>
        /// Memperoleh data jenis cuti yang dapat diambil sesuai dengan 3 kriteria: jenis kelamin, agama dan status pernikahan dari karyawan bersangkutan.
        /// </summary>
        /// <param name="Status_Nikah">Status Pernikahan</param>
        /// <param name="Agama">Agama</param>
        /// <param name="Jenis_Kelamin">Jenis Kelamin</param>
        /// <returns>List Jenis Cuti</returns>
        public static IEnumerable<TT_Pengajuan> GetKeperluanByKaryawan(string Status_Nikah, string Agama, string Jenis_Kelamin, string NIK, string Jenis_Cuti)
        {
            // prosedur reset sebelum mulai menggunakan variabel penguji
            DataHJ = null;
            DataUM = null;
            var BaseHaji = (from RiwayatH in DB.TM_Riwayats
                            where (RiwayatH.Jenis_Cuti.Trim() == "Haji" || RiwayatH.Keperluan.Trim() == "Haji") && RiwayatH.NIK == NIK && (RiwayatH.Status_Cuti.Trim() == "Disetujui" || RiwayatH.Status_Cuti.Trim() == "Tunggu")
                            select RiwayatH).ToArray();
            var BaseUmroh = (from RiwayatU in DB.TM_Riwayats
                             where (RiwayatU.Jenis_Cuti.Trim() == "Umroh" || RiwayatU.Keperluan.Trim() == "Umroh") && RiwayatU.NIK == NIK && (RiwayatU.Status_Cuti.Trim() == "Disetujui" || RiwayatU.Status_Cuti.Trim() == "Tunggu")
                             select RiwayatU).ToArray();

            for (int i = 0; i < BaseHaji.Length; i++)
            {
                DataHJ = BaseHaji[i].Jenis_Cuti.ToString();
            }

            for (int i = 0; i < BaseUmroh.Length; i++)
            {
                DataUM = BaseUmroh[i].Jenis_Cuti.ToString();
            }

            // pemeriksaan apakah karyawan ybs sudah pernah haji & umroh selama masa kerja
            // diperiksa berdasarkan data riwayat cuti ybs

            // NB: tidak perlu cek data agama ybs, karena daftar jenis cuti dalam pengajuan sudah dipengaruhi oleh agama (dalam hal ini agama Islam yang dapat memiliki opsi haji & umroh)
            if (!string.IsNullOrEmpty(DataHJ) || !string.IsNullOrEmpty(DataUM))
            {
                if (!string.IsNullOrEmpty(DataUM))
                {
                    var DataJenisCuti = (from TblDefault in DB.TM_Defaults
                                         where TblDefault.Kode_Jenis == "Jenis_Cuti" && TblDefault.Jenis_Isi != "Umroh" && ((TblDefault.Keterangan == Jenis_Kelamin && TblDefault.Tambahan == Status_Nikah) || TblDefault.Keterangan == Agama || TblDefault.Keterangan == "Cuti_Khusus" || TblDefault.Keterangan == "Cuti_Tahunan" || ((TblDefault.Keterangan == null || TblDefault.Keterangan == Agama) && TblDefault.Tambahan == Status_Nikah))
                                         orderby TblDefault.Jenis_Isi ascending
                                         select new
                                         {
                                             Kode_Jenis = TblDefault.Kode_Jenis,
                                             Jenis_Isi = TblDefault.Jenis_Isi
                                         }).ToArray();
                    List<TT_Pengajuan> listcuti = new List<TT_Pengajuan>();
                    for (int i = 0; i < DataJenisCuti.Length; i++)
                    {
                        listcuti.Add(new TT_Pengajuan { ID_Cuti = DataJenisCuti[i].Kode_Jenis.ToString(), Jenis_Cuti = DataJenisCuti[i].Jenis_Isi.ToString() });
                    }

                    return listcuti.Distinct().ToList();
                }
                else if (!string.IsNullOrEmpty(DataHJ))
                {
                    var DataJenisCuti = (from TblDefault in DB.TM_Defaults
                                         where TblDefault.Kode_Jenis == "Jenis_Cuti" && TblDefault.Jenis_Isi != "Haji" && ((TblDefault.Keterangan == Jenis_Kelamin && TblDefault.Tambahan == Status_Nikah) || TblDefault.Keterangan == Agama || TblDefault.Keterangan == "Cuti_Khusus" || TblDefault.Keterangan == "Cuti_Tahunan" || ((TblDefault.Keterangan == null || TblDefault.Keterangan == Agama) && TblDefault.Tambahan == Status_Nikah))
                                         orderby TblDefault.Jenis_Isi ascending
                                         select new
                                         {
                                             Kode_Jenis = TblDefault.Kode_Jenis,
                                             Jenis_Isi = TblDefault.Jenis_Isi
                                         }).ToArray();
                    List<TT_Pengajuan> listcuti = new List<TT_Pengajuan>();
                    for (int i = 0; i < DataJenisCuti.Length; i++)
                    {
                        listcuti.Add(new TT_Pengajuan { ID_Cuti = DataJenisCuti[i].Kode_Jenis.ToString(), Jenis_Cuti = DataJenisCuti[i].Jenis_Isi.ToString() });
                    }

                    return listcuti.Distinct().ToList();
                }
            }
            else if (!string.IsNullOrEmpty(DataHJ) && !string.IsNullOrEmpty(DataUM))
            {
                var DataJenisCuti = (from TblDefault in DB.TM_Defaults
                                     where TblDefault.Kode_Jenis == "Jenis_Cuti" && TblDefault.Jenis_Isi != "Haji" && TblDefault.Jenis_Isi != "Umroh" && ((TblDefault.Keterangan == Jenis_Kelamin && TblDefault.Tambahan == Status_Nikah) || TblDefault.Keterangan == Agama || TblDefault.Keterangan == "Cuti_Khusus" || TblDefault.Keterangan == "Cuti_Tahunan" || ((TblDefault.Keterangan == null || TblDefault.Keterangan == Agama) && TblDefault.Tambahan == Status_Nikah))
                                     orderby TblDefault.Jenis_Isi ascending
                                     select new
                                     {
                                         ID_Jenis = TblDefault.ID_Jenis,
                                         Kode_Jenis = TblDefault.Kode_Jenis,
                                         Jenis_Isi = TblDefault.Jenis_Isi
                                     }).ToList();

            if (Jenis_Cuti == "1" || Jenis_Cuti == "3") // cuti tahunan
            {
                DataJenisCuti = DataJenisCuti.Where(x => x.ID_Jenis.Contains("CT")).ToList();
            }
            else if (Jenis_Cuti == "2")
            {
                DataJenisCuti = DataJenisCuti.Where(x => x.ID_Jenis.Contains("CK")).ToList();
            }
            else
            {
                switch (Jenis_Cuti)
                {
                    case "Cuti Tahunan":
                    case "Cuti Advance":
                        DataJenisCuti = DataJenisCuti.Where(x => x.ID_Jenis.Contains("CT")).ToList();
                        break;

                    default: 
                        DataJenisCuti = DataJenisCuti.Where(x => x.ID_Jenis.Contains("CK")).ToList();
                        break;

                }
            }

                List<TT_Pengajuan> listcuti = new List<TT_Pengajuan>();
                for (int i = 0; i < DataJenisCuti.Count; i++)
                {
                    listcuti.Add(new TT_Pengajuan { ID_Cuti = DataJenisCuti[i].Kode_Jenis.ToString(), Jenis_Cuti = DataJenisCuti[i].Jenis_Isi.ToString() });
                }

                return listcuti.Distinct().ToList();
            }

            var DataJenis = (from TblDefault in DB.TM_Defaults
                             where TblDefault.Kode_Jenis == "Jenis_Cuti" && ((TblDefault.Keterangan == Jenis_Kelamin && TblDefault.Tambahan == Status_Nikah) || TblDefault.Keterangan == Agama || TblDefault.Keterangan == "Cuti_Khusus" || TblDefault.Keterangan == "Cuti_Tahunan" || ((TblDefault.Keterangan == null || TblDefault.Keterangan == Agama) && TblDefault.Tambahan == Status_Nikah))
                             orderby TblDefault.Jenis_Isi ascending
                             select new
                             {
                                 ID_Jenis = TblDefault.ID_Jenis,
                                 Kode_Jenis = TblDefault.Kode_Jenis,
                                 Jenis_Isi = TblDefault.Jenis_Isi
                             }).ToList();

            if (Jenis_Cuti == "1" || Jenis_Cuti == "3") // cuti tahunan
            {
                DataJenis = DataJenis.Where(x => x.ID_Jenis.Contains("CT")).ToList();
            }
            else if (Jenis_Cuti == "2")
            {
                DataJenis = DataJenis.Where(x => x.ID_Jenis.Contains("CK")).ToList();
            }

            List<TT_Pengajuan> list = new List<TT_Pengajuan>();
            for (int i = 0; i < DataJenis.Count; i++)
            {
                list.Add(new TT_Pengajuan { ID_Cuti = DataJenis[i].Kode_Jenis.ToString(), Jenis_Cuti = DataJenis[i].Jenis_Isi.ToString() });
            }
            return list;
        }
        #endregion

        #region Data Riwayat Cuti
        public static IEnumerable<TM_Riwayat> GetRiwayat()
        {
            return (from Riwayat in DB.TM_Riwayats
                    select Riwayat).OrderBy(x => x.Tgl_Pengajuan).ThenBy(x => x.Tgl_Mulai).ToList();
        }
        #endregion

        #region Data Karyawan Staf
        public static IEnumerable<TM_Karyawan> GetStaff()
        {
            return (from Karyawan in DB.TM_Karyawans
                    where (Karyawan.Privilege == "Staff" && (Karyawan.Status_Karyawan != "Trainee" && Karyawan.Status_Karyawan != "Percobaan"))
                    orderby Karyawan.Nama_Karyawan
                    select Karyawan).ToList();
        }
        #endregion

        #region Data Karyawan Sudah KKWT
        public static List<DataKaryawan> GetKKWT()
        {
            // jalankan SP otomatis berisi data karyawan yang sudah KKWT

            var query = DB.Database.SqlQuery<DataKaryawan>("SR_Karyawan_KKWT").ToList();

            return query;
        }
        #endregion

        #region Chart Provider
        public static IEnumerable<TM_Riwayat_Pengajuan> GetRiwayatPengajuan()
        {
            using (var context = new HRISContext())
            {
                return context.TM_Riwayat_Pengajuans.ToList();
            }
        }

        public static IEnumerable<TM_Riwayat_User> GetRiwayatUser()
        {
            using (HRISContext context = new HRISContext())
            {
                return context.TM_Riwayat_Users.ToList();
            }
        }

        // enumerasi pada list
        /// <summary>
        /// Mendapatkan properti berkas yang dipilih pada explorer window.
        /// </summary>
        /// <returns>Properti berkas terpilih</returns>
        public static List<FileSystemItem> GetFile()
        {
            List<FileSystemItem> files = (List<FileSystemItem>)HttpContext.Current.Session["HRIS"];
            if (files == null)
            {
                files = (from TM_File_Manager in DB.TM_File_Managers
                         select new FileSystemItem
                         {
                             FileID = TM_File_Manager.FileID,
                             ParentID = TM_File_Manager.ParentID,
                             NamaFile = TM_File_Manager.NamaFile,
                             IsFolder = TM_File_Manager.IsFolder ?? false,
                             Data = TM_File_Manager.Data,
                             WriteTime = (DateTime)TM_File_Manager.WriteTime
                         }).ToList();
                HttpContext.Current.Session["HRIS"] = files;
            }
            return files;
        }

        // dapatkan seri ID berkas
        static int GetNewFileID()
        {
            IEnumerable<FileSystemItem> files = GetFile();
            int NomorFile = Convert.ToInt32(files.Last().FileID);
            return (files.Count() > 0) ? NomorFile++ : 0;
        }

        // fungsi pengunggahan berkas
        /// <summary>
        /// Fungsi pengunggahan berkas dari komputer client dengan parameter nama file yang diunggah.
        /// </summary>
        /// <param name="NewFile"></param>
        public static void InsertFile(FileSystemItem NewFile)
        {
            NewFile.FileID = GetNewFileID();
            GetFile().Add(NewFile);
        }

        // fungsi pemutakhiran berkas
        /// <summary>
        /// Fungsi pemutakhiran dari berkas yang sudah diunggah dan tersimpan pada sistem.
        /// </summary>
        /// <param name="File"></param>
        /// <param name="Update"></param>
        public static void UpdateFile(FileSystemItem File, Action<FileSystemItem> Update)
        {
            Update(File);
        }

        // fungsi penghapusan berkas
        /// <summary>
        /// Fungsi penghapusan berkas yang sudah ditentukan dengan parameter nama berkas terpilih.
        /// </summary>
        /// <param name="File"></param>
        public static void DelFile(FileSystemItem File)
        {
            if (File.IsFolder)
            {
                List<FileSystemItem> ChildDirectory = GetFile().FindAll(item => item.IsFolder && item.ParentID == File.FileID);
                if (ChildDirectory != null)
                {
                    foreach (FileSystemItem ChildDir in ChildDirectory)
                    {
                        DelFile(ChildDir);
                    }
                }
            }
            GetFile().Remove(File);
        }
        #endregion

        #region Data Jenis Pengumuman
        public static IEnumerable<TM_Default> GetJenisPengumuman()
        {
            return (from Default in DB.TM_Defaults
                    where Default.Kode_Jenis == "Pengumuman"
                    select Default).ToList();
        }
        #endregion

        #region Grafik Riwayat Pengajuan Cuti
        public static IEnumerable GetTahunRekap()
        {
            var selected = (from Riwayat in DB.TM_Riwayats
                            where Riwayat.Tgl_Pengajuan != null
                            orderby Riwayat.Tgl_Pengajuan descending
                            select new
                            {
                                Tahun_Cuti = Riwayat.Tgl_Pengajuan.Value.Year
                            }).Distinct().OrderByDescending(x => x.Tahun_Cuti).ToList();
            return selected;
        }
        #endregion

        #region Cetakan Bukti PMK
        public static IEnumerable<TM_List_PMK> GetNomorPMK()
        {
            return (from pmk in DB.TM_List_PMKs
                    select pmk).ToList();
        }

        public static IEnumerable<TM_List_PMK> GetNomorPMK(DateTime date)
        {
            return (from pmk in DB.TM_List_PMKs
                    where pmk.Tgl_PMK.Value.Year == date.Year && pmk.Tgl_PMK.Value.Month == date.Month
                    select pmk).ToList();
        }

        // Mendapatkan daftar nomor PMK yang sudah tercatat di tabel
        public static IEnumerable<PMKModel> GetDaftarPMK()
        {
            return (from pmk in DB.TM_List_PMKs
                    select new PMKModel
                    {
                        No_PMK = pmk.No_PMK
                    }).ToList();
        }
        #endregion

        #region Daftar Jenis Cuti
        public static List<TT_Pengajuan> GetListJenisCuti()
        {
            return new List<TT_Pengajuan>()
            {
                new TT_Pengajuan { ID_Cuti = "1", Jenis_Cuti = "Cuti Tahunan" },
                new TT_Pengajuan { ID_Cuti = "2", Jenis_Cuti = "Cuti Khusus" }, 
                new TT_Pengajuan { ID_Cuti = "3", Jenis_Cuti = "Cuti Advance" },
            };
        }

        public static List<TT_Pengajuan> GetListJenisCuti(DateTime? TglMasuk)
        {
            if (TglMasuk != null)
            {
                var selisih = (DateTime.Now - TglMasuk.Value).TotalDays;

                // satu tahun dihitung 365 hari, tidak termasuk tahun kabisat
                if (selisih >= 365)
                {
                    var list = GetListJenisCuti().Where(x => x.ID_Cuti != "3").ToList();

                    return list;
                }
                else
                {
                    var list = GetListJenisCuti().Where(x => x.ID_Cuti != "1").ToList();

                    return list;
                }
            }
            else
            {
                return GetListJenisCuti();
            }
        }
        #endregion

        #region Daftar Status WP
        public static List<PajakModel> GetStatusWP(string Status_Nikah = null)
        {
            var list = new List<PajakModel>();

            using (var DB = new HRISContext())
            {
                list = DB.TM_StatusWPs.Select(x => new PajakModel
                {
                    KodeStatus = x.KodeStatus,
                    StatusWP = x.StatusWP
                }).ToList();
            }

            if (!string.IsNullOrWhiteSpace(Status_Nikah) && Status_Nikah.Contains("Menikah"))
            {
                list = list.Where(x => x.KodeStatus.StartsWith("K")).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(Status_Nikah) && !Status_Nikah.Contains("Menikah"))
            {
                list = list.Where(x => x.KodeStatus.StartsWith("TK")).ToList();
            }

            return list;
        }
        #endregion

        #endregion

        #region Enumerasi Data Lainnya
        /*
         *  Enumerasi data pada bagian ini mengandalkan ketersediaan sumber data eksternal
         *  Jika sumber data tidak tersedia, lempar exception ke browser
         */

        /// <summary>
        /// Memperoleh data absensi karyawan.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ABSENSI> GetAbsensi()
        {
            return (from absen in ADB.ABSENSIs
                    select absen);
        }

        public static IEnumerable GetAbsensi(String nomor)
        {
            var karyawan = ADB.ABSENSIs.Where(x => x.PIN == nomor).FirstOrDefault();

            if (karyawan != null)
            {
                var query = (from kar in ADB.KARYAWANs
                             join abs in ADB.ABSENSIs
                             on kar.PIN equals abs.PIN
                             where kar.PIN == nomor
                             select new
                             {
                                 NIK = kar.NIK,
                                 Nama_Karyawan = kar.NAMA,
                                 Masuk = abs.masuk,
                                 Pulang = abs.pulang,
                                 Terlambat = abs.isTelat,
                                 Keterlambatan = abs.Terlambat,
                                 Shift = abs.Shift,
                                 Keterangan = abs.keterangan
                             }).ToList();

                return query;
            }
            else
            {
                return (from absen in ADB.ABSENSIs
                        select absen);
            }
        }

        /// <summary>
        /// Memperoleh data grup shift karyawan.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<GRUPSHIFT> GetGrupShift()
        {
            return (from grup in ADB.GRUPSHIFTs
                    select grup);
        }

        /// <summary>
        /// Memperoleh data shift kerja.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<SHIFT> GetShift()
        {
            return (from shift in ADB.SHIFTs
                    select shift);
        }

        /// <summary>
        /// Memperoleh data shift tetap karyawan.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<SHIFTTETAP> GetShiftTetap()
        {
            return (from tetap in ADB.SHIFTTETAPs
                    select tetap);
        }

        /// <summary>
        /// Memperoleh data shift tidak tetap.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<SHIFTTDKTETAP> GetShiftTdkTetap()
        {
            return (from tdk in ADB.SHIFTTDKTETAPs
                    select tdk);
        }
        #endregion

        #region IP Provider
        public static string GetServerIP()
        {
            //string ServerIP = (HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"] ?? String.Empty).ToString();

            //if (string.IsNullOrEmpty(ServerIP))
            //{
            //    ServerIP = HttpContext.Current.Request.UserHostAddress;
            //}

            string ServerIP = "54.169.241.244:1025";

            var host = Dns.GetHostEntry(Dns.GetHostName());

            //var address = (from ip in host.AddressList select ip).ToList();

            //foreach (var ips in address)
            //{
            //    if (ips == null)
            //        continue;

            //    if (IPAddress.IsLoopback(ips))
            //    {
            //        ServerIP = "localhost";
            //    }
            //    else if (ips.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //    {
            //        ServerIP = ips.ToString();
            //    }
            //    else
            //    {
            //        ServerIP = ips.ToString();
            //        // ServerIP = ips.MapToIPv4().ToString();
            //    }
            //}

            // tidak dipakai, hanya untuk client saja
            // HttpContext.Current.Request.UserHostAddress

            return ServerIP;
        }
        #endregion
    }

    #region Manajemen Berkas
    /// <summary>
    /// Kelas item berkas untuk sistem file.
    /// </summary>
    public class FileSystemItem
    {
        [Key]
        public int FileID { get; set; }     // indeks berkas
        public int? ParentID { get; set; }  // indeks parent directory
        public string NamaFile { get; set; }  // bisa juga berupa nama direktori
        public bool IsFolder { get; set; }  // kondisi uji direktori
        public Binary Data { get; set; } // isi data
        public DateTime? WriteTime { get; set; } // waktu penulisan berkas
    }
    #endregion

    #region File System Provider
    /// <summary>
    /// Kelas provider untuk sistem file.
    /// </summary>
    public class FileSystemProvider : FileSystemProviderBase
    {
        const int RootItemID = 1;
        string RootDirDisplayName;
        Dictionary<int, FileSystemItem> DirCache;

        // konstruktor kelas file system provider
        public FileSystemProvider(string RootDirectory)
            : base(RootDirectory)
        {
            RefreshDirectoryCache();
        }

        /* @Override */
        public override string RootFolderDisplayName { get { return RootDirDisplayName; } }

        // pembuatan kamus informasi cache

        /* @Override */
        public Dictionary<int, FileSystemItem> DirectoryCache { get { return DirCache; } }

        /* @Override */
        public override IEnumerable<FileManagerFile> GetFiles(FileManagerFolder folder)
        {
            FileSystemItem HRISDirItem = FindDirectoryItem(folder);
            return from HRISItem in Providers.GetFile()
                   where !HRISItem.IsFolder && HRISItem.ParentID == HRISDirItem.FileID
                   select new FileManagerFile(this, folder, HRISItem.NamaFile);
        }

        /* @Override */
        public override IEnumerable<FileManagerFolder> GetFolders(FileManagerFolder parentfolder)
        {
            FileSystemItem HRISDirItem = FindDirectoryItem(parentfolder);
            return from HRISItem in DirectoryCache.Values
                   where HRISItem.IsFolder && HRISItem.ParentID == HRISDirItem.FileID
                   select new FileManagerFolder(this, parentfolder, HRISItem.NamaFile);
        }

        /* @Override */
        public override bool Exists(FileManagerFile file)
        {
            return FindFileItem(file) != null;
        }

        /* @Override */
        public override bool Exists(FileManagerFolder folder)
        {
            return FindDirectoryItem(folder) != null;
        }

        /* @Override */
        public override Stream ReadFile(FileManagerFile file)
        {
            return new MemoryStream(FindFileItem(file).Data.ToArray());
        }

        /* @Override */
        public override DateTime GetLastWriteTime(FileManagerFile file)
        {
            var HRISFileItem = FindFileItem(file);
            return HRISFileItem.WriteTime.GetValueOrDefault(DateTime.Now);
        }

        /* @Override */
        public override long GetLength(FileManagerFile file)
        {
            var HRISFileItem = FindFileItem(file);
            return HRISFileItem.Data.Length;
        }

        /* => operasi manajemen file & direktori
         * jenis operasi:
         * 1. BuatDir (serupa mkdir DOS)
         * 2. GantiNamaFile (serupa ren DOS)
         * 3. GantiNamaDir (serupa ren DOS)
         * 4. PindahkanFile (serupa move DOS)
         * 5. PindahkanDir (serupa move DOS)
         * 6. UploadFile
         * 7. HapusFile (serupa del DOS)
         * 8. HapusDir (serupa rmdir DOS)
         */

        /* @Override */
        public override void CreateFolder(FileManagerFolder parent, string name)
        {
            Providers.InsertFile(new FileSystemItem
            {
                IsFolder = true,
                WriteTime = DateTime.Now,
                NamaFile = name,
                ParentID = FindDirectoryItem(parent).FileID
            });
            RefreshDirectoryCache();
        }

        /* @Override */
        public override void RenameFile(FileManagerFile file, string name)
        {
            Providers.UpdateFile(FindFileItem(file), HRISItem => HRISItem.NamaFile = name);
        }

        /* @Override */
        public override void RenameFolder(FileManagerFolder folder, string name)
        {
            Providers.UpdateFile(FindDirectoryItem(folder), HRISItem => HRISItem.NamaFile = name);
            RefreshDirectoryCache();
        }

        /* @Override */
        public override void MoveFile(FileManagerFile file, FileManagerFolder newParentFolder)
        {
            Providers.UpdateFile(FindFileItem(file), HRISItem => HRISItem.ParentID = FindDirectoryItem(newParentFolder).FileID);
        }

        /* @Override */
        public override void MoveFolder(FileManagerFolder folder, FileManagerFolder newParentFolder)
        {
            Providers.UpdateFile(FindDirectoryItem(folder), HRISItem => HRISItem.ParentID = FindDirectoryItem(newParentFolder).FileID);
            RefreshDirectoryCache();
        }

        /* @Override */
        public override void UploadFile(FileManagerFolder folder, string fileName, Stream content)
        {
            Providers.InsertFile(new FileSystemItem
            {
                IsFolder = false,
                WriteTime = DateTime.Now,
                NamaFile = fileName,
                ParentID = FindDirectoryItem(folder).FileID,
                Data = ReadAllBytes(content)
            });
        }

        /* @Override */
        public override void DeleteFile(FileManagerFile file)
        {
            Providers.DelFile(FindFileItem(file));
        }

        /* @Override */
        public override void DeleteFolder(FileManagerFolder folder)
        {
            Providers.DelFile(FindDirectoryItem(folder));
            RefreshDirectoryCache();
        }


        protected FileSystemItem FindFileItem(FileManagerFile file)
        {
            FileSystemItem DirItem = FindDirectoryItem(file.Folder);
            if (DirItem == null)
                return null;
            return Providers.GetFile().FindAll(item => item.ParentID == DirItem.FileID && !item.IsFolder && item.NamaFile == file.Name).FirstOrDefault();
        }

        protected FileSystemItem FindDirectoryItem(FileManagerFolder folder)
        {
            return (from DirItem in DirectoryCache.Values
                    where DirItem.IsFolder && GetRelativeName(DirItem) == folder.RelativeName
                    select DirItem).FirstOrDefault();
        }

        protected string GetRelativeName(FileSystemItem DirItem)
        {
            if (DirItem.FileID == RootItemID) return string.Empty;
            if (DirItem.ParentID == RootItemID) return DirItem.NamaFile;
            if (!DirectoryCache.ContainsKey((int)DirItem.ParentID)) return null;
            string filename = GetRelativeName(DirectoryCache[(int)DirItem.ParentID]);
            return filename == null ? null : Path.Combine(filename, DirItem.NamaFile);
        }

        /// <summary>
        /// Pembersihan cache direktori dari sistem manajemen berkas.
        /// </summary>
        protected void RefreshDirectoryCache()
        {
            this.DirCache = Providers.GetFile().FindAll(Item => Item.IsFolder).ToDictionary(Item => Item.FileID);
            this.RootDirDisplayName = (from DirItem in DirectoryCache.Values where DirItem.FileID == RootItemID select DirItem.NamaFile).First();
        }

        /// <summary>
        /// Fungsi pembacaan seluruh byte data dari stream data yang ada pada sistem.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Array Stream</returns>
        protected static byte[] ReadAllBytes(Stream stream)
        {
            var buffer = new byte[16 * 1024];
            int ReadCount;
            using (var ms = new MemoryStream())
            {
                while ((ReadCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, ReadCount);
                }
                return ms.ToArray();
            }
        }
    }
    #endregion

    #region JSON Provider
    public class GetJsonResult : System.Web.Http.IHttpActionResult
    {
        private string _jsonstring;

        public GetJsonResult(string jsonstring)
        {
            _jsonstring = jsonstring;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken token)
        {
            var content = new StringContent(_jsonstring);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            return Task.FromResult(response);
        }
    }
    #endregion

    #endregion

}