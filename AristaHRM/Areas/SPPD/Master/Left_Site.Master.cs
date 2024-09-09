using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SPD.Master
{
    public partial class Left_Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect("~/Form/Login.aspx");
                }
                lbldep.Text = Session["Privilege"].ToString();
                lbljabatan.Text = Session["Jabatan"].ToString();

                if (lbldep.Text == "Admin")
                { 
                    NavBar.Items.FindByName("UserSPD1").Visible = false;
                    NavBar.Items.FindByName("UserSPD2").Visible = false;
                }

                if (lbldep.Text == "Staff" || lbldep.Text == "Supervisor")
                {
                    NavBar.Groups.FindByName("Approval").Visible = false;
                    NavBar.Groups.FindByName("Report").Visible = false;
                    NavBar.Groups.FindByName("Master").Visible = false;
                    NavBar.Items.FindByName("AdminSPD1").Visible = false;
                    NavBar.Items.FindByName("AdminSPD2").Visible = false;
                }

                if (lbldep.Text == "Manager")
                {
                        NavBar.Groups.FindByName("Report").Visible = false;
                        NavBar.Groups.FindByName("Master").Visible = false;
                        NavBar.Items.FindByName("AdminSPD1").Visible = false;
                        NavBar.Items.FindByName("AdminSPD2").Visible = false;
                        NavBar.Items.FindByName("GMHRF1").Visible = false;
                        NavBar.Items.FindByName("HRMF1").Visible = false;
                    
                }
                if (lbljabatan.Text == "HRD Manager")
                {
                    NavBar.Groups.FindByName("Report").Visible = false;
                    NavBar.Groups.FindByName("Master").Visible = false;
                    NavBar.Items.FindByName("AdminSPD1").Visible = false;
                    NavBar.Items.FindByName("AdminSPD2").Visible = false;
                    NavBar.Items.FindByName("GMHRF1").Visible = false;
                }
                if (lbljabatan.Text == "General Manager")
                {
                    NavBar.Groups.FindByName("Report").Visible = false;
                    NavBar.Groups.FindByName("Master").Visible = false;
                    NavBar.Items.FindByName("AdminSPD1").Visible = false;
                    NavBar.Items.FindByName("AdminSPD2").Visible = false;
                    NavBar.Items.FindByName("HRMF1").Visible = false;
                }
                //if (lbldep.Text == "Staff" || lbldep.Text == "Supervisor")
                //{
                //    //ASPxMenu1.Items.RemoveAt(4);
                //    NavBar.Groups.RemoveAt(3);

                //}
                //if (lbldep.Text == "Staff" || lbldep.Text == "Supervisor")
                //{
                   
                //    NavBar.Groups.RemoveAt(2);
                //}
                //if (lbldep.Text == "Staff" || lbldep.Text == "Supervisor")
                //{

                //    NavBar.Groups.RemoveAt(3);
                //}

                //else if (lbldep.Text == "Manager")
                //{
                //    NavBar.Groups.RemoveAt(2);
                //    NavBar.Groups.RemoveAt(3);
                //}
            }

        }
    }
}