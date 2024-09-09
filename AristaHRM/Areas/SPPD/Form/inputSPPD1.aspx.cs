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
    public partial class inputSPPD1 : System.Web.UI.Page
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
                    getvalue();
                    //autotgl();
                    double a = 0;
                    txttransport.Text = a.ToString();
                    txtinap.Text = a.ToString();
                    txtmakan.Text = a.ToString();
                    txtsaku.Text = a.ToString();
                    txtlokal.Text = a.ToString();
                    //autonumber();
                    break;

                case "EDIT":
                    LoadEditDAta();
                    //autotgl();
                    break;

                case "DELETE":
                    /*
                    DialogResult dialogResult = MessageBox.Show("Delete Data ?", "Please Maku Sure", MessageBoxButtons.YesNo);
                    if(dialogResult == DialogResult.Yes)
                        {
                            Delete();
                        }
                        else if (dialogResult == DialogResult.No)
                        {
                            //do something else
                        }
                    
                    */
                    break;
            }

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
        //        txtNo.Text = "HRD" + (txtwaktu.Text = System.DateTime.Now.ToString("-yy-MM-")) + "FP-";
        //    }
        //}
        


        public void Newdata()
        {




            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SPPD_F1 WHERE No='" + txtNo.Text + "' ", con);
            int x = (int)cmd.ExecuteScalar();//Check record yg sama
            con.Close();
            if (x == 0)
            {
                cmd = new SqlCommand("Login_InUp", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Selector", "Insert");
                cmd.Parameters.AddWithValue("@No", txtNo.Text.ToString());
                cmd.Parameters.AddWithValue("@NIK", txtnik.Text.ToString());
                cmd.Parameters.AddWithValue("@Nama_Karyawan", txtnama.Text.ToString());
                cmd.Parameters.AddWithValue("@Nama_Atasan", txtatasan.Text.ToString());
                //cmd.Parameters.AddWithValue("@Nama_Karyawan", ASPxComboBox1.Text.ToString());
                cmd.Parameters.AddWithValue("@PT", txtPT.Text.ToString());
                cmd.Parameters.AddWithValue("@Jabatan", txtjabatan.Text.ToString());
                cmd.Parameters.AddWithValue("@Divisi", txtdivisi.Text.ToString());
                cmd.Parameters.AddWithValue("@Cabang", txtcabang.Text.ToString());
                cmd.Parameters.AddWithValue("@Kota_Tujuan", ASPxComboBox2.Text.ToString());
                cmd.Parameters.AddWithValue("@Keperluan", txtperlu.Text.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Pengajuan", cmbtglpengajuan.Date.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Pergi", cmbtglberangkat.Date.ToString());
                cmd.Parameters.AddWithValue("@Tanggal_Pulang", cmbtglpulang.Date.ToString());
                //cmd.Parameters.AddWithValue("@Transport", tipetransport.Text.ToString());
                cmd.Parameters.AddWithValue("@T_Pesawat", CheckBox1.Checked.ToString());
                cmd.Parameters.AddWithValue("@T_Kereta", CheckBox2.Checked.ToString());
                cmd.Parameters.AddWithValue("@T_Kapal", CheckBox3.Checked.ToString());
                cmd.Parameters.AddWithValue("@T_Bus", CheckBox4.Checked.ToString());
                cmd.Parameters.AddWithValue("@T_Opr", CheckBox5.Checked.ToString());
                cmd.Parameters.AddWithValue("@Biaya_Transport", txttransport.Text.ToString());
                cmd.Parameters.AddWithValue("@Biaya_Penginapan", txtinap.Text.ToString());
                cmd.Parameters.AddWithValue("@Biaya_Uang_Makan", txtmakan.Text.ToString());
                cmd.Parameters.AddWithValue("@Biaya_Uang_Saku", txtsaku.Text.ToString());
                cmd.Parameters.AddWithValue("@Biaya_Transport_Lokal", txtlokal.Text.ToString());
                cmd.Parameters.AddWithValue("@Total_Uang_Muka", txttotal.Text.ToString());
                cmd.Parameters.AddWithValue("@Maskapai_Penerbangan_Berangkat", txttiketpergi.Text.ToString());
                cmd.Parameters.AddWithValue("@Harga_Berangkat", txtharga1.Text.ToString());
                cmd.Parameters.AddWithValue("@Waktu_Berangkat", txtwaktu1.Text.ToString());
                cmd.Parameters.AddWithValue("@Maskapai_Penerbangan_Pulang", txttiketpulang.Text.ToString());
                cmd.Parameters.AddWithValue("@Harga_Pulang", txtharga2.Text.ToString());
                cmd.Parameters.AddWithValue("@Waktu_Pulang", txtwaktu2.Text.ToString());
                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();

                Response.Redirect("~/Form/spd1.aspx");



            }
            else
            {
                string MsgError = "NIK atau UserName tidak boleh sama dgn data yg sudah ada.";//Data yang di inputkan sudah ada
                lblError.Visible = true;
                lblError.Text = MsgError;
            }
        }



        private void Delete()
        {
            setkoneksi();
            con.Open();
            string Id = Request.QueryString["Id"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("Update SPPD_F1 Set Deleted='True' WHERE Id='" + Id + "'", con);
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();
            Response.Redirect("~/Form/spd1.aspx");

        }


        private void LoadEditDAta()
        {
            setkoneksi();
            con.Open();
            string Id = Request.QueryString["Id"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F1 WHERE Id='" + Id + "'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                txtNo.Text = dr["No"].ToString();
                txtnik.Value = dr["NIK"];
                txtnama.Text = dr["Nama_Karyawan"].ToString();
                txtatasan.Text = dr["Nama_Atasan"].ToString();
                //ASPxComboBox1.Text = dr["Nama_Karyawan"].ToString();
                txtPT.Text = dr["PT"].ToString();
                txtjabatan.Text = dr["Jabatan"].ToString();
                txtdivisi.Text = dr["Divisi"].ToString();
                txtcabang.Text = dr["Cabang"].ToString();
                ASPxComboBox2.Text = dr["Kota_Tujuan"].ToString();
                txtperlu.Text = dr["Keperluan"].ToString();
                //cmbtglpengajuan.Value = dr["Tanggal_Pengajuan"];
                cmbtglberangkat.Value = dr["Tanggal_Pergi"];
                cmbtglpulang.Value = dr["Tanggal_Pulang"];
                //tipetransport.Text = dr["Transport"].ToString();
                txttransport.Text = dr["Biaya_Transport"].ToString();
                CheckBox1.Checked = Convert.ToBoolean(dr["T_Pesawat"]);
                CheckBox2.Checked = Convert.ToBoolean(dr["T_Kereta"]);
                CheckBox3.Checked = Convert.ToBoolean(dr["T_Kapal"]);
                CheckBox4.Checked = Convert.ToBoolean(dr["T_Bus"]);
                CheckBox5.Checked = Convert.ToBoolean(dr["T_Opr"]);
                txtinap.Text = dr["Biaya_Penginapan"].ToString();
                txtsaku.Text = dr["Biaya_Uang_Saku"].ToString();
                txtmakan.Text = dr["Biaya_Uang_Makan"].ToString();
                txtlokal.Text = dr["Biaya_Transport_Lokal"].ToString();
                txttotal.Text = dr["Total_Uang_Muka"].ToString();
                txttiketpergi.Text = dr["Maskapai_Penerbangan_Berangkat"].ToString();
                txtharga1.Text = dr["Harga_Berangkat"].ToString();
                txtwaktu1.Text = dr["Waktu_Berangkat"].ToString();
                txttiketpulang.Text = dr["Maskapai_Penerbangan_Pulang"].ToString();
                txtharga2.Text = dr["Harga_Pulang"].ToString();
                txtwaktu2.Text = dr["Waktu_Pulang"].ToString();


            }
            dr.Close();
            con.Close();
        }

        public void Editdata()
        {

            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("Login_InUp", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Selector", "Update");
            cmd.Parameters.AddWithValue("@No", txtNo.Text.ToString());
            cmd.Parameters.AddWithValue("@NIK", txtnik.Text.ToString());
            cmd.Parameters.AddWithValue("@Nama_Karyawan", txtnama.Text.ToString());
            cmd.Parameters.AddWithValue("@Nama_Atasan", txtatasan.Text.ToString());
            //cmd.Parameters.AddWithValue("@Nama_Karyawan", ASPxComboBox1.Text.ToString());
            cmd.Parameters.AddWithValue("@Jabatan", txtjabatan.Text.ToString());
            cmd.Parameters.AddWithValue("@Divisi", txtdivisi.Text.ToString());
            cmd.Parameters.AddWithValue("@Cabang", txtcabang.Text.ToString());
            cmd.Parameters.AddWithValue("@PT", txtPT.Text.ToString());
            cmd.Parameters.AddWithValue("@Kota_Tujuan", ASPxComboBox2.Text.ToString());
            cmd.Parameters.AddWithValue("@Keperluan", txtperlu.Text.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Pengajuan", cmbtglpengajuan.Date.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Pergi", cmbtglberangkat.Date.ToString());
            cmd.Parameters.AddWithValue("@Tanggal_Pulang", cmbtglpulang.Date.ToString());
            //cmd.Parameters.AddWithValue("@Transport", tipetransport.Text.ToString());
            cmd.Parameters.AddWithValue("@T_Pesawat", CheckBox1.Checked.ToString());
            cmd.Parameters.AddWithValue("@T_Kereta", CheckBox2.Checked.ToString());
            cmd.Parameters.AddWithValue("@T_Kapal", CheckBox3.Checked.ToString());
            cmd.Parameters.AddWithValue("@T_Bus", CheckBox4.Checked.ToString());
            cmd.Parameters.AddWithValue("@T_Opr", CheckBox5.Checked.ToString());
            cmd.Parameters.AddWithValue("@Biaya_Transport", txttransport.Text.ToString());
            cmd.Parameters.AddWithValue("@Biaya_Penginapan", txtinap.Text.ToString());
            cmd.Parameters.AddWithValue("@Biaya_Uang_Saku", txtsaku.Text.ToString());
            cmd.Parameters.AddWithValue("@Biaya_Uang_Makan", txtmakan.Text.ToString());
            cmd.Parameters.AddWithValue("@Biaya_Transport_Lokal", txtlokal.Text.ToString());
            cmd.Parameters.AddWithValue("@Total_Uang_Muka", txttotal.Text.ToString());
            cmd.Parameters.AddWithValue("@Maskapai_Penerbangan_Berangkat", txttiketpergi.Text.ToString());
            cmd.Parameters.AddWithValue("@Harga_Berangkat", txtharga1.Text.ToString());
            cmd.Parameters.AddWithValue("@Waktu_Berangkat", txtwaktu1.Text.ToString());
            cmd.Parameters.AddWithValue("@Maskapai_Penerbangan_Pulang", txttiketpulang.Text.ToString());
            cmd.Parameters.AddWithValue("@Harga_Pulang", txtharga2.Text.ToString());
            cmd.Parameters.AddWithValue("@Waktu_Pulang", txtwaktu2.Text.ToString());
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();

            Response.Redirect("~/Form/spd1.aspx");
        }


        protected void btnsimpan_Click(object sender, EventArgs e)
        {
            switch (lblMode.Text)
            {

                case "NEW":
                    //DialogResult dialogResult = MessageBox.Show("Save", "Please Make Sure", MessageBoxButtons.YesNo);
                    //if(dialogResult == DialogResult.Yes)
                    //    {
                            Newdata();
                        //}
                        //else if (dialogResult == DialogResult.No)
                        //{
                        //    //do something else
                        //}
                   
                    break;

                case "EDIT":
                    Editdata();
                    break;


            }
        }

        private void getvalue()
        {
            //[172.16.110.116].[HRIS_Dev].[dbo].[TM_Karyawan]
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
                txtatasan.Text = dr["Nama_Atasan"].ToString();

            }
            dr.Close();
            con.Close();

        }

        protected void ASPxComboBox1_ValueChanged(object sender, EventArgs e)
        {
            getvalue();
        }

        protected void txttransport_TextChanged(object sender, EventArgs e)
        {
            double a, b, c, d, s, jumlah;

            a = double.Parse(txttransport.Text);
            b = double.Parse(txtmakan.Text);
            c = double.Parse(txtinap.Text);
            d = double.Parse(txtsaku.Text);
            s = double.Parse(txtlokal.Text);

            jumlah = a + b + c + d + s;

            txttotal.Text = jumlah.ToString();
        }

        protected void txtinap_TextChanged(object sender, EventArgs e)
        {
            double a, b, c, d, s, jumlah;

            a = double.Parse(txttransport.Text);
            b = double.Parse(txtmakan.Text);
            c = double.Parse(txtinap.Text);
            d = double.Parse(txtsaku.Text);
            s = double.Parse(txtlokal.Text);

            jumlah = a + b + c + d + s;

            txttotal.Text = jumlah.ToString();
        }
        protected void txtmakan_TextChanged(object sender, EventArgs e)
        {
            double a, b, c, d, s, jumlah;

            a = double.Parse(txttransport.Text);
            b = double.Parse(txtmakan.Text);
            c = double.Parse(txtinap.Text);
            d = double.Parse(txtsaku.Text);
            s = double.Parse(txtlokal.Text);

            jumlah = a + b + c + d + s;

            txttotal.Text = jumlah.ToString();
        }
        protected void txtsaku_TextChanged(object sender, EventArgs e)
        {
            double a, b, c, d, s, jumlah;

            a = double.Parse(txttransport.Text);
            b = double.Parse(txtmakan.Text);
            c = double.Parse(txtinap.Text);
            d = double.Parse(txtsaku.Text);
            s = double.Parse(txtlokal.Text);

            jumlah = a + b + c + d + s;

            txttotal.Text = jumlah.ToString();
        }
        protected void txtlokal_TextChanged(object sender, EventArgs e)
        {
            double a, b, c, d, s, jumlah;

            a = double.Parse(txttransport.Text);
            b = double.Parse(txtmakan.Text);
            c = double.Parse(txtinap.Text);
            d = double.Parse(txtsaku.Text);
            s = double.Parse(txtlokal.Text);

            jumlah = a + b + c + d + s;

            txttotal.Text = jumlah.ToString();
        }

        //public void autotgl()
        //{
        //    cmbtglpengajuan.Date = System.DateTime.Now.Date;

        //}
        
        
        protected void cmbtglberangkat_DateChange(object sender, EventArgs e)
        {

            getdate();

        }
        public void getdate()
        {
            DateTime tanggal1 = Convert.ToDateTime(cmbtglpengajuan.Text);
            cmbtglberangkat.MinDate = tanggal1;
        }
       
        protected void cmbtglpulang_DateChange(object sender, EventArgs e)
        {
            
            cmbtglpulang.MinDate = cmbtglberangkat.Date;

        }
        
        //protected void txttipe_TextChanged(object sender, EventArgs e)
        //{

        //    txttipe.Text = "";
        //    string selectedItems = String.Join(",",
        //     tipetransport.Items.OfType<ListItem>().Where(r => r.Selected)
        //    .Select(r => r.Text));
        //    txttipe.Text = selectedItems;  

        //}
        


    }
}