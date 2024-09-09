using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using AristaHRM.Models;

namespace AristaHRM.Models
{
    public class Import
    {
        // Remote connection string
        public static readonly String RemoteConnect = ConfigurationManager.ConnectionStrings["AbsenConnection"].ConnectionString;

        public SqlConnection conn;
        public SqlCommand cmd;

        public DataTable dt;

        /// <summary>
        /// Membuat koneksi ke database SQL Server dengan credential yang ditentukan.
        /// </summary>
        /// <param name="server">Nama server instance (data source).</param>
        /// <param name="dbName">Nama database (initial catalog).</param>
        /// <param name="userId">Nama user database.</param>
        /// <param name="password">Password untuk user ybs.</param>
        /// <returns></returns>
        public static String GenerateSqlConnection(String server, String dbName, String userId, String password)
        {

            var sqlbuilder = new SqlConnectionStringBuilder();

            sqlbuilder.DataSource = server;
            sqlbuilder.InitialCatalog = dbName;
            sqlbuilder.UserID = userId;
            sqlbuilder.Password = password;
            sqlbuilder.IntegratedSecurity = false;
            sqlbuilder.PersistSecurityInfo = true;

            return sqlbuilder.ToString();
        }

        /// <summary>
        /// Membuat string koneksi untuk pembuatan model pada Entity Framework.
        /// </summary>
        /// <param name="server">Nama server instance (data source).</param>
        /// <param name="dbName">Nama database (initial catalog).</param>
        /// <param name="userId">Nama user database.</param>
        /// <param name="password">Password untuk user ybs.</param>
        /// <param name="modelName">Nama model yang akan digunakan.</param>
        /// <returns></returns>
        public static String GenerateEntityConnection(String server, String dbName, String userId, String password, String modelName)
        {
            String provider = GenerateSqlConnection(server, dbName, userId, password);

            var entitybuilder = new EntityConnectionStringBuilder();

            entitybuilder.Provider = "System.Data.SqlClient";
            entitybuilder.ProviderConnectionString = provider;

            String metadata = @"res://*/" + modelName + @".csdl|res://*/" + modelName + @".ssdl|res://*/" + modelName + ".msl";

            entitybuilder.Metadata = metadata;

            return entitybuilder.ToString();
        }

        public static String GetSqlConnection(String connectionString)
        {
            // connection string harus memiliki parameter metadata (khusus EF)
            var builder = new EntityConnectionStringBuilder(connectionString);

            return builder.ConnectionString.ToString();
        }

        public static String AlterConnectionString(String connectionName, String newConnection)
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            ConnectionStringsSection section = config.GetSection("connectionStrings") as ConnectionStringsSection;

            if (section != null)
            {
                section.ConnectionStrings[connectionName].ConnectionString = newConnection;
                config.Save();
                ConfigurationManager.RefreshSection("connectionStrings");
            }

            return section.ConnectionStrings[connectionName].ConnectionString;
        }

        public static String AlterConnectionString(String connectionName, String server, String databaseName, String userId, String password, bool persistSecurityInfo = false, bool multipleActiveResultSets = false)
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            ConnectionStringsSection section = config.GetSection("connectionStrings") as ConnectionStringsSection;

            String newConnection = String.Empty;

            var sb = new StringBuilder();

            if (section != null)
            {
                sb.Append("Data Source=" + server + ";");
                sb.Append("Initial Catalog=" + databaseName + ";");
                sb.Append("User Id=" + userId +";");
                sb.Append("Password=" + password + ";");
                sb.Append("Persist Security Info=" + persistSecurityInfo.ToString() + ";");
                sb.Append("MultipleActiveResultSets=" + multipleActiveResultSets.ToString());

                newConnection = sb.ToString();

                section.ConnectionStrings[connectionName].ConnectionString = newConnection;
                config.Save();
                ConfigurationManager.RefreshSection("connectionStrings");
            }

            return section.ConnectionStrings[connectionName].ConnectionString;
        }
    }
}