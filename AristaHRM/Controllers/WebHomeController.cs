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
    [RoutePrefix("Home")]
    public class WebHomeController : ApiController
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

        #region Login

        [HttpPost]
        public IHttpActionResult Login(string NIK, string Password, bool Remember)
        {
            var js = new JavaScriptSerializer();

            if (String.IsNullOrEmpty(NIK))
            {
                // nomor induk login tidak diisi
                var model = new ObjectModel()
                {
                    Id = "0",
                    Result = "Error",
                    Message = "Kesalahan proses login: NIK harus diisi.",
                    ThrownError = "Required Field: NIK"
                };

                Output = js.Serialize(model);
            }
            else
            {
                // pemeriksaan password
                if (String.IsNullOrEmpty(Password))
                {
                    var model = new ObjectModel()
                    {
                        Id = "0",
                        Result = "Error",
                        Message = "Kesalahan proses login: Password harus diisi.",
                        ThrownError = "Required Field: Password"
                    };

                    Output = js.Serialize(model);
                }
                else
                {
                    string inputPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "SHA1");

                    try
                    {
                        // query DB
                        var karyawan = DB.TM_Karyawans.FirstOrDefault(x => x.NIK == NIK);
                        if (!inputPassword.Equals(karyawan.Password))
                        {
                            // password tidak sesuai

                            var model = new ObjectModel()
                            {
                                Id = "0",
                                Result = "Error",
                                Message = "Kesalahan proses login: NIK atau password tidak sesuai.",
                                ThrownError = "Login: NIK/Password Mismatch"
                            };

                            Output = js.Serialize(model);
                        }
                        else
                        {
                            // setting cookie untuk sesi berjalan
                        }
                    }
                    catch (Exception e)
                    {
                        var model = new ObjectModel()
                        {
                            Id = e.HResult.ToString(),
                            Result = "Error",
                            Message = "Kesalahan pada proses login: '" + e.Message + "'",
                            InnerException = e.InnerException.Message ?? e.Message,
                            ThrownError = e.Message.ToString()
                        };

                        Output = js.Serialize(model);
                    }
                }
            }

            return new GetJsonResult(Output);
        }

        #endregion
    }
}
