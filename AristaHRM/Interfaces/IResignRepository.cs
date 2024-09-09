using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AristaHRM.Interfaces
{
    public interface IResignRepository
    {
        string NIK { get; set; }
        string Nama_Karyawan { get; set; }
        string Perusahaan { get; set; }
        string Cabang { get; set; }
        string Jabatan { get; set; }
        string Departemen { get; set; }
        string Nama_Atasan { get; set; }
        string Nama_Supervisor { get; set; }
        DateTime Tgl_Resign { get; set; }
        string Alasan { get; set; }
        string Status_Resign { get; set; }
    }
}
