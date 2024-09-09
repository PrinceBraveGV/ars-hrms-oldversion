﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SPD.Report
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

            var q = Request.QueryString.Count;
            XtraReport4 r = new XtraReport4();
            if (q > 0)
            {
                var Id = Request.QueryString[0].ToString();
                r.Id.Value = Id;
                r.Id.Visible = false;
                ASPxDocumentViewer1.Report = r;
            }
            else
            {
                r.Id.Visible = true;
                ASPxDocumentViewer1.Report = r;
            }
        }
    }
}