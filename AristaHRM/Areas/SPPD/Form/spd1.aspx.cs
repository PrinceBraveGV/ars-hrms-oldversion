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
    public partial class spd1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect("~/Form/Login.aspx");
                }
                
            }
            LoadData2();
            
        }

        SqlConnection con;
        public void setkoneksi()
        {
            string conakses = ConfigurationManager.ConnectionStrings["DB_Connect"].ConnectionString;
            con = new SqlConnection(conakses);
        }

        public void LoadData2()
        {
            //setkoneksi();
            //SqlCommand cmd = new SqlCommand("SelectData_SPPD1", con);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@Selector", "SelectData");

            //SqlDataAdapter da = new SqlDataAdapter(cmd);
            //DataSet ds = new DataSet();
            //da.Fill(ds);
            //data.DataSource = ds;
            //data.DataBind();
            setkoneksi();
            con.Open();
            //string Id = Request.QueryString["Id"].ToString().Trim();
            //SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2 WHERE Id='" + Id + "'", con);
            string No = Session["UserName"].ToString();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F1 WHERE NIK='" + No + "' and Deleted = 'False'", con);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@Selector", "SelectData");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            data.DataSource = ds;
            data.DataBind();
        }

        protected void tambahsppd1_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Form/InputSPPD1.aspx?Mode=NEW");
        }
        protected void klikedit(object sender, EventArgs e)
        {
            data.Columns["Id"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object Id = data.GetRowValues(data.FocusedRowIndex, "Id");

            Response.Redirect("~/Form/InputSPPD1.aspx?Mode=EDIT" + "&Id=" + Id.ToString().Trim());
          
        }
        protected void klikdelete(object sender, EventArgs e)
        {
            data.Columns["Id"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object Id = data.GetRowValues(data.FocusedRowIndex, "Id");

            Response.Redirect("~/Form/InputSPPD1.aspx?Mode=DELETE" + "&Id=" + Id.ToString().Trim());

        }


        protected void klikprint(object sender, EventArgs e)
        {
            data.Columns["Id"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            string Id = data.GetRowValues(data.FocusedRowIndex, "Id").ToString();
            Response.Redirect("~/Report/spd1.aspx?Id=" + Id);
        }
     
        
    }
}