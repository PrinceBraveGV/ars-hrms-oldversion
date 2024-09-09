using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace AristaHRM.Reports
{
    /// <summary>
    /// Laporan data cuti perseorangan.
    /// </summary>
    public partial class ReportCuti : DevExpress.XtraReports.UI.XtraReport
    {
        public ReportCuti()
        {
            InitializeComponent();
            this.Parameters["TglMulai"].Value = DateTime.Now.Date;
            this.Parameters["TglSelesai"].Value = DateTime.Now.Date;
        }

    }
}
