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
    [RoutePrefix("Manager")]
    public class WebManagerController : ApiController
    {
        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public string Output;

        #region Pengaturan Karyawan

        #region Daftar Karyawan
        public IHttpActionResult GetKaryawan(string NIK)
        {
            var js = new JavaScriptSerializer();

            using (var DB = new HRISContext())
            {
                var manager = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK);

                if (manager == null)
                {
                    var model = new ObjectModel()
                    {
                        Id = "0",
                        Result = "Error",
                        Message = "NIK harus diisi.",
                        ThrownError = "Required Field: NIK"
                    };

                    Output = js.Serialize(model);

                    return new GetJsonResult(Output);
                }
                else
                {
                    var karyawan = DB.TM_Karyawans.Where(x => x.Nama_Atasan == manager.Nama_Karyawan).ToList();
                    Output = js.Serialize(karyawan);
                    
                    return new GetJsonResult(Output);
                }
            }
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
                }
            }

            return new GetJsonResult(Output);
        }

        [HttpPost]
        public IHttpActionResult Pengajuan(string NIK)
        {
            var js = new JavaScriptSerializer();

            using (var DB = new HRISContext())
            {
                var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK);
                if (karyawan != null)
                {

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
                var model = new ObjectModel()
                {

                };

                Output = js.Serialize(model);

                return new GetJsonResult(Output);
            }

            using (var DB = new HRISContext())
            {
                try
                {

                }
                catch (Exception e)
                {
                    var model = new ObjectModel()
                    {
                        Id = e.HResult.ToString(),
                        Result = "Error",
                        Message = "",
                        InnerException = (e.InnerException.Message ?? String.Empty).ToString(),
                        ThrownError = (e.InnerException.Message ?? e.Message).ToString()
                    };
                }
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
                var model = new ObjectModel()
                {
                    Id = "0",
                    Result = "Error",
                    Message = "ID pengajuan harus tersedia untuk memproses persetujuan."
                };

                Output = js.Serialize(model);

                return new GetJsonResult(Output);
            }

            using (var DB = new HRISContext())
            {

            }

            return new GetJsonResult(Output);
        }
        #endregion

        #endregion

        #region Riwayat
        // GET: Riwayat
        public IHttpActionResult GetRiwayat(string NIK)
        {
            var js = new JavaScriptSerializer();

            // mekanisme memperoleh nama user aktif sedikit berbeda
            if (NIK == null)
            {
                var model = new ObjectModel()
                {
                    Id = "0",
                    Result = "Error",
                    Message = "ID karyawan harus tersedia untuk memproses riwayat."
                };

                Output = js.Serialize(model);
                return new GetJsonResult(Output);
            }

            using (var DB = new HRISContext())
            {
                var list = (from Riwayat in DB.TM_Riwayats
                         where (Riwayat.Nama_Karyawan == User.Identity.Name || Riwayat.Atasan == User.Identity.Name)
                         select Riwayat).ToList();

                Output = js.Serialize(list);
            }

            return new GetJsonResult(Output);
        }

        // POST: Riwayat
        [HttpPost]
        public IHttpActionResult EditRiwayat(string IdCuti)
        {
            var js = new JavaScriptSerializer();



            return new GetJsonResult(Output);
        }

        #endregion

    }
}
