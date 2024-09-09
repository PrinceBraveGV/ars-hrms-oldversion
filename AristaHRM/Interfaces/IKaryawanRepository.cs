using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AristaHRM.Interfaces
{
    /// <summary>
    /// Implementasi interface untuk data karyawan.
    /// </summary>
    public interface IKaryawanRepository
    {
        string NIK { get; set; }
        string Nama_Karyawan { get; set; }
        string Jenis_Kelamin { get; set; }
        string Tempat_Lahir { get; set; }
        DateTime Tgl_Lahir { get; set; }
        string Provinsi { get; set; }
        string Kota { get; set; }
        string Alamat { get; set; }
        string Agama { get; set; }
        string Status_Nikah { get; set; }
        string Email { get; set; }
        string Email_Perusahaan { get; set; }
        string Perusahaan { get; set; }
        string Cabang { get; set; }
        string Jabatan { get; set; }
        string Departemen { get; set; }
        string Nama_Atasan { get; set; }
        string Nama_Supervisor { get; set; }
        string Nama_Advisor { get; set; }
        string Status_Karyawan { get; set; }
        DateTime Tgl_Masuk { get; set; }
        DateTime Tgl_Resign { get; set; }
        string Alasan { get; set; }
        string Privilege { get; set; }
        string Area_Kerja { get; set; }
        string Notes { get; set; }
        string NoteBaru { get; set; }
    }
}
