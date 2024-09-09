using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.CheckBoxList;


namespace SPD.Form
{
    public partial class Contact : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect("~/Form/Login.aspx");
                }
                    InitMode();
                    getvalue();
                    autotgl();
            }
        }

        
                    
                    
        protected void btnsimpan_Click(object sender, EventArgs e)
        {
            Newdata();
            //switch (lblMode.Text)
            //{

            //    case "NEW":
                    
            //        Newdata();
                   

            //        break;

            //    //case "EDIT":
            //    //    Editdata();
            //    //    break;


            //}
        }

        public void InitMode()
        {
            
            
        }

        SqlConnection con;
        public void setkoneksi()
        {
            string conakses = ConfigurationManager.ConnectionStrings["DB_Connect"].ConnectionString;
            con = new SqlConnection(conakses);
        }

        //static Random random = new Random();
        //public void autonumber()
        //{
        //    for (int i = 0; i < 2; i++)
        //    {
        //        txtNo.Text = "HRD" + (txtwaktu.Text = System.DateTime.Now.ToString("-yy-MM-")) + "FP-";
        //    }
        //}
        public void autotgl()
        {
            cmbtanggal.Date = System.DateTime.Now.Date;

        }

        public void Newdata()
        {
            
            setkoneksi();
                SqlCommand cmd = new SqlCommand("Login_Contact", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Selector", "Insert");
                cmd.Parameters.AddWithValue("@NIK", txtnik.Text.ToString());
                cmd.Parameters.AddWithValue("@Email_Perusahaan", txtemail.Text.ToString());
                cmd.Parameters.AddWithValue("@Nama_Karyawan", txtnama.Text.ToString());
                cmd.Parameters.AddWithValue("@Perusahaan", txtPT.Text.ToString());
                cmd.Parameters.AddWithValue("@Divisi", txtdivisi.Text.ToString());
                cmd.Parameters.AddWithValue("@Jabatan", txtjabatan.Text.ToString());
                cmd.Parameters.AddWithValue("@Tanggal", cmbtanggal.Date.ToString());
                cmd.Parameters.AddWithValue("@Pesan", txtpesan.Text.ToString());
                //cmd.Parameters.AddWithValue("@Nama_Karyawan", ASPxComboBox1.Text.ToString());
               
                
                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    cmd.ExecuteNonQuery();
                    /*
                    DialogResult dialogResult = MessageBox.Show("Pesan Terkirim", "Notice", MessageBoxButtons.OK);
                    if (dialogResult == DialogResult.OK)
                    {
                        Response.Redirect("~/Form/Home.aspx");
                    }
                    */
                }
                con.Close();
                
                



            }
           
        

        private void getvalue()
        {
            setkoneksi();
            con.Open();
            string No = Session["UserName"].ToString();
            SqlCommand cmd = new SqlCommand("select * from [172.16.110.116].[HRIS_Dev].[dbo].[TM_Karyawan] where NIK='" + No + "'", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                txtnama.Text = dr["Nama_Karyawan"].ToString();
                txtnik.Text = dr["NIK"].ToString();
                txtPT.Text = dr["Perusahaan"].ToString();
                txtjabatan.Text = dr["Jabatan"].ToString();
                txtdivisi.Text = dr["Departemen"].ToString();
                txtemail.Text = dr["Email_Perusahaan"].ToString();

            }
            dr.Close();
            con.Close();

        }
    }
}