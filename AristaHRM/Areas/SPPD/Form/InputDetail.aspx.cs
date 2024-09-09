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
    public partial class InputDetail : System.Web.UI.Page
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
                    LoadEditDAta();
                    LoadData2();
                    double a = 0;
                    txtbiaya.Text = a.ToString();
                    //txtmakan.Text = a.ToString();
                    //txtinap.Text = a.ToString();
                    //txtsaku.Text = a.ToString();
                    //txtlain.Text = a.ToString();

                    break;

                case "HAPUS":
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

                case "UBAH":
                    LoadEdit();
                    break;
            }

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
        }
        SqlConnection con;
        public void setkoneksi()
        {
            string conakses = ConfigurationManager.ConnectionStrings["DB_Connect"].ConnectionString;
            con = new SqlConnection(conakses);
        }
        private void LoadEditDAta()
        {
            setkoneksi();
            con.Open();
            string No = Request.QueryString["No"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2 WHERE No='" + No + "'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                txtNo.Text = dr["No"].ToString();

            }
            dr.Close();
            con.Close();
        }


        public void Newdata()
        {

            setkoneksi();
            //con.Open();
            SqlCommand cmd = new SqlCommand();
            //("SELECT COUNT(*) FROM SPPD_F2_Detail WHERE No='" + txtNo.Text + "' ", con);
            //int x = (int)cmd.ExecuteScalar();//Check record yg sama
            //con.Close();
            //if (x == 0)
            //{
            cmd = new SqlCommand("Login_Detail", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Selector", "Insert");
            cmd.Parameters.AddWithValue("@No", txtNo.Text.ToString());
            cmd.Parameters.AddWithValue("@Keterangan", txtketerangan.Text.ToString());
            cmd.Parameters.AddWithValue("@Tanggal", cmbtgl.Date.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Tranport", txttransport.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Penginapan", txtinap.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Makan", txtmakan.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Saku", txtsaku.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Lain", txtlain.Text.ToString());
            cmd.Parameters.AddWithValue("@Jenis_Biaya", ASPxComboBox1.Text.ToString());
            cmd.Parameters.AddWithValue("@Jumlah", txtjumlah.Text.ToString());


            //if (ASPxComboBox1.Value == "1")
            //{
            //    cmd.Parameters.AddWithValue("Biaya_Transport", txtbiaya.Text.ToString());
            //}
            if (ASPxComboBox1.Value == "1")
            {
                cmd.Parameters.AddWithValue("@Biaya_Tranport", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "2")
            {
                cmd.Parameters.AddWithValue("@Biaya_Penginapan", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "3")
            {
                cmd.Parameters.AddWithValue("@Biaya_Makan", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "4")
            {
                cmd.Parameters.AddWithValue("@Biaya_Saku", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "5")
            {
                cmd.Parameters.AddWithValue("@Biaya_Lain", txtbiaya.Text.ToString());
            }

            con.Open();
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();
            //Response.Redirect("~/Form/spd2.aspx");



            //}
            //else
            //{
            //    string MsgError = "NIK atau UserName tidak boleh sama dgn data yg sudah ada.";//Data yang di inputkan sudah ada
            //    lblError.Visible = true;
            //    lblError.Text = MsgError;
            //}
        }

        protected void btnsimpan_Click(object sender, EventArgs e)
        {
            switch (lblMode.Text)
            {

                case "NEW":
                    Newdata();
                    txtketerangan.Text = null;
                    LoadData2();
                    break;

                case "UBAH":
                    Editdata();
                    LoadData2();
                    break;


            }
        }
        private void LoadEdit()
        {
            setkoneksi();
            con.Open();
            string Id_detail = Request.QueryString["Id_detail"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("SELECT * FROM SPPD_F2_Detail WHERE Id_detail='" + Id_detail + "'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                txtId.Text = dr["Id_detail"].ToString();
                txtNo.Text = dr["No"].ToString();
                txtketerangan.Text = dr["Keterangan"].ToString();
                cmbtgl.Value = dr["Tanggal"];
                //txttransport.Text = dr["Biaya_Tranport"].ToString();
                //txtinap.Text = dr["Biaya_Penginapan"].ToString();
                //txtmakan.Text = dr["Biaya_Makan"].ToString();
                //txtsaku.Text = dr["Biaya_Saku"].ToString();
                //txtlain.Text = dr["Biaya_Lain"].ToString();
                ASPxComboBox1.Text = dr["Jenis_Biaya"].ToString();
                txtjumlah.Text = dr["Jumlah"].ToString();

            }
            dr.Close();
            con.Close();
        }

        public void Editdata()
        {

            setkoneksi();
            con.Open();
            SqlCommand cmd = new SqlCommand("Login_Detail", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Selector", "Update");
            cmd.Parameters.AddWithValue("@Id_detail", txtId.Text.ToString());
            cmd.Parameters.AddWithValue("@No", txtNo.Text.ToString());
            cmd.Parameters.AddWithValue("@Keterangan", txtketerangan.Text.ToString());
            cmd.Parameters.AddWithValue("@Tanggal", cmbtgl.Date.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Tranport", txttransport.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Penginapan", txtinap.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Makan", txtmakan.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Saku", txtsaku.Text.ToString());
            //cmd.Parameters.AddWithValue("@Biaya_Lain", txtlain.Text.ToString());
            cmd.Parameters.AddWithValue("@Jenis_Biaya", ASPxComboBox1.Text.ToString());
            cmd.Parameters.AddWithValue("@Jumlah", txtjumlah.Text.ToString());

            if (ASPxComboBox1.Value == "1")
            {
                cmd.Parameters.AddWithValue("@Biaya_Tranport", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "2")
            {
                cmd.Parameters.AddWithValue("@Biaya_Penginapan", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "3")
            {
                cmd.Parameters.AddWithValue("@Biaya_Makan", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "4")
            {
                cmd.Parameters.AddWithValue("@Biaya_Saku", txtbiaya.Text.ToString());
            }
            if (ASPxComboBox1.Value == "5")
            {
                cmd.Parameters.AddWithValue("@Biaya_Lain", txtbiaya.Text.ToString());
            }
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();

            Response.Redirect("~/Form/InputSPPD2.aspx?Mode=NEW");
        }


        private void DeleteData()
        {
            setkoneksi();
            con.Open();
            string Id_detail = Request.QueryString["Id_detail"].ToString().Trim();
            SqlCommand cmd = new SqlCommand("Update SPPD_F2_Detail Set Deleted='True' WHERE Id_detail='" + Id_detail + "'", con);
            if (con.State == ConnectionState.Open)
            {
                cmd.ExecuteNonQuery();
            }
            con.Close();
            Response.Redirect("~/Form/spd2.aspx");

        }

        protected void txtbiaya_TextChanged(object sender, EventArgs e)
        {
            double a, jumlah;

            a = double.Parse(txtbiaya.Text);


            jumlah = a;

            txtjumlah.Text = jumlah.ToString();
        }



        //protected void txtlain_TextChanged(object sender, EventArgs e)
        //{
        //    double a, b, c, d, s, jumlah;

        //    a = double.Parse(txttransport.Text);
        //    b = double.Parse(txtmakan.Text);
        //    c = double.Parse(txtinap.Text);
        //    d = double.Parse(txtsaku.Text);
        //    s = double.Parse(txtlain.Text);

        //    jumlah = a + b + c + d + s;

        //    txtjumlah.Text = jumlah.ToString();
        //}

        //protected void txttransport_TextChanged(object sender, EventArgs e)
        //{
        //    double a, b, c, d, s, jumlah;

        //    a = double.Parse(txttransport.Text);
        //    b = double.Parse(txtmakan.Text);
        //    c = double.Parse(txtinap.Text);
        //    d = double.Parse(txtsaku.Text);
        //    s = double.Parse(txtlain.Text);

        //    jumlah = a + b + c + d + s;

        //    txtjumlah.Text = jumlah.ToString();
        //}

        //protected void txtinap_TextChanged(object sender, EventArgs e)
        //{
        //    double a, b, c, d, s, jumlah;

        //    a = double.Parse(txttransport.Text);
        //    b = double.Parse(txtmakan.Text);
        //    c = double.Parse(txtinap.Text);
        //    d = double.Parse(txtsaku.Text);
        //    s = double.Parse(txtlain.Text);

        //    jumlah = a + b + c + d + s;

        //    txtjumlah.Text = jumlah.ToString();
        //}

        //protected void txtmakan_TextChanged(object sender, EventArgs e)
        //{
        //    double a, b, c, d, s, jumlah;

        //    a = double.Parse(txttransport.Text);
        //    b = double.Parse(txtmakan.Text);
        //    c = double.Parse(txtinap.Text);
        //    d = double.Parse(txtsaku.Text);
        //    s = double.Parse(txtlain.Text);

        //    jumlah = a + b + c + d + s;

        //    txtjumlah.Text = jumlah.ToString();
        //}

        //protected void txtsaku_TextChanged(object sender, EventArgs e)
        //{
        //    double a, b, c, d, s, jumlah;

        //    a = double.Parse(txttransport.Text);
        //    b = double.Parse(txtmakan.Text);
        //    c = double.Parse(txtinap.Text);
        //    d = double.Parse(txtsaku.Text);
        //    s = double.Parse(txtlain.Text);

        //    jumlah = a + b + c + d + s;

        //    txtjumlah.Text = jumlah.ToString();
        //}

        //protected void txttransport_TextChanged(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(txttransport.Text) && !string.IsNullOrEmpty(txtmakan.Text) && !string.IsNullOrEmpty(txtinap.Text) && !string.IsNullOrEmpty(txtlain.Text) && !string.IsNullOrEmpty(txtsaku.Text))
        //        txtjumlah.Text = (Convert.ToInt32(txttransport.Text) + Convert.ToInt32(txtmakan.Text) + Convert.ToInt32(txtinap.Text) + Convert.ToInt32(txtlain.Text) + Convert.ToInt32(txtsaku.Text)).ToString();
        //}

        //protected void txtinap_TextChanged(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(txttransport.Text) && !string.IsNullOrEmpty(txtmakan.Text) && !string.IsNullOrEmpty(txtinap.Text) && !string.IsNullOrEmpty(txtlain.Text) && !string.IsNullOrEmpty(txtsaku.Text))
        //        txtjumlah.Text = (Convert.ToInt32(txttransport.Text) + Convert.ToInt32(txtmakan.Text) + Convert.ToInt32(txtinap.Text) + Convert.ToInt32(txtlain.Text) + Convert.ToInt32(txtsaku.Text)).ToString();
        //}

        //protected void txtmakan_TextChanged(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(txttransport.Text) && !string.IsNullOrEmpty(txtmakan.Text) && !string.IsNullOrEmpty(txtinap.Text) && !string.IsNullOrEmpty(txtlain.Text) && !string.IsNullOrEmpty(txtsaku.Text))
        //        txtjumlah.Text = (Convert.ToInt32(txttransport.Text) + Convert.ToInt32(txtmakan.Text) + Convert.ToInt32(txtinap.Text) + Convert.ToInt32(txtlain.Text) + Convert.ToInt32(txtsaku.Text)).ToString();
        //}

        //protected void txtsaku_TextChanged(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(txttransport.Text) && !string.IsNullOrEmpty(txtmakan.Text) && !string.IsNullOrEmpty(txtinap.Text) && !string.IsNullOrEmpty(txtlain.Text) && !string.IsNullOrEmpty(txtsaku.Text))
        //        txtjumlah.Text = (Convert.ToInt32(txttransport.Text) + Convert.ToInt32(txtmakan.Text) + Convert.ToInt32(txtinap.Text) + Convert.ToInt32(txtlain.Text) + Convert.ToInt32(txtsaku.Text)).ToString();
        //}

        //protected void txtlain_TextChanged1(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(txttransport.Text) && !string.IsNullOrEmpty(txtmakan.Text) && !string.IsNullOrEmpty(txtinap.Text) && !string.IsNullOrEmpty(txtlain.Text) && !string.IsNullOrEmpty(txtsaku.Text))
        //        txtjumlah.Text = (Convert.ToInt32(txttransport.Text) + Convert.ToInt32(txtmakan.Text) + Convert.ToInt32(txtinap.Text) + Convert.ToInt32(txtlain.Text) + Convert.ToInt32(txtsaku.Text)).ToString();
        //}





    }
}