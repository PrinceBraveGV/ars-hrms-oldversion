using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AristaHRM
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        SqlCommand cmd;
        SqlDataReader dr;

        private static int totalusr = 0;
        private static int currentusr = 0;
        private static DateTime currentdate;

        public static int TotalUsr
        {
            get
            {
                return totalusr;
            }
        }

        public static int CurrentUsr
        {
            get
            {
                return currentusr;
            }
        }

        public static DateTime CurrentDate
        {
            get
            {
                return currentdate;
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas(); // register semua area selain default

            // bersihkan dulu semua view engine yang ada
            ViewEngines.Engines.Clear();

            // ingat: razor engine harus didahulukan dari web forms engine!!
            ViewEngines.Engines.Add(new RazorViewEngine());
            ViewEngines.Engines.Add(new WebFormViewEngine());

            // setelah memuat base engine razor & webforms, waktunya untuk memuat custom search pages
            ViewEngines.Engines.Add(new CustomRazorEngine());
            ViewEngines.Engines.Add(new CustomWebFormEngine());

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            
            ModelBinders.Binders.DefaultBinder = new DevExpress.Web.Mvc.DevExpressEditorsBinder();

            // nonaktifkan pemeriksaan user untuk AFT
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

            // update 231120: AWS hanya mendukung versi TLS 1.2 ke atas
            // sehingga protokol pengiriman harus disetel ke TLS 1.2 dst
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // bagian ini khusus untuk menonaktifkan validasi pada custom query yang dijalankan report sistem
            //DevExpress.DataAccess.Sql.SqlDataSource.AllowCustomSqlQueries = true;
            //DevExpress.DataAccess.Sql.SqlDataSource.DisableCustomQueryValidation = true;

            DevExpress.Web.ASPxWebControl.CallbackError += Application_Error;
        }

        protected void Application_Error(object sender, EventArgs e) 
        {
            Exception exception = System.Web.HttpContext.Current.Server.GetLastError();
            //TODO: Handle Exception

            string eventLog = "HRIS Event Log";
            string eventSource = "HRIS";

            string errorMessage = "";

            if (exception is HttpAntiForgeryException)
            {
                Response.Clear();
                Server.ClearError();
                Response.Redirect("~/Error/CSRFError"); // redirect ke halaman error
            }

            while (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
            {

                errorMessage += "Message\r\n" +
                    exception.Message.ToString() + "\r\n\r\n";
                errorMessage += "Source\r\n" +
                    exception.Source + "\r\n\r\n";
                errorMessage += "Target site\r\n" +
                    exception.TargetSite.ToString() + "\r\n\r\n";
                errorMessage += "Stack trace\r\n" +
                    exception.StackTrace + "\r\n\r\n";
                errorMessage += "ToString()\r\n\r\n" +
                    exception.ToString();

                exception = exception.InnerException;
            }

            EventLog.CreateEventSource(eventSource, eventLog);

            var systemLog = new EventLog(eventLog);
            systemLog.Source = eventSource;

            systemLog.WriteEntry("Error occurred: "
                                  + eventSource + "\r\n\r\n" + errorMessage,
                                  EventLogEntryType.Error);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // bagian blok hit counter dipindah ke session management
            // karena Application_Start tidak dapat menggunakan current session
            try
            {
                // Bagian inisialisasi hit counter
                string cmdstring = "SELECT * FROM TT_Hit_Counter";

                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand(cmdstring, conn);
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        totalusr = Convert.ToInt32(dr["Pengunjung"].ToString().Trim());
                        currentusr = Convert.ToInt32(dr["Sesi"].ToString().Trim());
                        if (dr["Tgl_Kunjungan"].ToString().Trim() != null)
                        {
                            currentdate = Convert.ToDateTime(dr["Tgl_Kunjungan"].ToString().Trim());
                        }
                        else
                        {
                            currentdate = DateTime.Now.Date;
                        }
                    }
                    else
                    {
                        totalusr = 0;
                        currentusr = 0;
                        currentdate = DateTime.Now.Date;
                    }

                    // ganti hari: current user reset ke 0
                    if (DateTime.Today.Subtract(currentdate).Days >= 1)
                    {
                        currentusr = 0;
                    }

                    // tutup koneksi DB
                    dr.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("~/Error/Index");
                // HttpContext.Current.Response.Redirect("~/Error/Index");
            }

            if (Session.IsNewSession)
            {
                totalusr++;
                currentusr++;
                currentdate = DateTime.Now.Date;
            }
            else
            {
                currentdate = DateTime.Now.Date;
            }

            // write user data
            try
            {
                using (var conn = new SqlConnection(connstring))
                {
                    cmd = new SqlCommand("SP_Counter", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Pengunjung", SqlDbType.NVarChar).Value = totalusr;
                    cmd.Parameters.Add("@Sesi", SqlDbType.NVarChar).Value = currentusr;
                    cmd.Parameters.Add("@Tgl_Kunjungan", SqlDbType.NVarChar).Value = string.Format("{0:yyyy-MM-dd}", currentdate.Date);
                    cmd.Parameters.Add("@Selector", SqlDbType.NVarChar).Value = "Update";
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (SqlException ex)
            {
                Session["ErrorMsg"] = "Terjadi kesalahan dalam proses data ke server.";
                // jika ada kesalahan di SQL Server
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            currentusr -= 1;
        }
    }
}