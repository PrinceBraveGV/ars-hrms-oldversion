//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AristaHRM.Models
{
    using System;
    
    public partial class SM_Riwayat_Result
    {
        public string ID_Cuti { get; set; }
        public string NIK { get; set; }
        public string Nama_Karyawan { get; set; }
        public string Jenis_Cuti { get; set; }
        public Nullable<System.DateTime> Tgl_Pengajuan { get; set; }
        public Nullable<System.DateTime> Tgl_Setuju { get; set; }
        public Nullable<System.DateTime> Tgl_Mulai { get; set; }
        public Nullable<System.DateTime> Tgl_Selesai { get; set; }
        public Nullable<int> Masa_Cuti { get; set; }
        public string Keperluan { get; set; }
        public string Pemberi { get; set; }
        public string Approved_By { get; set; }
        public string Status_Cuti { get; set; }
        public string Keterangan { get; set; }
        public Nullable<int> Tahun_Cuti { get; set; }
        public string Alasan { get; set; }
        public Nullable<int> Batal { get; set; }
        public string Alasan_Batal { get; set; }
        public string Cabang { get; set; }
        public string Atasan { get; set; }
        public string User_Login { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
