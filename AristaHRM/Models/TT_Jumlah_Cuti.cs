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
    using System.Collections.Generic;
    
    using System.ComponentModel.DataAnnotations;
    public partial class TT_Jumlah_Cuti
    {
        [Key]
        public string NIK { get; set; }
        public string Nama_Karyawan { get; set; }
        public string Perusahaan { get; set; }
        public string Cabang { get; set; }
        public string Jabatan { get; set; }
        public string Departemen { get; set; }
        public string Nama_Atasan { get; set; }
        public string Nama_Supervisor { get; set; }
        public Nullable<int> Jumlah_Pribadi { get; set; }
        public Nullable<int> Jumlah_Khusus { get; set; }
        public Nullable<int> Jumlah_Massal { get; set; }
        public Nullable<int> Jumlah_Hangus { get; set; }
        public Nullable<int> Tahun_Hangus { get; set; }
        public Nullable<int> Jumlah_Sisa { get; set; }
    }
}
