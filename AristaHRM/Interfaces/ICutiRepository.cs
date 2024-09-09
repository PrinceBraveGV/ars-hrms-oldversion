using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AristaHRM.Interfaces
{
    /// <summary>
    /// Implementasi interface untuk pengajuan cuti karyawan.
    /// </summary>
    public interface ICutiRepository
    {
        string ID_Cuti { get; set; }
        string NIK { get; set; }
        string Nama_Karyawan { get; set; }
        string Jenis_Kelamin { get; set; }
        string Agama { get; set; }
        string Status_Nikah { get; set; }
        string Perusahaan { get; set; }
        string Cabang { get; set; }
        string Jabatan { get; set; }
        string Departemen { get; set; }
        string Nama_Atasan { get; set; }
        string Nama_Supervisor { get; set; }
        DateTime Tgl_Masuk { get; set; }
        string Jenis_Cuti { get; set; }
        DateTime Tgl_Pengajuan { get; set; }
        DateTime Tgl_Setuju { get; set; }
        DateTime Tgl_Mulai { get; set; }
        DateTime Tgl_Selesai { get; set; }
        int Masa_Cuti { get; set; }
        string Keperluan { get; set; }
        string Pemberi { get; set; }
        string Status_Cuti { get; set; }
        string Keterangan { get; set; }
        string Lokasi { get; set; }
        string Nomor_Kontak { get; set; }
        int Tahun_Cuti { get; set; }
        string Alasan { get; set; }
        int Batal { get; set; }
        string Alasan_Batal { get; set; }
        int Jumlah_Pribadi { get; set; }
        int Jumlah_Khusus { get; set; }
        int Jumlah_Massal { get; set; }
        int Jumlah_Hangus { get; set; }
        int Jumlah_Sisa { get; set; }
        string Privilege { get; set; }
    }
}
