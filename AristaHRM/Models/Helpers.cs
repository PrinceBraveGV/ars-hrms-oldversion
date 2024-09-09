using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.UI;
using DevExpress.Web;
using DevExpress.Web.Mvc;

using boolean = System.Boolean;

namespace AristaHRM.Models
{
    public static class Helpers
    {
        #region Daftar Variabel

        /* Variabel database */
        // variabel database SQL Server
        // static HttpSessionStateBase session = new HttpSessionStateWrapper(HttpContext.Current.Session);
        public static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public static DateTime Tgl_Awal, Tgl_Akhir;
        public static int Counter, Total_Count;
        public static String FilePath = "~/Files/Import/";

        #endregion

        #region File Manager Helper
        public static readonly UploadControlValidationSettings ValidationSettings = new UploadControlValidationSettings
        {
            AllowedFileExtensions = new String[] { ".xls", ".xlsx", ".csv", ".mdb", ".accdb", ".txt" }
        };

        // Menangkap file yang diunggah ke server dan menampilkan alamat relatif file tsb (relative path).
        // Harap diingat: <input type="file" /> tidak dapat diisi dengan nama file dari model data
        // sehingga dibutuhkan satu label untuk menampilkan alamat relatif.
        public static void UploadComplete(Object s, FileUploadCompleteEventArgs e)
        {
            HttpSessionStateBase session = new HttpSessionStateWrapper(HttpContext.Current.Session);

            var filelist = ((MVCxUploadControl)s).UploadedFiles;
            if (filelist[0].IsValid && !String.IsNullOrEmpty(filelist[0].FileName))
            {
                String sumber = filelist[0].FileName;

                // hindari berkas yang mengandung whitespace dengan menyatukan nama file tsb tanpa spasi
                sumber = sumber.Replace(" ", String.Empty);
                String res = HttpContext.Current.Request.MapPath(FilePath + sumber);
                String ekstensi = Path.GetExtension(sumber);
                e.UploadedFile.SaveAs(res, true);
                IUrlResolutionService resolve = s as IUrlResolutionService;
                if (resolve != null)
                {
                    e.CallbackData = resolve.ResolveClientUrl(FilePath + sumber);
                }
            }
            else
            {
                session["Error"] = "Berkas tidak valid atau tidak sesuai format yang ditentukan.";
            }
        }

        #endregion

        #region Setting Email
        /// <summary>
        /// Metode helper pengiriman email untuk setting password akun.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="AlamatTarget"></param>
        public static void SendSettingPassword(String body, String AlamatTarget, String AlamatCMP, String AlamatPRI)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatTarget, AlamatTarget));
            // carbon copy jika ada 2 email
            if (!string.IsNullOrEmpty(AlamatCMP) && !string.IsNullOrEmpty(AlamatPRI))
            {
                EmailRequest.Bcc.Add(new MailAddress(AlamatPRI, AlamatPRI));
            }
            EmailRequest.Subject = "HRIS Arista Group - Permintaan Setting Password";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server, diambil dari email helper
            SMTPServer.Port = EmailHelper.EmailPort; // port default, diambil dari email helper
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server, diambil dari email helper
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT

            using (var sqlconn = new SqlConnection(connstring))
            {
                var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
                sqlconn.Open();
                var sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                        Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        if (sdr["Tgl_Setting"].ToString().Trim() != null)
                        {
                            Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
                        }
                        else
                        {
                            Tgl_Awal = DateTime.Now.Date;
                        }
                    }
                }
                else
                {
                    Total_Count = 0;
                    Counter = 0;
                    Tgl_Awal = DateTime.Now.Date;
                }
                sdr.Close();
                sqlconn.Close();

                if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
                {
                    Counter = 0;
                }

                // tambahkan 1 angka ke dalam counter
                Counter++;
                Total_Count++;

                cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
                cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
                cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                sqlconn.Open();
                cmd.ExecuteNonQuery();
                sqlconn.Close();
                // END EMAIL COUNT
            }

            SMTPServer.Send(EmailRequest);
        }

        /// <summary>
        /// Metode helper pengiriman email untuk reset password akun.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="AlamatTarget"></param>
        public static void SendReset(String body, String AlamatTarget)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatTarget));
            EmailRequest.Subject = "HRIS Arista Group - Permintaan Reset Password";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server, diambil dari email helper
            SMTPServer.Port = EmailHelper.EmailPort; // port default, diambil dari email helper
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server, diambil dari email helper
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT

            using (var sqlconn = new SqlConnection(connstring))
            {
                var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
                sqlconn.Open();
                var sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                        Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        if (sdr["Tgl_Setting"].ToString().Trim() != null)
                        {
                            Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
                        }
                        else
                        {
                            Tgl_Awal = DateTime.Now.Date;
                        }
                    }
                }
                else
                {
                    Total_Count = 0;
                    Counter = 0;
                    Tgl_Awal = DateTime.Now.Date;
                }
                sdr.Close();
                sqlconn.Close();

                if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
                {
                    Counter = 0;
                }

                // tambahkan 1 angka ke dalam counter
                Counter++;
                Total_Count++;

                cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
                cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
                cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                sqlconn.Open();
                cmd.ExecuteNonQuery();
                sqlconn.Close();
            }
            // END EMAIL COUNT

            SMTPServer.Send(EmailRequest);
        }

        public static void SendReset(String body, String AlamatTarget, String AlamatCMP, String AlamatPRI)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatTarget, AlamatTarget));
            // carbon copy jika ada 2 email
            if (!string.IsNullOrEmpty(AlamatCMP) && !string.IsNullOrEmpty(AlamatPRI))
            {
                EmailRequest.Bcc.Add(new MailAddress(AlamatPRI, AlamatPRI));
            }
            EmailRequest.Subject = "HRIS Arista Group - Permintaan Reset Password";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server, diambil dari email helper
            SMTPServer.Port = EmailHelper.EmailPort; // port default, diambil dari email helper
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server, diambil dari email helper
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT

            using (var sqlconn = new SqlConnection(connstring))
            {
                var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
                sqlconn.Open();
                var sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                        Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        if (!string.IsNullOrEmpty(sdr["Tgl_Setting"].ToString().Trim()))
                        {
                            Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
                        }
                        else
                        {
                            Tgl_Awal = DateTime.Now.Date;
                        }
                    }
                }
                else
                {
                    Total_Count = 0;
                    Counter = 0;
                    Tgl_Awal = DateTime.Now.Date;
                }
                sdr.Close();
                sqlconn.Close();

                if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
                {
                    Counter = 0;
                }

                // tambahkan 1 angka ke dalam counter
                Counter++;
                Total_Count++;

                cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
                cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
                cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                sqlconn.Open();
                cmd.ExecuteNonQuery();
                sqlconn.Close();
            }
            // END EMAIL COUNT

            SMTPServer.Send(EmailRequest);
        }

        /// <summary>
        /// Metode helper pengiriman email untuk pengajuan cuti.
        /// </summary>
        /// <param name="body">Body pesan yang dikirimkan.</param>
        /// <param name="AlamatAtasan">Alamat email atasan ybs.</param>
        /// <param name="AlamatSPV">Alamat email supervisor ybs.</param>
        public static void SendPengajuan(String body, String AlamatAtasan, String AlamatSPV, String AlamatAdvisor = null)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatAtasan, AlamatAtasan));
            if (!string.IsNullOrEmpty(AlamatSPV))
            {
                EmailRequest.CC.Add(new MailAddress(AlamatSPV, AlamatSPV));
            }
            if (!string.IsNullOrEmpty(AlamatAdvisor))
            {
                EmailRequest.CC.Add(new MailAddress(AlamatAdvisor, AlamatAdvisor));
            }
            EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Pengajuan Cuti";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server
            SMTPServer.Port = EmailHelper.EmailPort; // port default, bergantung kondisi
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            //using (var sqlconn = new SqlConnection(connstring))
            //{
            //    var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
            //    sqlconn.Open();
            //    var sdr = cmd.ExecuteReader();
            //    if (sdr.HasRows)
            //    {
            //        while (sdr.Read())
            //        {
            //            Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
            //            Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
            //            if (sdr["Tgl_Setting"].ToString().Trim() != null)
            //            {
            //                Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
            //            }
            //            else
            //            {
            //                Tgl_Awal = DateTime.Now.Date;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Total_Count = 0;
            //        Counter = 0;
            //        Tgl_Awal = DateTime.Now.Date;
            //    }
            //    sdr.Close();
            //    sqlconn.Close();

            //    if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
            //    {
            //        Counter = 0;
            //    }

            //    // tambahkan 1 angka ke dalam counter
            //    Counter++;
            //    Total_Count++;

            //    cmd = new SqlCommand("SP_Email_Counter", sqlconn);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
            //    cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
            //    cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
            //    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
            //    sqlconn.Open();
            //    cmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}
            // END EMAIL COUNT

            SMTPServer.Send(EmailRequest);
        }

        public static void SendPersetujuan(String body, String AlamatTarget, String AlamatAtasan, String AlamatCMP, String AlamatPRI, String AlamatSPV = null)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatCMP, AlamatCMP));
            EmailRequest.CC.Add(new MailAddress(AlamatAtasan, AlamatAtasan));

            if (!string.IsNullOrEmpty(AlamatSPV))
                EmailRequest.CC.Add(new MailAddress(AlamatSPV, AlamatSPV));

            if (!string.IsNullOrEmpty(AlamatCMP) && !string.IsNullOrEmpty(AlamatPRI))
                EmailRequest.Bcc.Add(new MailAddress(AlamatPRI, AlamatPRI));
            EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Persetujuan Cuti";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server
            SMTPServer.Port = EmailHelper.EmailPort; // port default, bergantung kondisi
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT
            //using (var sqlconn = new SqlConnection(connstring))
            //{
            //    var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
            //    sqlconn.Open();
            //    var sdr = cmd.ExecuteReader();
            //    if (sdr.HasRows)
            //    {
            //        while (sdr.Read())
            //        {
            //            Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
            //            Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
            //            if (sdr["Tgl_Setting"].ToString().Trim() != null)
            //            {
            //                Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
            //            }
            //            else
            //            {
            //                Tgl_Awal = DateTime.Now.Date;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Total_Count = 0;
            //        Counter = 0;
            //        Tgl_Awal = DateTime.Now.Date;
            //    }
            //    sdr.Close();
            //    sqlconn.Close();

            //    if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
            //    {
            //        Counter = 0;
            //    }

            //    // tambahkan 1 angka ke dalam counter
            //    Counter++;
            //    Total_Count++;

            //    cmd = new SqlCommand("SP_Email_Counter", sqlconn);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
            //    cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
            //    cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
            //    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
            //    sqlconn.Open();
            //    cmd.ExecuteNonQuery();
            //    sqlconn.Close();
            //}
            // END EMAIL COUNT

            SMTPServer.Send(EmailRequest);
        }

        /// <summary>
        /// Metode helper pengiriman email untuk konfirmasi cuti menunggu persetujuan.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="AlamatTarget"></param>
        public static void SendTunggu(String body, String AlamatTarget)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatTarget, AlamatTarget));
            EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Pengajuan Cuti";
            EmailRequest.Body = body;
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server
            SMTPServer.Port = EmailHelper.EmailPort; // port default, bergantung kondisi
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT

            using (var sqlconn = new SqlConnection(connstring))
            {
                var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
                sqlconn.Open();
                var sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                        Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        if (sdr["Tgl_Setting"].ToString().Trim() != null)
                        {
                            Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
                        }
                        else
                        {
                            Tgl_Awal = DateTime.Now.Date;
                        }
                    }
                }
                else
                {
                    Total_Count = 0;
                    Counter = 0;
                    Tgl_Awal = DateTime.Now.Date;
                }
                sdr.Close();
                sqlconn.Close();

                if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
                {
                    Counter = 0;
                }

                // tambahkan 1 angka ke dalam counter
                Counter++;
                Total_Count++;

                cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
                cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
                cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                sqlconn.Open();
                cmd.ExecuteNonQuery();
                sqlconn.Close();
            }
            // END EMAIL COUNT

            SMTPServer.Send(EmailRequest);
        }

        /// <summary>
        /// Metode helper pengiriman email untuk pembatalan cuti tahunan maupun cuti khusus.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="AlamatTarget"></param>
        /// <param name="AlamatAtasan"></param>
        /// <param name="AlamatCMP"></param>
        /// <param name="AlamatPRI"></param>
        public static void SendPembatalan(String body, String AlamatTarget, String AlamatAtasan, String AlamatCMP, String AlamatPRI)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatTarget, AlamatTarget));
            EmailRequest.CC.Add(new MailAddress(AlamatAtasan, AlamatAtasan));
            // carbon copy jika ada 2 email
            if (!string.IsNullOrEmpty(AlamatCMP) && !string.IsNullOrEmpty(AlamatPRI))
            {
                EmailRequest.Bcc.Add(new MailAddress(AlamatPRI, AlamatPRI));
            }
            EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Pembatalan Cuti";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server
            SMTPServer.Port = EmailHelper.EmailPort; // port default, bergantung kondisi
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT

            using (var sqlconn = new SqlConnection(connstring))
            {
                var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
                sqlconn.Open();
                var sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                        Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        if (sdr["Tgl_Setting"].ToString().Trim() != null)
                        {
                            Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
                        }
                        else
                        {
                            Tgl_Awal = DateTime.Now.Date;
                        }
                    }
                }
                else
                {
                    Total_Count = 0;
                    Counter = 0;
                    Tgl_Awal = DateTime.Now.Date;
                }
                sdr.Close();
                sqlconn.Close();

                if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
                {
                    Counter = 0;
                }

                // tambahkan 1 angka ke dalam counter
                Counter++;
                Total_Count++;

                cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
                cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
                cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                sqlconn.Open();
                cmd.ExecuteNonQuery();
                sqlconn.Close();
            }
            // END EMAIL COUNT

            SMTPServer.Send(EmailRequest);
        }

        /// <summary>
        /// Model helper pengiriman email untuk mutasi karyawan.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="AlamatTarget"></param>
        /// <param name="AlamatCMP"></param>
        /// <param name="AlamatPRI"></param>
        public static void SendMutasi(String body, String AlamatTarget, String AlamatCMP, String AlamatPRI)
        {
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatTarget, AlamatTarget));
            EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Mutasi Karyawan";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server
            SMTPServer.Port = EmailHelper.EmailPort; // port default, bergantung kondisi
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT

            using (var sqlconn = new SqlConnection(connstring))
            {
                var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
                sqlconn.Open();
                var sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                        Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        if (sdr["Tgl_Setting"].ToString().Trim() != null)
                        {
                            Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
                        }
                        else
                        {
                            Tgl_Awal = DateTime.Now.Date;
                        }
                    }
                }
                else
                {
                    Total_Count = 0;
                    Counter = 0;
                    Tgl_Awal = DateTime.Now.Date;
                }
                sdr.Close();
                sqlconn.Close();

                if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
                {
                    Counter = 0;
                }

                // tambahkan 1 angka ke dalam counter
                Counter++;
                Total_Count++;

                cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
                cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
                cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                sqlconn.Open();
                cmd.ExecuteNonQuery();
                sqlconn.Close();
                // END EMAIL COUNT
            }

            SMTPServer.Send(EmailRequest);
        }

        public static void SendResignation(String body, String AlamatTarget, String AlamatCMP, String AlamatPRI)
        {
            // membuat pesan email dengan body pesan terdefinisi s.d.a
            var EmailRequest = new MailMessage();
            EmailRequest.From = new MailAddress("cuti.arista@arista-group.co.id", "HRIS Arista Group"); // => isi dengan alamat email noreply
            EmailRequest.To.Add(new MailAddress(AlamatTarget, AlamatTarget));
            // carbon copy jika ada 2 email
            if (!string.IsNullOrEmpty(AlamatCMP) && !string.IsNullOrEmpty(AlamatPRI))
            {
                EmailRequest.Bcc.Add(new MailAddress(AlamatPRI, AlamatPRI));
            }
            EmailRequest.Subject = "HRIS Arista Group - Pemberitahuan Persetujuan Resign";
            EmailRequest.Body = body.ToString();
            EmailRequest.IsBodyHtml = true;

            // kirim menggunakan SMTP server
            //
            //  catatan untuk SMTP port: 
            //  - penggunaan umum = port 25, 587, 2525
            //
            var SMTPServer = new SmtpClient();
            SMTPServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SMTPServer.Host = EmailHelper.EmailHost; // isi dengan alamat atau IP server
            SMTPServer.Port = EmailHelper.EmailPort; // port default, bergantung kondisi
            SMTPServer.UseDefaultCredentials = false;
            SMTPServer.Credentials = EmailHelper.InfoUser; // isi dengan user ID & password mail server
            SMTPServer.EnableSsl = true; // default, jika server support SSL

            // BEGIN EMAIL COUNT

            using (var sqlconn = new SqlConnection(connstring))
            {
                var cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Select";
                sqlconn.Open();
                var sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Total_Count = Convert.ToInt32(sdr["Total_Email"].ToString());
                        Counter = Convert.ToInt32(sdr["Email_Harian"].ToString());
                        if (sdr["Tgl_Setting"].ToString().Trim() != null)
                        {
                            Tgl_Awal = Convert.ToDateTime(sdr["Tgl_Setting"].ToString().Trim());
                        }
                        else
                        {
                            Tgl_Awal = DateTime.Now.Date;
                        }
                    }
                }
                else
                {
                    Total_Count = 0;
                    Counter = 0;
                    Tgl_Awal = DateTime.Now.Date;
                }
                sdr.Close();
                sqlconn.Close();

                if (DateTime.Today.Subtract(Tgl_Awal.Date).Days > 0)
                {
                    Counter = 0;
                }

                // tambahkan 1 angka ke dalam counter
                Counter++;
                Total_Count++;

                cmd = new SqlCommand("SP_Email_Counter", sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Total_Email", SqlDbType.NVarChar).Value = Total_Count;
                cmd.Parameters.Add("@Email_Harian", SqlDbType.NVarChar).Value = Counter;
                cmd.Parameters.Add("@Tgl_Setting", SqlDbType.DateTime).Value = Convert.ToDateTime(Tgl_Awal.Date);
                cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                sqlconn.Open();
                cmd.ExecuteNonQuery();
                sqlconn.Close();
            }
            // END EMAIL COUNT

            SMTPServer.Send(EmailRequest);
        }
        #endregion

        #region Angka Terbilang
        /// <summary>
        /// Mengubah data angka menjadi kalimat.
        /// </summary>
        /// <param name="nominal">Nominal angka/uang yang diinput.</param>
        /// <returns>Bilangan angka dalam kalimat termasuk keterangan nilai tempat.</returns>
        public static String AngkaTerbilang(decimal nominal)
        {
            HttpSessionStateBase session = new HttpSessionStateWrapper(HttpContext.Current.Session);

            try
            {
                // bilangan yang memiliki 1 nomina
                String[] bilangan = {
                                    "",
                                    "Satu", // 1
                                    "Dua", // 2
                                    "Tiga", // 3
                                    "Empat", // 4
                                    "Lima", // 5
                                    "Enam", // 6
                                    "Tujuh", // 7
                                    "Delapan", // 8
                                    "Sembilan", // 9
                                    "Sepuluh", // 10
                                    "Sebelas" // 11
                                };
                String temp = "";

                decimal juta = Convert.ToDecimal(Math.Pow(10, 6));
                decimal miliar = Convert.ToDecimal(Math.Pow(10, 9));
                decimal triliun = Convert.ToDecimal(Math.Pow(10, 12));

                if (nominal < 12)
                {
                    temp = " " + bilangan[(long)nominal];
                }
                else if (nominal < 20)
                {
                    temp = " " + AngkaTerbilang(nominal - 10).ToString() + " Belas";
                }
                else if (nominal < 100)
                {
                    temp = AngkaTerbilang(nominal / 10) + " Puluh" + AngkaTerbilang(nominal % 10);
                }
                else if (nominal < 200)
                {
                    temp = " Seratus" + AngkaTerbilang(nominal - 100);
                }
                else if (nominal < 1000)
                {
                    temp = AngkaTerbilang(nominal / 100) + " Ratus" + AngkaTerbilang(nominal % 100);
                }
                else if (nominal < 2000)
                {
                    temp = " Seribu" + AngkaTerbilang(nominal - 1000);
                }
                else if (nominal < juta)
                {
                    temp = AngkaTerbilang(nominal / 1000) + " Ribu" + AngkaTerbilang(nominal % 1000);
                }
                else if (nominal < miliar)
                {
                    temp = AngkaTerbilang(nominal / juta) + " Juta" + AngkaTerbilang(nominal % juta);
                }
                else if (nominal < triliun)
                {
                    temp = AngkaTerbilang(nominal / miliar) + " Miliar" + AngkaTerbilang(nominal % miliar);
                }
                else if (nominal >= triliun)
                {
                    temp = AngkaTerbilang(nominal / triliun) + " Triliun" + AngkaTerbilang(nominal % triliun);
                }

                // kembalikan nilai terbilang dalam kalimat & buang semua spasi berlebihan
                temp = Regex.Replace(temp, @"\s+", " ", RegexOptions.Multiline);
                return temp;
            }
            catch (Exception e)
            {
                session["Error"] = "\"" + e.Message + "\"";
                return String.Empty;
            }
        }

        public static String SetTerbilang(decimal nominal)
        {
            String hasil = AngkaTerbilang(nominal).Trim();
            String matauang = "Rupiah";

            return hasil + " " + matauang;
        }
        #endregion

        #region Empty Element Checker Helper
        public static boolean IsEmptyArray<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return true;
            }
            else
            {
                return array.All(item => item == null);
            }
        } 
        #endregion

        #region Custom HTML Helper
        /// <summary>
        /// Custom HTML form editor.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="tagName">Nama tag yang dibuat.</param>
        /// <param name="name">Nama elemen yang muncul pada atribut 'id' atau 'name' dari tag.</param>
        /// <param name="value">Initial value dari editor.</param>
        /// <param name="type">Tipe dari editor (khusus tag input).</param>
        /// <param name="htmlAttributes">Tambahan atribut HTML yang tidak termasuk dalam standar helper.</param>
        /// <returns></returns>
        public static MvcHtmlString CustomEditor(this HtmlHelper html, String tagName, String name, String value = null, String type = null, Object htmlAttributes = null)
        {
            var builder = new TagBuilder(tagName);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("name", name);

            if (tagName == "input" && !String.IsNullOrEmpty(type))
            {
                builder.MergeAttribute("type", type);
            }

            if (!String.IsNullOrEmpty(value))
            {
                builder.MergeAttribute("value", value);
            }

            if (htmlAttributes != null)
            {
                var attr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                builder.MergeAttributes(attr);
            }

            var sb = new StringBuilder();

            if (tagName == "input")
            {
                sb.Append(builder.ToString(TagRenderMode.SelfClosing));
            }
            else
            {
                sb.Append(builder.ToString(TagRenderMode.StartTag));
                sb.Append(builder.ToString(TagRenderMode.EndTag));
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// Custom HTML form editor.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="html"></param>
        /// <param name="expression"></param>
        /// <param name="tagName">Nama tag yang dibuat.</param>
        /// <param name="type">Tipe dari editor (khusus tag input).</param>
        /// <param name="htmlAttributes">Tambahan atribut HTML yang tidak termasuk dalam standar helper.</param>
        /// <returns></returns>
        public static MvcHtmlString CustomEditorFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, String tagName, String type = null, Object htmlAttributes = null)
        {
            var data = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            String propertyName = data.PropertyName;

            var builder = new TagBuilder(tagName);
            builder.MergeAttribute("id", propertyName);
            builder.MergeAttribute("name", propertyName);

            if (tagName == "input" && !String.IsNullOrEmpty(type))
            {
                builder.MergeAttribute("type", type);
            }

            if (htmlAttributes != null)
            {
                var attr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                builder.MergeAttributes(attr);
            }

            var sb = new StringBuilder();

            if (tagName == "input")
            {
                sb.Append(builder.ToString(TagRenderMode.SelfClosing));
            }
            else
            {
                sb.Append(builder.ToString(TagRenderMode.StartTag));
                sb.Append(builder.ToString(TagRenderMode.EndTag));
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString DropDownListFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> selectList, IDictionary<string, object> htmlAttributes = null, bool editable = true)
        {
            if (!editable && htmlAttributes == null)
            {
                return html.DropDownListFor(expression, selectList, new { @disabled = "disabled" });
            }
            else if (!editable && htmlAttributes != null)
            {
                var attr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                attr.Add("disabled", "disabled");

                return html.DropDownListFor(expression, selectList, htmlAttributes);
            }

            return html.DropDownListFor(expression, selectList, htmlAttributes);
        }

        /// <summary>
        /// Custom HTML image helper.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="html"></param>
        /// <param name="expression"></param>
        /// <param name="path">Path sumber dari gambar.</param>
        /// <param name="alt">Teks alternatif untuk browser yang tidak menampilkan gambar.</param>
        /// <param name="htmlAttributes">Tambahan atribut HTML yang tidak termasuk dalam standar helper.</param>
        /// <returns></returns>
        public static MvcHtmlString ImageFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, String path, String alt = null, Object htmlAttributes = null)
        {
            var data = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            String propertyName = data.PropertyName;

            var builder = new TagBuilder("img");
            builder.MergeAttribute("id", propertyName);
            builder.MergeAttribute("src", path);
            builder.MergeAttribute("alt", alt);

            if (htmlAttributes != null)
            {
                var attr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                builder.MergeAttributes(attr);
            }

            var sb = new StringBuilder();

            sb.Append(builder.ToString(TagRenderMode.SelfClosing));
            return MvcHtmlString.Create(sb.ToString());
        }
        #endregion

        #region DB Helper
        public static string GenerateCommandText(string procedureName, SqlParameter[] parameters)
        {
            string commandText = "EXEC {0} {1}";
            var parameterNames = new List<string>();

            foreach (var pr in parameters)
            {
                parameterNames.Add(pr.ParameterName);
            }

            string output = string.Format(commandText, procedureName, string.Join(",", parameterNames.ToArray()));

            return output;
        }
        #endregion
    }
}