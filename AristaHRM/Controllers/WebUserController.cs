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
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.Security;
using AristaHRM.Models;

namespace AristaHRM.Controllers
{
    public class WebUserController : ApiController
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
        public string Output;

        #endregion
        #endregion

        #region Pengaturan Karyawan

        #endregion

        #region Pengaturan Cuti

        #region Pengajuan Cuti
        public IHttpActionResult Pengajuan(string NIK, string jenis, string keperluan, string tgp, string tgm, string tgs, string ket = null, string lok = null, string kontak = null)
        {
            var js = new JavaScriptSerializer();

            if (String.IsNullOrEmpty(NIK))
            {
                var obj = new ObjectModel()
                {
                    Id = "0",
                    Result = "Error",
                    Message = "Isian NIK tidak boleh kosong sebelum melakukan submit.",
                    ThrownError = "Required Field: NIK"
                };
                Output = js.Serialize(obj);

                return new GetJsonResult(Output);
            }

            var model = new CutiModel();

            using (var DB = new HRISContext())
            {
                try
                {
                    var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK);
                    if (karyawan != null)
                    {
                        model.NIK = NIK;
                        model.Nama_Karyawan = karyawan.Nama_Karyawan;
                        model.Perusahaan = karyawan.Perusahaan;
                        model.Cabang = karyawan.Cabang;
                        model.Jabatan = karyawan.Jabatan;
                        model.Departemen = karyawan.Departemen;
                        model.Nama_Atasan = karyawan.Nama_Atasan;
                        model.Nama_Supervisor = karyawan.Nama_Supervisor;
                        model.Tgl_Pengajuan = DateTime.Parse(tgp);
                        model.Tgl_Mulai = DateTime.Parse(tgm);
                        model.Tgl_Selesai = DateTime.Parse(tgs);
                        model.Keterangan = ket ?? string.Empty;
                        model.Lokasi = lok ?? string.Empty;
                        model.Nomor_Kontak = kontak ?? string.Empty;

                        Output = js.Serialize(model);
                    }
                    else
                    {
                        var obj = new ObjectModel()
                        {
                            Id = "0",
                            Result = "Error",
                            Message = "Karyawan yang diminta tidak ditemukan.",
                            ThrownError = "No Such Item: Karyawan"
                        };
                        Output = js.Serialize(obj);

                        return new GetJsonResult(Output);
                    }
                }
                catch (Exception e)
                {
                    var obj = new ObjectModel()
                    {
                        Id = e.HResult.ToString(),
                        Result = "Error",
                        Message = "Kesalahan pada proses pengajuan cuti karyawan. Kode masalah: '" + e.Message + "'",
                        ThrownError = (e.InnerException.Message ?? e.Message).ToString()
                    };
                    Output = js.Serialize(obj);

                    return new GetJsonResult(Output);
                }
            }

            return new GetJsonResult(Output);
        }
        #endregion

        #region Persetujuan Cuti Khusus
        public IHttpActionResult ApprovalCK(String IdCuti)
        {
            var js = new JavaScriptSerializer();
            if (String.IsNullOrEmpty(IdCuti))
            {
                var obj = new ObjectModel()
                {
                    Id = "0",
                    Result = "Error",
                    Message = "Harap memilih satu pengajuan terlebih dahulu.",
                    ThrownError = "Required Field: ID_Cuti"
                };

                Output = js.Serialize(obj);

                return new GetJsonResult(Output);
            }

            var model = new CutiModel();
            using (var DB = new HRISContext())
            {
                var approval = DB.TT_Approval_K.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                if (approval != null)
                {
                    model.ID_Cuti = IdCuti;
                    model.NIK = approval.NIK;
                    model.Nama_Karyawan = approval.Nama_Karyawan;
                    model.Jenis_Cuti = "Cuti Khusus";
                    model.Keperluan = approval.Keperluan;
                    model.Keterangan = approval.Keterangan ?? String.Empty;
                }
            }

            Output = js.Serialize(model);

            return new GetJsonResult(Output);
        }
        #endregion

        #region Persetujuan Cuti Tahunan
        public IHttpActionResult ApprovalCT(String IdCuti)
        {
            var js = new JavaScriptSerializer();
            if (String.IsNullOrEmpty(IdCuti))
            {
                var obj = new ObjectModel()
                {
                    Id = "0",
                    Result = "Error",
                    Message = "Harap memilih satu pengajuan terlebih dahulu.",
                    ThrownError = "Required Field: ID_Cuti"
                };

                Output = js.Serialize(obj);

                return new GetJsonResult(Output);
            }

            var model = new CutiModel();
            using (var DB = new HRISContext())
            {
                var approval = DB.TT_Approval_T.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                if (approval != null)
                {
                    model.ID_Cuti = IdCuti;
                    model.NIK = approval.NIK;
                    model.Nama_Karyawan = approval.Nama_Karyawan;
                    model.Jenis_Cuti = "Cuti Tahunan";
                    model.Masa_Cuti = approval.Masa_Cuti.Value;
                    model.Keperluan = approval.Keperluan;
                }
            }

            return new GetJsonResult(Output);
        }
        #endregion

        #endregion

        #region Fungsi Lainnya

        #endregion
    }
}
