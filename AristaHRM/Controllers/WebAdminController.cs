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
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.Security;
using AristaHRM.Models;

namespace AristaHRM.Controllers
{
    [RoutePrefix("Admin")]
    public class WebAdminController : ApiController
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

        #region Daftar Karyawan
        // GET: Karyawan
        public IHttpActionResult GetKaryawan()
        {
            var js = new JavaScriptSerializer();

            using (var DB = new HRISContext())
            {
                var karyawan = js.Serialize(DB.TM_Karyawans.Select(x => x).ToList());
                return new GetJsonResult(karyawan);
            }
        }

        public IHttpActionResult GetKaryawanByNIK(string NIK)
        {
            var js = new JavaScriptSerializer();

            var karyawan = js.Serialize(DB.TM_Karyawans.Where(x => x.NIK == NIK).FirstOrDefault());
            return new GetJsonResult(karyawan);
        }
        #endregion

        #endregion

        #region Pengaturan Cuti

        #region Pengajuan
        // GET: List Pengajuan
        public IHttpActionResult GetJenisCuti(string NIK)
        {
            var js = new JavaScriptSerializer();

            using (var DB = new HRISContext())
            {
                var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK);
                if (karyawan != null)
                {
                    String jk = karyawan.Jenis_Kelamin;
                    String agama = karyawan.Agama;

                    var query = DB.TM_Defaults.Where(x => x.Keterangan == agama);

                    Output = js.Serialize(query);
                }
            }

            return new GetJsonResult(Output);
        }

        // POST: Pengajuan
        [HttpPost]
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

                        // tambahkan pengajuan ke database



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
                        InnerException = (e.InnerException.Message ?? String.Empty).ToString(),
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
        [HttpPost]
        public IHttpActionResult ApprovalCK(string IdCuti)
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
                // SELECT Riwayat
                var approval = DB.TM_Riwayats.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                if (approval != null)
                {
                    model.ID_Cuti = IdCuti;
                    model.NIK = approval.NIK;
                    model.Nama_Karyawan = approval.Nama_Karyawan;
                    model.Jenis_Cuti = approval.Jenis_Cuti;
                    model.Keperluan = approval.Keperluan;
                    model.Keterangan = approval.Keterangan ?? String.Empty;
                    model.Status_Cuti = "Disetujui";
                    model.Tgl_Pengajuan = approval.Tgl_Pengajuan.Value;
                    model.Tgl_Mulai = approval.Tgl_Mulai.Value;
                    model.Tgl_Selesai = approval.Tgl_Selesai.Value;

                    approval.Status_Cuti = "Disetujui";

                    // hapus dari daftar persetujuan
                    var record = DB.TT_Approval_K.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                    DB.TT_Approval_K.Remove(record);

                    DB.SaveChanges();

                    // pengiriman email

                    var message = new MailMessage();
                    // message.Attachments.Add(new Attachment(Path.GetFileName(@"drive:\path_to_folder"), System.Net.Mime.MediaTypeNames.Application.Pdf));


                }
            }

            Output = js.Serialize(model);

            return new GetJsonResult(Output);
        }

        [HttpPost]
        public IHttpActionResult SetujuCK(String[] arg)
        {
            var js = new JavaScriptSerializer();

            foreach (String idc in arg)
            {

            }

            return new GetJsonResult(Output);
        }
        #endregion

        #region Persetujuan Cuti Tahunan
        [HttpPost]
        public IHttpActionResult ApprovalCT(string IdCuti)
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
                var approval = DB.TM_Riwayats.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                if (approval != null)
                {
                    model.ID_Cuti = IdCuti;
                    model.NIK = approval.NIK;
                    model.Nama_Karyawan = approval.Nama_Karyawan;
                    model.Jenis_Cuti = "Cuti Tahunan";
                    model.Masa_Cuti = approval.Masa_Cuti.Value;
                    model.Keperluan = approval.Keperluan;
                    model.Keterangan = approval.Keterangan ?? String.Empty;

                    approval.Status_Cuti = "Disetujui";

                    // hapus dari daftar persetujuan
                    var record = DB.TT_Approval_T.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                    DB.TT_Approval_T.Remove(record);

                    DB.SaveChanges();
                }
            }

            return new GetJsonResult(Output);
        }
        #endregion



        #region Riwayat
        // GET: Riwayat
        public IHttpActionResult GetRiwayat()
        {
            var js = new JavaScriptSerializer();

            using (var DB = new HRISContext())
            {
                try
                {
                    var riwayat = DB.TM_Riwayats.Select(x => x).ToList();
                    Output = js.Serialize(riwayat);
                }
                catch (Exception e)
                {
                    var obj = new ObjectModel()
                    {
                        Id = e.HResult.ToString(),
                        Result = "Error",
                        Message = "Kesalahan pada proses pemuatan riwayat. Kode masalah: '" + e.Message + "'",
                        ThrownError = (e.InnerException.Message ?? e.Message).ToString()
                    };
                    Output = js.Serialize(obj);

                    return new GetJsonResult(Output);
                }
            }

            return new GetJsonResult(Output);
        }

        // POST: Riwayat
        [HttpPost]
        public IHttpActionResult EditRiwayat(string IdCuti, string TglMulai, string TglSelesai)
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

            using (var DB = new HRISContext())
            {
                try
                {
                    var daftar = DB.TM_Riwayats.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                    if (daftar == null)
                    {
                        var obj = new ObjectModel()
                        {
                            Id = "0",
                            Result = "Error",
                            Message = "Tidak ada data riwayat pengajuan dengan nomor ID tsb.",
                            ThrownError = "Data Unavailable: ID_Cuti"
                        };

                        Output = js.Serialize(obj);

                        return new GetJsonResult(Output);
                    }
                    else
                    {
                        DateTime mulai = DateTime.Parse(TglMulai);
                        DateTime selesai = DateTime.Parse(TglSelesai);

                        // hanya 3 parameter yang diubah: tanggal mulai, tanggal selesai & masa berlaku
                        daftar.Tgl_Mulai = mulai;
                        daftar.Tgl_Selesai = selesai;

                        daftar.Masa_Cuti = (mulai == selesai) ? 1 : (int)(mulai - selesai).TotalDays + 1;

                        DB.SaveChanges();

                        Output = js.Serialize(daftar);
                    }
                }
                catch (Exception e)
                {
                    var obj = new ObjectModel()
                    {
                        Id = e.HResult.ToString(),
                        Result = "Error",
                        Message = "Kesalahan pada proses edit riwayat. Kode masalah: '" + e.Message + "'",
                        ThrownError = (e.InnerException.Message ?? e.Message).ToString()
                    };
                    Output = js.Serialize(obj);

                    return new GetJsonResult(Output);
                }
            }

            return new GetJsonResult(Output);
        }

        [HttpPost]
        public IHttpActionResult DelRiwayat(String IdCuti)
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

            using (var DB = new HRISContext())
            {
                try
                {
                    var record = DB.TM_Riwayats.FirstOrDefault(x => x.ID_Cuti == IdCuti);
                    if (record == null)
                    {
                        var obj = new ObjectModel()
                        {
                            Id = "0",
                            Result = "Error",
                            Message = "Tidak ada data riwayat pengajuan dengan nomor ID tsb.",
                            ThrownError = "Data Unavailable: ID_Cuti"
                        };

                        Output = js.Serialize(obj);

                        return new GetJsonResult(Output);
                    }
                    else
                    {
                        DB.TM_Riwayats.Remove(record);
                        DB.SaveChanges();

                        Output = js.Serialize(DB.TM_Riwayats.Select(x => x).ToList());
                    }
                }
                catch (Exception e)
                {
                    var obj = new ObjectModel()
                    {
                        Id = e.HResult.ToString(),
                        Result = "Error",
                        Message = "Kesalahan pada proses edit riwayat. Kode masalah: '" + e.Message + "'",
                        ThrownError = (e.InnerException.Message ?? e.Message).ToString()
                    };
                    Output = js.Serialize(obj);

                    return new GetJsonResult(Output);
                }
            }

            return new GetJsonResult(Output);
        }
        #endregion

        #endregion

        #region Fungsi Lainnya
        [HttpPost]
        public IHttpActionResult UploadFile(HttpPostedFileBase file)
        {
            var js = new JavaScriptSerializer();

            if (file == null)
            {
                var msg = new ObjectModel()
                {
                    Id = "0",
                    Result = "Error",
                    Message = "Tidak ada berkas yang diunggah"
                };

                Output = js.Serialize(msg);

                return new GetJsonResult(Output);
            }
            else
            {
                var obj = new ObjectModel()
                {

                };

                Output = js.Serialize(obj);
            }

            return new GetJsonResult(Output);
        }

        #endregion
    }
}
