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
    public partial class TM_Cuti_Hangus
    {
        [Key]
        public string ID_Catat { get; set; }
        public string NIK { get; set; }
        public Nullable<int> Jumlah_Hangus { get; set; }
        public Nullable<int> Tahun_Cuti { get; set; }
    }
}
