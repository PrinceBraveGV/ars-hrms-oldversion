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
    public partial class InputKaryawan : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect("~/Form/Login.aspx");
                }
                Mode();
            }
        }
        public void Mode()
        {
            lblMode.Text = Request.QueryString["Mode"].ToString();

            switch (lblMode.Text)
            {

                case "NEW":
                    InitMode();
                    break;

                case "UBAH":
                    LoadEditDAta();
                    break;

                case "DELETE":
                    DeleteData();
                    break;
            }

        }
        private void DeleteData()
        {
            setkoneksi();
            con.Open();
            string id_karyawan = Request.QueryString["id_karyawan"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("Update karyawan Set Deleted='True' WHERE id_karyawan='" + id_karyawan + "'", con);
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();
            Response.Redirect("~/Form/MasterKaryawan.aspx");

        }

        protected void btnsimpan_Click(object sender, EventArgs e)
        {
            switch (lblMode.Text)
            {

                case "NEW":
                    Newdata();
                    break;

                case "UBAH":
                    Editdata();
                    break;


            }
        }
        public void InitMode()
        {
            txtnik.Text = "";
        }

        SqlConnection con;
        public void setkoneksi()
        {
            string conakses = ConfigurationManager.ConnectionStrings["DB_Connect"].ConnectionString;
            con = new SqlConnection(conakses);
        }

        public void Newdata()
        {


            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM karyawan WHERE id_karyawan='" + id_karyawan + "' ", con);
            int x = (int)cmd.ExecuteScalar();//Check record yg sama
            con.Close();
            if (x == 0)
            {
                cmd = new SqlCommand("Master_Karyawan", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Selector", "Insert");
                cmd.Parameters.AddWithValue("@nik", txtnik.Text.ToString());
                cmd.Parameters.AddWithValue("@Nama", txtnama.Text.ToString());
                cmd.Parameters.AddWithValue("@Jabatan", txtjabatan.Text.ToString());
                cmd.Parameters.AddWithValue("@PT", txtPT.Text.ToString());
                cmd.Parameters.AddWithValue("@Cabang", txtcabang.Text.ToString());
                cmd.Parameters.AddWithValue("@Divisi", txtdivisi.Text.ToString());
                cmd.Parameters.AddWithValue("@Alamat", txtalamat.Text.ToString());
                cmd.Parameters.AddWithValue("@Agama", txtagama.Text.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Lahir", cmbtgllahir.Date.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Masuk", cmbtglmasuk.Date.ToString());
                cmd.Parameters.AddWithValue("@Jk", txtjk.Text.ToString());
                cmd.Parameters.AddWithValue("@Status", txtstatus.Text.ToString());
                cmd.Parameters.AddWithValue("@Atasan", txtatasan.Text.ToString());
                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();
                Response.Redirect("~/Form/MasterKaryawan.aspx");



            }
            else
            {
                string MsgError = "NIK atau UserName tidak boleh sama dgn data yg sudah ada.";//Data yang di inputkan sudah ada
                lblError.Visible = true;
                lblError.Text = MsgError;
            }
        }


        private void LoadEditDAta()
        {
            setkoneksi();
            con.Open();
            string id_karyawan = Request.QueryString["id_karyawan"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("SELECT * FROM karyawan WHERE id_karyawan='" + id_karyawan + "'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                txtnik.Value = dr["nik"];
                txtnama.Text = dr["Nama"].ToString();
                txtPT.Text = dr["PT"].ToString();
                txtjabatan.Text = dr["Jabatan"].ToString();
                txtdivisi.Text = dr["Divisi"].ToString();
                txtcabang.Text = dr["Cabang"].ToString();
                txtPT.Text = dr["PT"].ToString();
                txtalamat.Text = dr["Alamat"].ToString();
                txtagama.Text = dr["Agama"].ToString();
                cmbtgllahir.Value = dr["Tanggal_Lahir"];
                cmbtglmasuk.Value = dr["Tanggal_Masuk"];
                txtjk.Text = dr["Jk"].ToString();
                txtstatus.Text = dr["Status"].ToString();
                txtatasan.Text = dr["Atasan"].ToString();


            }
            dr.Close();
            con.Close();
        }

        public void Editdata()
        {

            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("Master_Karyawan", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Selector", "Update");
            cmd.Parameters.AddWithValue("@nik", txtnik.Text.ToString());
            cmd.Parameters.AddWithValue("@Nama", txtnama.Text.ToString());
            cmd.Parameters.AddWithValue("@Jabatan", txtjabatan.Text.ToString());
            cmd.Parameters.AddWithValue("@PT", txtPT.Text.ToString());
            cmd.Parameters.AddWithValue("@Cabang", txtcabang.Text.ToString());
            cmd.Parameters.AddWithValue("@Divisi", txtdivisi.Text.ToString());
            cmd.Parameters.AddWithValue("@Alamat", txtalamat.Text.ToString());
            cmd.Parameters.AddWithValue("@Agama", txtagama.Text.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Lahir", cmbtgllahir.Date.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Masuk", cmbtglmasuk.Date.ToString());
            cmd.Parameters.AddWithValue("@Jk", txtjk.Text.ToString());
            cmd.Parameters.AddWithValue("@Status", txtstatus.Text.ToString());
            cmd.Parameters.AddWithValue("@Atasan", txtatasan.Text.ToString());
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();

            Response.Redirect("~/Form/MasterKaryawan.aspx");
        }


        public string id_karyawan { get; set; }

        

       // public string btnbatal_Click(object sender, EventArgs e)
       // {
           
       //}

        
    }
}