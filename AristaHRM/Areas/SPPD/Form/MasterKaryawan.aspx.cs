using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SPD.Form
{
    public partial class MasterKaryawan : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (Session["username"] == null)
                {
                    Response.Redirect("~/Form/Login.aspx");
                }
                LoadData();
            }
        }
        SqlConnection con;
        public void setkoneksi()
        {
            string conakses = ConfigurationManager.ConnectionStrings["DB_Connect"].ConnectionString;
            con = new SqlConnection(conakses);
        }

        public void LoadData()
        {
            setkoneksi();
            SqlCommand cmd = new SqlCommand("Login_SeLogin", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Selector", "Tampil");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            datakaryawan.DataSource = ds;
            datakaryawan.DataBind();
        }
        protected void newkaryawan_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Form/InputKaryawan.aspx?Mode=NEW");
        }
        protected void klikedit_Click(object sender, EventArgs e)
        {
            datakaryawan.Columns["id_karyawan"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object id_karyawan = datakaryawan.GetRowValues(datakaryawan.FocusedRowIndex, "id_karyawan");

            Response.Redirect("~/Form/InputKaryawan.aspx?Mode=UBAH" + "&id_karyawan=" + id_karyawan.ToString().Trim());

        }
        protected void klikdelete_Click(object sender, EventArgs e)
        {
            datakaryawan.Columns["id_karyawan"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object id_karyawan = datakaryawan.GetRowValues(datakaryawan.FocusedRowIndex, "id_karyawan");

            Response.Redirect("~/Form/InputKaryawan.aspx?Mode=DELETE" + "&id_karyawan=" + id_karyawan.ToString().Trim());

        }
    }
}