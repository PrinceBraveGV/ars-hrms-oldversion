using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace AristaHRM.Reports
{
    public partial class ReportPMK : DevExpress.XtraReports.UI.XtraReport
    {
        public ReportPMK()
        {
            InitializeComponent();
        }

        public ReportPMK(string No_PMK, DateTime? Tgl_PMK, string Jenis_PMK)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(No_PMK))
            {
                this.Parameters["No_PMK"].Value = No_PMK;
            }

            if (Tgl_PMK != null)
            {
                this.Parameters["Tgl_PMK"].Value = Tgl_PMK.Value;
            }

            if (!string.IsNullOrEmpty(Jenis_PMK))
            {
                this.Parameters["Jenis_PMK"].Value = Jenis_PMK;
            }
        }

    }
}
