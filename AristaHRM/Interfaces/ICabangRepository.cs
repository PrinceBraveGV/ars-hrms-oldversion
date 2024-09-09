using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AristaHRM.Interfaces
{
    /// <summary>
    /// Implementasi interface untuk data cabang.
    /// </summary>
    public interface ICabangRepository
    {
        string Kode_Cabang { get; set; }
        string Kode_Singkat { get; set; }
        string Nama_Perusahaan { get; set; }
        string Nama_Cabang { get; set; }
        string Kepala_Cabang { get; set; }
        string Lokasi { get; set; }
    }
}
