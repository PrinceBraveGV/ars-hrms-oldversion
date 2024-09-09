using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AristaHRM.Interfaces
{
    /// <summary>
    /// Implementasi interface untuk data perusahaan.
    /// </summary>
    public interface IPerusahaanRepository
    {
        string Kode_Perusahaan { get; set; }
        string Nama_Perusahaan { get; set; }
        string Kode_Singkat { get; set; }
    }
}
