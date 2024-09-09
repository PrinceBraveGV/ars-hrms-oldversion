using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace AristaHRM.Reports
{
    /// <summary>
    /// Laporan data cuti kolektif/berkelompok.
    /// </summary>
    public partial class ReportRiwayat : DevExpress.XtraReports.UI.XtraReport
    {
        public ReportRiwayat()
        {
            InitializeComponent();
            this.Parameters["TglMulai"].Value = DateTime.Now.Date;
            this.Parameters["TglSelesai"].Value = DateTime.Now.Date;
        }

    }
}
