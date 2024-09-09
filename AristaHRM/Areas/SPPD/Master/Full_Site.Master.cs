using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SPD.Master
{
    public partial class Full_Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!this.IsPostBack)
            {
                if (Session["UserName"] == null)
                {
                    Response.Redirect("~/Form/Login.aspx");
                }
            }
            lblID.Text = Session["UserName"].ToString();
            ASPxLabel2.Text = Session["Nama"].ToString();
            ASPxLabel4.Text = Session["Privilege"].ToString();
            ASPxLabel5.Text = Session["Jabatan"].ToString();

            
        }
        SqlConnection con;
        public void setkoneksi()
        {
            string conakses = ConfigurationManager.ConnectionStrings["DB_Connect"].ConnectionString;
            con = new SqlConnection(conakses);
        }
       
        protected void ASPxButton1_Click(object sender, EventArgs e)
        {

        }
    }
}