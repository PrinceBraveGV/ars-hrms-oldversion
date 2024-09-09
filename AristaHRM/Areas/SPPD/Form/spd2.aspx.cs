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
    public partial class spd2 : System.Web.UI.Page
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
            //SqlCommand cmd = new SqlCommand("SelectData_SPPD2", con);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@Selector", "SelectData");

            //SqlDataAdapter da = new SqlDataAdapter(cmd);
            //DataSet ds = new DataSet();
            //da.Fill(ds);
            //datas.DataSource = ds;
            //datas.DataBind();
            setkoneksi();
            con.Open();
            //string Id = Request.QueryString["Id"].ToString().Trim();
            //SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2 WHERE Id='" + Id + "'", con);
            string No = Session["UserName"].ToString();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2 WHERE NIK='" + No + "' and Deleted = 'False'", con);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@Selector", "SelectData");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            datas.DataSource = ds;
            datas.DataBind();
        }

        protected void tambahsppd2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Form/InputSPPD2.aspx?Mode=NEW");
        }

        protected void klikedit(object sender, EventArgs e)
        {
            datas.Columns["Id"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object Id = datas.GetRowValues(datas.FocusedRowIndex, "Id");
            datas.Columns["No"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object No = datas.GetRowValues(datas.FocusedRowIndex, "No");

            Response.Redirect("~/Form/InputSPPD2.aspx?Mode=EDIT" + "&Id=" + Id.ToString().Trim() + "&No=" + No.ToString().Trim());

        }

        protected void klikprint(object sender, EventArgs e)
        {
            datas.Columns["Id"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            string Id = datas.GetRowValues(datas.FocusedRowIndex, "Id").ToString();
            Response.Redirect("~/Report/spd2.aspx?Id=" + Id);
        }

        protected void klikdelete(object sender, EventArgs e)
        {
            datas.Columns["Id"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object Id = datas.GetRowValues(datas.FocusedRowIndex, "Id");

            Response.Redirect("~/Form/InputSPPD2.aspx?Mode=DELETE" + "&Id=" + Id.ToString().Trim());

        }
    }
}