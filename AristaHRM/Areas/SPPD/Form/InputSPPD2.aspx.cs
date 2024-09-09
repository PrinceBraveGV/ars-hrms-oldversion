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
    public partial class InputSPPD2 : System.Web.UI.Page
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
            else
            {
                txtNo.Text = (Session["nomor"] ?? String.Empty).ToString();
            }

        }
        public void Mode()
        {
            lblMode.Text = Request.QueryString["Mode"].ToString();

            switch (lblMode.Text)
            {

                case "NEW":
                    InitMode();
                    //autonumber();
                    getvalue();
                    autotgl();
                    btnsimpan.Enabled = true;
                    break;

                case "EDIT":
                    autotgl();
                    LoadEditDAta();
                    LoadData2();
                    
                    break;

                case "DELETE":
                    /*
                   DialogResult dialogResult = MessageBox.Show("Delete Data ?", "Please Maku Sure", MessageBoxButtons.YesNo);
                    if(dialogResult == DialogResult.Yes)
                        {
                            DeleteData();
                        }
                        else if (dialogResult == DialogResult.No)
                        {
                            //do something else
                        }
                    */
                    break;

            }

        }
        public void autotgl()
        {
            cmbtglpengajuan.Date = System.DateTime.Now.Date;

        }
        public void LoadData2()
        {
            setkoneksi();
            con.Open();
            //string Id = Request.QueryString["Id"].ToString().Trim();
            //SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2 WHERE Id='" + Id + "'", con);
            string No = Request.QueryString["No"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2_Detail WHERE No='" + No + "' and Deleted = 'False'", con);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@Selector", "SelectData");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            datadetail.DataSource = ds;
            datadetail.DataBind();
        }

        public void InitMode()
        {
            txtNo.Text = "";
            cmbtglberangkat.Text = "";
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
        //        txtNo.Text = "HRD" + (txtwaktu.Text = System.DateTime.Now.ToString("-yy-MM-")) + "FJ-";
        //    }
        //}
        public void Newdata()
        {


            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SPPD_F2 WHERE No='" + txtNo.Text + "' ", con);
            int x = (int)cmd.ExecuteScalar();//Check record yg sama
            con.Close();
            if (x == 0)
            {
                cmd = new SqlCommand("Login_InUp2", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Selector", "Insert");
                cmd.Parameters.AddWithValue("@NoReff", ASPxComboBox1.Text.ToString());
                cmd.Parameters.AddWithValue("@No", txtNo.Text.ToString());
                cmd.Parameters.AddWithValue("@NIK", txtnik.Text.ToString());
                cmd.Parameters.AddWithValue("@Nama_Karyawan", txtnama.Text.ToString());
                //cmd.Parameters.AddWithValue("@Nama_Karyawan", ASPxComboBox1.Text.ToString());
                cmd.Parameters.AddWithValue("@Divisi", txtdivisi.Text.ToString());
                cmd.Parameters.AddWithValue("@Jabatan", txtjabatan.Text.ToString());
                cmd.Parameters.AddWithValue("@PT", txtPT.Text.ToString());
                cmd.Parameters.AddWithValue("@Cabang", txtcabang.Text.ToString());
                cmd.Parameters.AddWithValue("@Jumlah_Hari", txtjumlahhari.Text.ToString());
                cmd.Parameters.AddWithValue("@Jenis_Perjalanan", tipeperjalanan.Text.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Pengajuan", cmbtglpengajuan.Date.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Pergi", cmbtglberangkat.Date.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Pulang", cmbtglpulang.Date.ToString());
                cmd.Parameters.AddWithValue("@Uang_Muka", txtuangmuka.Text.ToString());

                SqlParameter nomor_surat = new SqlParameter("@Generated", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(nomor_surat);

                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();
                //string No = Request.QueryString["No"].ToString().Trim();
                //Response.Redirect("~/Form/inputSPPD2.aspx?MODE=EDIT" + "&No=" + txtNo.Text);
                //LoadEditDAta();

                txtNo.Text = cmd.Parameters["@Generated"].Value.ToString(); // hasil parameter output nomor generated
                Session["nomor"] = txtNo.Text;

            }
            else
            {
                string MsgError = "NIK atau UserName tidak boleh sama dgn data yg sudah ada.";//Data yang di inputkan sudah ada
                lblError.Visible = true;
                lblError.Text = MsgError;
            }
        }

        private void DeleteData()
        {
            setkoneksi();
            con.Open();
            string Id = Request.QueryString["Id"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("Update SPPD_F2 Set Deleted='True' WHERE Id='" + Id + "'", con);
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();
            Response.Redirect("~/Form/spd2.aspx");

        }


        protected void simpan_Click(object sender, EventArgs e)
        {
            switch (lblMode.Text)
            {

                case "NEW":
                    /*
                    DialogResult dialogResult = MessageBox.Show("Data Sudah Sesuai ?", "Please Make Sure", MessageBoxButtons.YesNo);
                    if(dialogResult == DialogResult.Yes)
                    {
                        Newdata();
                        
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do something else
                    }
                    */

                    break;

                case "EDIT":
                    Editdata();
                    break;


            }
        }


       
        private void getvalue()
        {
            setkoneksi();
            con.Open();
            string No = Session["UserName"].ToString();
            SqlCommand cmd = new SqlCommand("select * from [TM_Karyawan] where NIK='" + No + "'", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                txtnama.Text = dr["Nama_Karyawan"].ToString();
                txtnik.Text = dr["NIK"].ToString();
                txtPT.Text = dr["Perusahaan"].ToString();
                txtjabatan.Text = dr["Jabatan"].ToString();
                txtdivisi.Text = dr["Departemen"].ToString();
                txtcabang.Text = dr["Cabang"].ToString();


            }
            dr.Close();
            con.Close();

        }

        

        protected void AddDetail_Click(object sender, EventArgs e)
        {
            //string No = Request.QueryString["No"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2 WHERE No='" + txtNo.Text + "'", con);
            Response.Redirect("~/Form/InputDetail.aspx?Mode=NEW" + "&No=" + txtNo.Text);
        }


        private void LoadEditDAta()
        {
            setkoneksi();
            con.Open();
            string Id = Request.QueryString["Id"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2 WHERE Id='" + Id + "'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                ASPxComboBox1.Text = dr["NoReff"].ToString();
                txtNo.Text = dr["No"].ToString();
                txtnik.Value = dr["NIK"];
                txtnama.Text = dr["Nama_Karyawan"].ToString();
                //ASPxComboBox1.Text = dr["Nama_Karyawan"].ToString();
                txtPT.Text = dr["PT"].ToString();
                txtjabatan.Text = dr["Jabatan"].ToString();
                txtdivisi.Text = dr["Divisi"].ToString();
                txtcabang.Text = dr["Cabang"].ToString();
                txtjumlahhari.Text = dr["Jumlah_Hari"].ToString();
                tipeperjalanan.Text = dr["Jenis_Perjalanan"].ToString();
                cmbtglberangkat.Value = dr["Tanggal_Pergi"];
                cmbtglpulang.Value = dr["Tanggal_Pulang"];
                txtuangmuka.Text = dr["Uang_Muka"].ToString();

            }
            dr.Close();
            con.Close();
        }

        public void Editdata()
        {

            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("Login_InUp2", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Selector", "Update");
            cmd.Parameters.AddWithValue("@NoReff", ASPxComboBox1.Text.ToString());
            cmd.Parameters.AddWithValue("@No", txtNo.Text.ToString());
            cmd.Parameters.AddWithValue("@NIK", txtnik.Text.ToString());
            cmd.Parameters.AddWithValue("@Nama_Karyawan", txtnama.Text.ToString());
            //cmd.Parameters.AddWithValue("@Nama_Karyawan", ASPxComboBox1.Text.ToString());
            cmd.Parameters.AddWithValue("@Divisi", txtdivisi.Text.ToString());
            cmd.Parameters.AddWithValue("@Jabatan", txtjabatan.Text.ToString());
            cmd.Parameters.AddWithValue("@PT", txtPT.Text.ToString());
            cmd.Parameters.AddWithValue("@Cabang", txtcabang.Text.ToString());
            cmd.Parameters.AddWithValue("@Jumlah_Hari", txtjumlahhari.Text.ToString());
            cmd.Parameters.AddWithValue("@Jenis_Perjalanan", tipeperjalanan.Text.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Pengajuan", cmbtglpengajuan.Date.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Pergi", cmbtglberangkat.Date.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Pulang", cmbtglpulang.Date.ToString());
            cmd.Parameters.AddWithValue("@Uang_Muka", txtuangmuka.Text.ToString());
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();

            Response.Redirect("~/Form/spd2.aspx");
        }

        protected void klikhapus(object sender, EventArgs e)
        {
            datadetail.Columns["Id_detail"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object Id_detail = datadetail.GetRowValues(datadetail.FocusedRowIndex, "Id_detail");

            Response.Redirect("~/Form/InputDetail.aspx?Mode=HAPUS" + "&Id_detail=" + Id_detail.ToString().Trim());

        }

        protected void klikedit(object sender, EventArgs e)
        {
            datadetail.Columns["Id_detail"].Visible = true; //visible dirubah ke true supaya dapet value ID_Record
            object Id_detail = datadetail.GetRowValues(datadetail.FocusedRowIndex, "Id_detail");

            Response.Redirect("~/Form/InputDetail.aspx?Mode=UBAH" + "&Id_detail=" + Id_detail.ToString().Trim());

        }

        protected void cmbtglpulang_DateChanged(object sender, EventArgs e)
        {
            hitunghari();
        }
       
        public void hitunghari()
        {
            DateTime tanggal1 = Convert.ToDateTime(cmbtglberangkat.Text);
            DateTime tanggal2 = Convert.ToDateTime(cmbtglpulang.Text).AddDays(+1);
            TimeSpan ts = new TimeSpan();
            ts = tanggal2 - tanggal1;
            txtjumlahhari.Text = ts.Days + " Hari";
        }

        //protected void txtNoReff_TextChanged(object sender, EventArgs e)
        //{
        //    getvaluef1();
        //}

        //private void getvaluef1()
        //{
        //    setkoneksi();
        //    con.Open();
        //    SqlCommand cmd = new SqlCommand("select * from SPPD_F1 where No='" + ASPxComboBox1.Text + "'", con);
        //    SqlDataAdapter sda = new SqlDataAdapter(cmd);
        //    SqlDataReader dr = cmd.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        txtnama.Text = dr["Nama_Karyawan"].ToString();
        //        //ASPxComboBox1.Text = dr["Nama_Karyawan"].ToString();
        //        txtnik.Text = dr["NIK"].ToString();
        //        txtPT.Text = dr["PT"].ToString();
        //        txtjabatan.Text = dr["Jabatan"].ToString();
        //        txtdivisi.Text = dr["Divisi"].ToString();
        //        txtcabang.Text = dr["Cabang"].ToString();
        //        cmbtglberangkat.Value = dr["Tanggal_Pergi"];
        //        cmbtglpulang.Value = dr["Tanggal_Pulang"];
        //        txtuangmuka.Text = dr["Total_Uang_Muka"].ToString();

        //    }
        //    hitunghari();
        //    dr.Close();
        //    con.Close();

        //}

        protected void ASPxComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            getNo();
        }
        private void getNo()
        {
            //[172.16.110.116].[HRIS_Dev].[dbo].[TM_Karyawan]
            setkoneksi();
            con.Open();

            //string Nama = Session["Nama"].ToString();
            SqlCommand cmd = new SqlCommand("select * from SPPD_F1 where No='" + ASPxComboBox1.Value + "' ", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                txtnama.Text = dr["Nama_Karyawan"].ToString();
                //ASPxComboBox1.Text = dr["Nama_Karyawan"].ToString();
                txtnik.Text = dr["NIK"].ToString();
                txtPT.Text = dr["PT"].ToString();
                txtjabatan.Text = dr["Jabatan"].ToString();
                txtdivisi.Text = dr["Divisi"].ToString();
                txtcabang.Text = dr["Cabang"].ToString();
                cmbtglberangkat.Value = dr["Tanggal_Pergi"];
                cmbtglpulang.Value = dr["Tanggal_Pulang"];
                txtuangmuka.Text = dr["Total_Uang_Muka"].ToString();

            }
            hitunghari();
            dr.Close();
            con.Close();

        }
        //protected void IncreaseBtn_Click(object sender, EventArgs e)
        //{
        //    var value = this.txtjumlahhari.Text;
        //    var intValue = 0;
        //    Int32.TryParse(value, out intValue);
        //    this.txtjumlahhari.Text = (++intValue).ToString();
        //}

        //protected void DecreaseBtn_Click(object sender, EventArgs e)
        //{
        //    var value = this.txtjumlahhari.Text;
        //    var intValue = 0;
        //    Int32.TryParse(value, out intValue);
        //    this.txtjumlahhari.Text = (--intValue).ToString();
        //}
        //private void bantuan_Click(object sender, System.EventArgs e)
        //{
        //    Form f = new Form();
        //    f.ShowDialog(this);
        //}
    }
}