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
    [RoutePrefix("Supervisor")]
    public class WebSupervisorController : ApiController
    {
        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public string Output;

        #region Pengaturan Karyawan
        public IHttpActionResult GetKaryawan(string NIK)
        {
            var js = new JavaScriptSerializer();

            using (var DB = new HRISContext())
            {
                var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK);
                if (karyawan != null)
                {
                    var listkaryawan = DB.TM_Karyawans.Where(x => x.Nama_Supervisor == karyawan.Nama_Karyawan).ToList();

                    Output = js.Serialize(listkaryawan);
                }
            }

            return new GetJsonResult(Output);
        }


        #endregion

        #region Pengaturan Cuti
        [HttpPost]
        public IHttpActionResult Pengajuan(string NIK, string jenis, string keperluan, string tgp, string tgm, string tgs, string ket = null, string lok = null, string kontak = null)
        {
            var js = new JavaScriptSerializer();

            var obj = new ObjectModel();

            if (String.IsNullOrEmpty(NIK) || String.IsNullOrEmpty(jenis) || String.IsNullOrEmpty(keperluan))
            {
                obj.Id = "0";
                obj.Result = "Error";
                obj.Message = "Isian tidak boleh kosong sebelum melakukan submit.";
                obj.ThrownError = "Required Field: NIK, Jenis Cuti, Keperluan";
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
                        model.Keperluan = keperluan;
                        model.Keterangan = ket ?? string.Empty;
                        model.Lokasi = lok ?? string.Empty;
                        model.Nomor_Kontak = kontak ?? string.Empty;

                        // gunakan SP pengisian data ke database

                        using (var conn = new SqlConnection(connstring))
                        {
                            using (var cmd = new SqlCommand("SP_Pengajuan", conn))
                            {
                                try
                                {
                                    conn.Open();
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("@NIK", SqlDbType.NVarChar).Value = model.NIK;
                                    cmd.Parameters.Add("@Nama_Karyawan", SqlDbType.NVarChar).Value = model.Nama_Karyawan;
                                    cmd.Parameters.AddWithValue("", "").DbType = DbType.String;
                                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Insert";
                                    cmd.ExecuteNonQuery();

                                    obj.Id = "1";
                                    obj.Result = "Sukses";
                                    obj.Message = "Data pengajuan telah disimpan.";
                                }
                                catch (SqlException e)
                                {
                                    obj.Id = e.HResult.ToString();
                                    obj.Result = "Error";
                                    obj.Message = "Kesalahan dalam pengisian data pengajuan: " + e.Message;
                                }
                            }
                        }

                        Output = js.Serialize(model);
                    }
                    else
                    {
                        obj = new ObjectModel()
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
                    obj = new ObjectModel()
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



    }
}
