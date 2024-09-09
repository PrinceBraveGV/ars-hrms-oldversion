using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace SPD.Form
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Username"] = null;
          
        }
        SqlConnection con;
        public void setkoneksi()
        {
            string conakses = ConfigurationManager.ConnectionStrings["DB_Connect"].ConnectionString;
            con = new SqlConnection(conakses);
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
             //Login menggunakan Store Procedure
            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("Login_SeLogin", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Selector", "Select");
            cmd.Parameters.AddWithValue("@NIK", username.Text.ToString());   //for username 
            cmd.Parameters.AddWithValue("@Password", password.Text.ToString());
           // cmd.Parameters.AddWithValue("@Sebagai", cmbsebagai.Text.ToString()); //for password

            int usercount = (Int32)cmd.ExecuteScalar();// for taking single value

            if (usercount == 1)  // comparing users from table 
            {
                Session["Username"] = username.Text;
                Session["Nama"] = txtnama.Text;
                Session["Privilege"] = txtprivilege.Text;
                Session["Jabatan"] = txtjabatan.Text;
                //Session["Sebagai"] = cmbsebagai.Text;
                Response.Redirect("~/Form/Home.aspx");  //for sucsseful login
            }
            else
            {
                label.Visible = true; //for invalid login
            }
            con.Close();

        }

        protected void username_TextChanged(object sender, EventArgs e)
        {
            getvaluef1();
        }

        private void getvaluef1()
        {
            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("select * from TM_Karyawan where NIK='" + username.Text + "'", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                txtnama.Text = dr["Nama_Karyawan"].ToString();
                txtprivilege.Text = dr["Privilege"].ToString();
                txtjabatan.Text = dr["Jabatan"].ToString();
            }
           
            dr.Close();
            con.Close();

        }
    }
}