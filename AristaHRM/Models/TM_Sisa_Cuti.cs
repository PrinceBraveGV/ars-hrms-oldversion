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
    public partial class TM_Sisa_Cuti
    {
        [Key]
        public string ID_Sisa { get; set; }
        public string NIK { get; set; }
        public string Nama_Karyawan { get; set; }
        public Nullable<int> Sisa_Cuti { get; set; }
        public Nullable<int> Tahun_Cuti { get; set; }
    }
}
