using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AristaHRM.Areas.SPPD.Form
{
    public partial class CheckBoxView : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            DetailsView dv = (DetailsView)sender;
            bool test = (dv.Rows[0].Cells[0].Controls[0] as CheckBox).Checked;
            bool test2 = (dv.Rows[0].Cells[0].FindControl("ThingEnabled") as CheckBox).Checked;
        }
    }
}