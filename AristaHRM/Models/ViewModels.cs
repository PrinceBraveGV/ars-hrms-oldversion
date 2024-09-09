using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web;
using DevExpress.Web.ASPxScheduler;
using DevExpress.Web.Mvc;
using AristaHRM.Interfaces;
using AristaHRM.Models;
using AristaHRM.Models.Absensi;

using boolean = System.Boolean;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace AristaHRM.Models
{
    public class HRISContext : DbContext
    {
        public HRISContext() : base("DefaultConnection")
        {
            // patch 160119: mengatasi masalah "wait operation timed out" untuk tabel dengan ukuran > 5000 baris
            // angka yang diberikan adalah batas toleransi waktu koneksi dalam satuan detik (default: 5 menit = 300 detik)
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 300;
        }

        /*
         *  Petunjuk untuk database set:
         *  Tambahan huruf "s", huruf "_K" atau huruf "_T" di akhir nama tabel membedakan variabel database set dengan nama tabel rujukan pada database.
         * 
         *  Database set dapat digunakan pada data provider dalam bentuk enumerasi untuk membuat list atau array data tabel.
         *  
         *  Mohon perhatikan struktur database terlebih dahulu sebelum membuat konteks data baru.
         */

        #region Tabel Master
        public DbSet<TM_Default> TM_Defaults { get; set; }  // tabel master TM_Default
        public DbSet<TM_Karyawan> TM_Karyawans { get; set; }  // tabel master TM_Karyawan
        public DbSet<TM_Perusahaan> TM_Perusahaans { get; set; }  // tabel master TM_Perusahaan
        public DbSet<TM_Cabang> TM_Cabangs { get; set; }  // tabel master TM_Cabang
        public DbSet<TM_Departemen> TM_Departemens { get; set; } // tabel master TM_Departemen
        public DbSet<TM_Riwayat> TM_Riwayats { get; set; }  // tabel master TM_Riwayat
        public DbSet<TM_Cuti> TM_Cutis { get; set; }  // tabel master TM_Cuti
        public DbSet<TM_Cuti_Bawahan> TM_Cuti_Bawahans { get; set; } // tabel master TM_Cuti_Bawahan
        public DbSet<TM_Mutasi> TM_Mutasis { get; set; }  // tabel master TM_Mutasi
        public DbSet<TM_List_Cuti> TM_List_Cutis { get; set; } // tabel master TM_List_Cuti (cuti massal)
        public DbSet<TM_List_Libur> TM_List_Liburs { get; set; }  // tabel master TM_List_Libur
        public DbSet<TM_List_PMK> TM_List_PMKs { get; set; }
        public DbSet<TM_File_Manager> TM_File_Managers { get; set; }  // tabel master TM_File_Manager
        public DbSet<TM_Karyawan_Resign> TM_Resigns { get; set; }
        public DbSet<TM_Pesan> TM_Pesans { get; set; }
        public DbSet<TM_Riwayat_Pengajuan> TM_Riwayat_Pengajuans { get; set; }
        public DbSet<TM_Riwayat_User> TM_Riwayat_Users { get; set; }
        public DbSet<TM_Wilayah> TM_Wilayahs { get; set; }
        public DbSet<TM_Kalender> TM_Kalenders { get; set; }
        public DbSet<TM_Pengumuman> TM_Pengumumans { get; set; }
        public DbSet<TM_StatusWP> TM_StatusWPs { get; set; }

        #endregion

        #region Tabel Transaksi
        public DbSet<TT_Pengajuan> TT_Pengajuans { get; set; }  // tabel temporer TT_Pengajuan
        public DbSet<TT_Approval_Khusus> TT_Approval_K { get; set; }  // tabel temporer TT_Approval_Khusus
        public DbSet<TT_Approval_Tahunan> TT_Approval_T { get; set; }  // tabel temporer TT_Approval_Tahunan
        #endregion
    }

    public class AbsenContext
    {
        /*
         *  Bagian ini diatur untuk mengatur konteks data dari sumber lain
         *  Untuk versi SQL Server di bawah 2005, dapat menggunakan DBML
         */

        public AbsensiDataContext DB = new AbsensiDataContext();
    }

    /* BEGIN MODEL */

    #region Definisi Model
    /*
     *  Bagian definisi model untuk web form
     *  Pembagian jenis model: model master dan model transaksi
     *  Model dapat dipakai di lebih dari satu view form, akan tetapi tidak boleh saling tumpang tindih
     *  (contoh: model karyawan & model cuti)
     */


    #region Model Master
    /* BEGIN MODEL */

    #region Login
    /// <summary>
    /// Model fungsi login
    /// </summary>
    public class LoginModel
    {
        // konstruktor standar
        public LoginModel()
        {
            this.NIK = NIK;
            this.Password = Password;
            this.Remember = Remember;
        }

        [Key]
        [Required(ErrorMessage = "Silakan mengisi NIK.")]
        public string NIK { get; set; }

        [Required(ErrorMessage = "Silakan mengisi password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        bool? remember;
        [Display(Name = "Ingat saya")]
        public bool Remember
        {
            get { return remember ?? false; }
            set { remember = value; }
        }

        bool? isTrainee;

        [Display(Name = "Karyawan trainee/percobaan")]
        public bool IsTrainee
        {
            get { return isTrainee ?? false; }
            set { isTrainee = value; }
        }
    }
    #endregion

    #region Karyawan
    // Model fungsi registrasi karyawan baru, tambah karyawan via admin, resign & model karyawan secara umum
    /// <summary>
    /// Kelas untuk mendefinisikan entitas beserta data pribadi karyawan.
    /// </summary>
    [Table("TM_Karyawan")]
    public class KaryawanModel : IKaryawanRepository
    {
        /*  Catatan: 
         *  Atasan dan supervisor dapat dipilih secara manual; default kolom atasan adalah nama manajer pada departemen kerja ybs
         */

        [Key]
        public string NIK { get; set; }
        public string NIK_Temp { get; set; }

        [Required(ErrorMessage = "Silakan mengisi nama Anda.")]
        [Display(Name = "Nama Karyawan")]
        public string Nama_Karyawan { get; set; }

        [Required(ErrorMessage = "Silakan mengisi jenis kelamin Anda.")]
        [Display(Name = "Jenis Kelamin")]
        public string Jenis_Kelamin { get; set; }

        [Required(ErrorMessage = "Silakan mengisi tempat/kota kelahiran Anda.")]
        [Display(Name = "Tempat Lahir")]
        public string Tempat_Lahir { get; set; }

        [Required(ErrorMessage = "Silakan mengisi tanggal lahir Anda.")]
        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Tanggal Lahir")]
        public DateTime Tgl_Lahir { get; set; }

        [Display(Name = "Provinsi")]
        public string Provinsi { get; set; }

        [Display(Name = "Kota")]
        public string Kota { get; set; }

        [Display(Name = "Alamat")]
        public string Alamat { get; set; }

        [Required(ErrorMessage = "Pilih sesuai agama yang dianut.")]
        public string Agama { get; set; }

        [Required(ErrorMessage = "Pilih sesuai status pernikahan anda saat ini.")]
        [Display(Name = "Status Pernikahan")]
        public string Status_Nikah { get; set; }

        [Required(ErrorMessage = "Masukkan alamat e-mail pribadi anda.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^([a-zA-Z0-9_\\-\\.]+)@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,3})$")]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Perusahaan")]
        [RegularExpression("^([a-zA-Z0-9_\\-\\.]+)@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,3})$")]
        public string Email_Perusahaan { get; set; }

        [Required(ErrorMessage = "Pilih perusahaan tempat anda bekerja saat ini.")]
        public string Perusahaan { get; set; }

        [Required(ErrorMessage = "Pilih cabang di mana anda ditugaskan.")]
        public string Cabang { get; set; }

        [Required(ErrorMessage = "Pilih jabatan atau level posisi anda sekarang.")]
        public string Jabatan { get; set; }

        [Required(ErrorMessage = "Pilih departemen posisi anda bekerja saat ini.")]
        public string Departemen { get; set; }

        [Display(Name = "Atasan Pertama")]
        public string NIK_Advisor { get; set; }
        public string Nama_Advisor { get; set; }

        [Display(Name = "Atasan Kedua")]
        public string NIK_Atasan { get; set; }
        public string Nama_Atasan { get; set; }

        [Display(Name = "Atasan Ketiga")]
        public string NIK_Supervisor { get; set; }
        public string Nama_Supervisor { get; set; }

        [Display(Name = "Status Karyawan")]
        public string Status_Karyawan { get; set; }

        [Required(ErrorMessage = "Masukkan tanggal pertama kali anda bekerja.")]
        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}")]
        [Display(Name = "Tanggal Masuk")]
        public DateTime Tgl_Masuk { get; set; }

        [Display(Name = "Tanggal Resign")]
        public DateTime Tgl_Resign { get; set; }  // khusus untuk resign
        public string Alasan { get; set; }  // khusus untuk resign
        public string Privilege { get; set; }
        public string Area_Kerja { get; set; }
        public string Notes { get; set; }
        public string NoteBaru { get; set; }
        public string NPWP { get; set; }
        public string Status_WP { get; set; }
        public string NoBPJSTK { get; set; }
        public string NoBPJSKS { get; set; }
    }
    #endregion

    #region Password
    // Model form setting password & reset password
    /// <summary>
    /// Kelas untuk melakukan setting maupun reset password.
    /// </summary>
    public class PasswordModel
    {
        [Key]
        public string NIK { get; set; }
        public string Nama_Karyawan { get; set; }

        // password untuk keperluan tertentu
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        // password lama untuk proses penggantian
        [DataType(DataType.Password)]
        [Display(Name = "Password Lama")]
        public string Pass_Lama { get; set; }

        // password baru untuk proses penggantian
        [DataType(DataType.Password)]
        [Display(Name = "Password Baru")]
        public string Pass_Baru { get; set; }

        // password konfirmasi untuk proses penggantian
        [Display(Name = "Konfirmasi Password")]
        public string Konfirmasi { get; set; }
    }
    #endregion

    #region Perusahaan
    // Model form isian data perusahaan
    /// <summary>
    /// Kelas untuk pengisian dan perubahan data perusahaan.
    /// </summary>
    [Table("TM_Perusahaan")]
    public class PerusahaanModel : IPerusahaanRepository
    {
        [Key]
        [Display(Name = "Kode Perusahaan")]
        public string Kode_Perusahaan { get; set; }

        [Display(Name = "Nama Perusahaan")]
        public string Nama_Perusahaan { get; set; }

        [Display(Name = "Kode Singkatan")]
        public string Kode_Singkat { get; set; }
    }
    #endregion

    #region Cabang
    // Model form isian data cabang
    /// <summary>
    /// Kelas untuk pengisian dan perubahan data cabang dari perusahaan tertentu.
    /// </summary>
    [Table("TM_Cabang")]
    public class CabangModel : ICabangRepository
    {
        [Key]
        [Display(Name = "Kode Cabang")]
        public string Kode_Cabang { get; set; }

        [Display(Name = "Kode Singkatan")]
        public string Kode_Singkat { get; set; }

        [Display(Name = "Nama Perusahaan")]
        public string Nama_Perusahaan { get; set; }

        [Display(Name = "Nama Cabang")]
        public string Nama_Cabang { get; set; }

        [Display(Name = "Kepala Cabang")]
        public string Kepala_Cabang { get; set; }

        [Display(Name = "Lokasi")]
        public string Lokasi { get; set; }
    }
    #endregion

    #region Cuti
    // Model form isian pengajuan cuti dll
    /// <summary>
    /// Kelas untuk pengajuan dan persetujuan cuti karyawan.
    /// </summary>
    public class CutiModel : ICutiRepository
    {
        [Key]
        public string ID_Cuti { get; set; }

        [Required(ErrorMessage = "Silakan mengisi NIK.")]
        [Display(Name = "NIK")]
        public string NIK { get; set; }

        [Required(ErrorMessage = "Nama karyawan harus diisi.")]
        [Display(Name = "Nama Karyawan")]
        public string Nama_Karyawan { get; set; }

        [Display(Name = "Jenis Kelamin")]
        public string Jenis_Kelamin { get; set; }
        public string Agama { get; set; }

        [Display(Name = "Status Pernikahan")]
        public string Status_Nikah { get; set; }
        public string Perusahaan { get; set; }
        public string Cabang { get; set; }
        public string Jabatan { get; set; }
        public string Departemen { get; set; }

        [Display(Name = "Atasan Utama")]
        public string Nama_Advisor { get; set; }

        [Display(Name = "Atasan Ybs")]
        public string Nama_Atasan { get; set; }

        [Display(Name = "Supervisor")]
        public string Nama_Supervisor { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}")]
        [Display(Name = "Tanggal Masuk")]
        public DateTime Tgl_Masuk { get; set; }

        [Required(ErrorMessage = "Silakan mengisi jenis cuti.")]
        [Display(Name = "Jenis Cuti")]
        public string Jenis_Cuti { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}")]
        [Required(ErrorMessage = "Tanggal pengajuan harus diisi.")]
        [Display(Name = "Tanggal Pengajuan")]
        public DateTime Tgl_Pengajuan { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}")]
        public DateTime Tgl_Setuju { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}")]
        [Required(ErrorMessage = "Tanggal mulai cuti harus diisi.")]
        [Display(Name = "Tanggal Mulai")]
        public DateTime Tgl_Mulai { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMMM yyyy}")]
        [Required(ErrorMessage = "Tanggal selesai cuti harus diisi.")]
        [Display(Name = "Tanggal Selesai")]
        public DateTime Tgl_Selesai { get; set; }
        public int Masa_Cuti { get; set; }

        [Display(Name = "Keperluan Cuti")]
        public string Keperluan { get; set; }

        [Display(Name = "Pemberi Cuti")]
        public string Pemberi { get; set; }
        public string Status_Cuti { get; set; }
        public string Keterangan { get; set; }

        [Display(Name = "Lokasi Cuti")]
        public string Lokasi { get; set; }

        [Display(Name = "Nomor Kontak")]
        public string Nomor_Kontak { get; set; }
        public int Tahun_Cuti { get; set; }
        public string Alasan { get; set; }
        public int Batal { get; set; }
        public string Alasan_Batal { get; set; }
        public int Jumlah_Pribadi { get; set; }
        public int Jumlah_Khusus { get; set; }
        public int Jumlah_Massal { get; set; }
        public int Jumlah_Hangus { get; set; }
        public int Jumlah_Sisa { get; set; }
        public string Privilege { get; set; }
    }
    #endregion

    #region Cuti Massal
    // Model form isian cuti massal
    /// <summary>
    /// Kelas untuk definisi tanggal cuti massal.
    /// </summary>
    [Table("TM_List_Cuti")]
    public class CutiMassalModel
    {
        [Key]
        public string ID_Daftar { get; set; }

        [Required(ErrorMessage = "Tanggal cuti massal harus diisi.")]
        [Display(Name = "Tanggal Cuti")]
        public DateTime Tgl_Cuti { get; set; }
        public string Jenis_Cuti { get; set; }
        public string Keterangan { get; set; }
        public int Cuti_Massal { get; set; }
    }
    #endregion

    #region Approval
    public class ApprovalModel
    {
        public string Nama_Atasan { get; set; }
        public string Privilege { get; set; }
        public bool SetStartup { get; set; }
        public bool IsManager { get; set; }
        public bool IsSupervisor { get; set; }
    }
    #endregion

    #region Statistik Karyawan
    public class KaryawanStatModel
    {
        // karyawan HO
        public string TotalPusat { get; set; }
        public string KarPusatAktif { get; set; }
        public string KarPusatResign { get; set; }

        // karyawan cabang
        public string TotalCabang { get; set; }
        public string KarCabangAktif { get; set; }
        public string KarCabangResign { get; set; }

    }
    #endregion

    #region Statistik Pengajuan Cuti
    public class CutiStatModel
    {
        // cuti harian
        public string CTHarian { get; set; }
        public string CKHarian { get; set; }
        public string CTApproveHarian { get; set; }
        public string CTRejectHarian { get; set; }
        public string CTBatalHarian { get; set; }
        public string CKApproveHarian { get; set; }
        public string CKRejectHarian { get; set; }
        public string CKBatalHarian { get; set; }

        // cuti mingguan/pekan
        public string CTMingguan { get; set; }
        public string CKMingguan { get; set; }
        public string CTApproveMingguan { get; set; }
        public string CTRejectMingguan { get; set; }
        public string CTBatalMingguan { get; set; }
        public string CKApproveMingguan { get; set; }
        public string CKRejectMingguan { get; set; }
        public string CKBatalMingguan { get; set; }

    }
    #endregion

    #region Hari Libur
    /// <summary>
    /// Kelas untuk definisi hari libur.
    /// </summary>
    public class LiburModel
    {
        [Key]
        public string ID_Daftar { get; set; }

        [Required(ErrorMessage = "Tanggal libur harus diisi.")]
        [Display(Name = "Tanggal Libur")]
        public DateTime Tgl_Libur { get; set; }

        public string Keterangan { get; set; }
    }
    #endregion

    #region Mutasi
    // Model form isian mutasi
    /// <summary>
    /// Kelas untuk pengajuan mutasi karyawan.
    /// </summary>
    public class MutasiModel
    {
        [Key]
        [Required(ErrorMessage = "Silakan tentukan NIK anda.")]
        public string NIK { get; set; }

        [Required(ErrorMessage = "Silakan tentukan nama anda.")]
        [Display(Name = "Nama Karyawan")]
        public string Nama_Karyawan { get; set; }
        public string Perusahaan { get; set; }
        public string Cabang { get; set; }
        public string Jabatan { get; set; }
        public string Departemen { get; set; }

        [Display(Name = "Nama Atasan")]
        public string Nama_Atasan { get; set; }

        [Display(Name = "Nama Supervisor")]
        public string Nama_Supervisor { get; set; }

        [Display(Name = "Perusahaan Baru")]
        public string Perusahaan_Baru { get; set; }

        [Display(Name = "Cabang Baru")]
        public string Cabang_Baru { get; set; }

        [Display(Name = "Jabatan Baru")]
        public string Jabatan_Baru { get; set; }

        [Display(Name = "Departemen Baru")]
        public string Departemen_Baru { get; set; }

        [Display(Name = "Atasan Baru")]
        public string Atasan_Baru { get; set; }

        [Display(Name = "Supervisor Baru")]
        public string Supervisor_Baru { get; set; }

        // opsional, bila perubahan staf melibatkan staf pengganti
        [Display(Name = "Staf Pengganti")]
        public string Staf_Ganti { get; set; }

        // opsional, bila perubahan atasan melibatkan atasan pengganti
        [Display(Name = "Atasan Pengganti")]
        public string Atasan_Ganti { get; set; }

        [Display(Name = "Atasan Asal")]
        public string Atasan_Asal { get; set; }

        [Required(ErrorMessage = "Tanggal mutasi harus diisi.")]
        [Display(Name = "Tanggal Mutasi")]
        public DateTime Tgl_Mutasi { get; set; }
        public boolean GantiPerusahaan { get; set; }
        public boolean GantiCabang { get; set; }
        public boolean GantiJabatan { get; set; }
        public boolean GantiDept { get; set; }
        public boolean GantiAtasan { get; set; }

        public string Keterangan { get; set; }
        public bool Konfirmasi { get; set; }

        public List<KaryawanModel> ListBawahan { get; set; }
        public List<KaryawanModel> ListSelectedNIK { get; set; }
    }
    #endregion

    #region Mutasi Kepala Cabang
    public class MutasiKCBModel
    {
        // maksimal mutasi kepala cabang secara berantai sebanyak 5 mutasi dalam satu waktu (10 field)
        // untuk mutasi yang merupakan pertukaran, cukup 4 field saja yang digunakan
        public string Tipe { get; set; }
        public string Perusahaan { get; set; }
        public DateTime? Tgl_Mutasi { get; set; }
        public List<KaryawanModel> ListKaryawan { get; set; }
        public List<CabangModel> ListCabang { get; set; }
    }
    #endregion

    #region Resign
    // Model form isian resign
    /// <summary>
    /// Kelas untuk pengajuan dan persetujuan resign karyawan.
    /// </summary>
    public class ResignModel : IResignRepository
    {
        [Key]
        [Required(ErrorMessage = "Silakan isi NIK anda.")]
        public string NIK { get; set; }

        [Display(Name = "Nama Karyawan")]
        public string Nama_Karyawan { get; set; }
        public string Perusahaan { get; set; }
        public string Cabang { get; set; }
        public string Jabatan { get; set; }
        public string Departemen { get; set; }

        [Display(Name = "Nama Atasan")]
        public string Nama_Atasan { get; set; }

        [Display(Name = "Nama Supervisor")]
        public string Nama_Supervisor { get; set; }

        [Required(ErrorMessage = "Silakan tentukan tanggal resign anda.")]
        [Display(Name = "Tanggal Resign")]
        public DateTime Tgl_Resign { get; set; }

        [Required(ErrorMessage = "Silakan tentukan alasan anda resign dari perusahaan.")]
        [Display(Name = "Alasan Resign")]
        public string Alasan { get; set; }
        public string Status_Resign { get; set; }
    }
    #endregion

    #region Feedback
    // Model form isian feedback
    /// <summary>
    /// Kelas untuk pengisian umpan-balik/feedback.
    /// </summary>
    public class FeedbackModel
    {
        [Key]
        public string ID_Pesan { get; set; }

        [Required(ErrorMessage = "Silakan mengisi NIK anda.")]
        public string NIK { get; set; }

        [Display(Name = "Nama Karyawan")]
        public string Nama_Karyawan { get; set; }
        public string Perusahaan { get; set; }
        public string Cabang { get; set; }
        public string Jabatan { get; set; }
        public string Departemen { get; set; }

        [Required(ErrorMessage = "Silakan mengisi subjek pesan anda.")]
        [Display(Name = "Subjek Pesan")]
        public string Subjek { get; set; }

        [Required(ErrorMessage = "Silakan mengisi pesan yang akan dikirimkan sebelum diproses.")]
        [Display(Name = "Isi Pesan")]
        public string Isi_Pesan { get; set; }
    }
    #endregion

    #region Query
    public class QueryModel
    {
        public string Alamat_Server { get; set; }
        public string Nama_DB { get; set; }
        public string User_Id { get; set; }
        public string Password { get; set; }
        public string Query_String { get; set; }

        public string Nilai_Awal { get; set; }
        public string Nilai_Akhir { get; set; }
    }
    #endregion

    #region Training
    public class TrainingModel
    {
        public string ID_Daftar { get; set; }

        [Required(ErrorMessage = "Silakan mengisi NIK karyawan bersangkutan.")]
        public string NIK { get; set; }
        public string Nama_Karyawan { get; set; }
        public string Jenis_Training { get; set; }
        public string Nama_Training { get; set; }
        public DateTime Tgl_Mulai { get; set; }
        public DateTime Tgl_Selesai { get; set; }
        public string Keterangan { get; set; }

    }
    #endregion

    #region Pengumuman
    public class AnnoModel
    {
        [Required(ErrorMessage = "Jenis pengumuman wajib diisi.")]
        public string Jenis_Anno { get; set; }

        [Required(ErrorMessage = "Keterangan pengumuman wajib diisi.")]
        public string Keterangan { get; set; }

        [Required(ErrorMessage = "Tanggal pengumuman wajib diisi.")]
        public DateTime Tgl_Anno { get; set; }
    }
    #endregion

    #region Reporting
    /// <summary>
    /// Deklarasi model custom reporting. 
    /// Untuk menghindari konflik atribut dengan System.ComponentModel.DataAnnotations.KeyAttribute untuk plain data model,
    /// namespace DevExpress.Xpo.Key sengaja tidak dideklarasikan di bagian using namespaces.
    /// </summary>
    [DevExpress.Xpo.DeferredDeletion(false)]
    public class ReportEntities : DevExpress.Xpo.XPCustomObject
    {
        string url;
        string name;
        byte[] layout;

        // konstruktor
        public ReportEntities(DevExpress.Xpo.Session session)
            : base(session)
        {

        }

        // properti
        [DevExpress.Xpo.Key]
        public string Url
        {
            get { return url; }
            set { SetPropertyValue("Url", ref url, value); }
        }

        public byte[] Layout
        {
            get { return layout; }
            set { SetPropertyValue("Layout", ref layout, value); }
        }

    }

    public class ReportModel
    {
        public string Url { get; set; }
    }

    public class DesignerModel
    {
        public string Url { get; set; }
        public Object DataSource { get; set; }
        public ReportModel[] Reports { get; set; }
        public String SelectedUrl { get; set; }
    }
    #endregion

    #region PMK
    public class PMKModel
    {
        [Key]
        public long ID_PMK { get; set; }
        public string No_PMK { get; set; }
        public DateTime? Tgl_PMK { get; set; }
        public string Jenis_PMK { get; set; }
        public string NIK_Asal { get; set; }
        public string Perusahaan_Asal { get; set; }
        public string Cabang_Asal { get; set; }
        public string Jabatan_Asal { get; set; }
        public string Departemen_Asal { get; set; }
        public string NIK_Baru { get; set; }
        public string Perusahaan_Baru { get; set; }
        public string Cabang_Baru { get; set; }
        public string Jabatan_Baru { get; set; }
        public string Departemen_Baru { get; set; }
        public DateTime? Tgl_Efektif { get; set; }
        public string Status_Asal { get; set; }
        public string Status_Baru { get; set; }
        public string Alasan { get; set; }
        public string Notes { get; set; }
        public string Created_By { get; set; }
        public DateTime? Created_Date { get; set; }
        public string Modified_By { get; set; }
        public DateTime? Modified_Date { get; set; }
    }
    #endregion

    #region Pajak
    public class PajakModel
    {
        public string KodeStatus { get; set; }
        public string StatusWP { get; set; }
    }
    #endregion

    /* END MODEL */
    #endregion

    #region Model Transaksi
    /* BEGIN MODEL */

    #region Event
    public class EventModel
    {
        public string ID_Daftar { get; set; }
        public string Nama_Event { get; set; }
        public string Jenis_Event { get; set; }
        public DateTime Tgl_Mulai { get; set; }
        public DateTime Tgl_Selesai { get; set; }
    }
    #endregion

    #region Pencarian Data
    public class FindModel
    {
        [Key]
        [Display(Name = "Pilih Karyawan")]
        public string NIK { get; set; }
        public string NIK_Baru { get; set; }
        public string Nama_Karyawan { get; set; }
        public string Perusahaan { get; set; }
        public string Cabang { get; set; }
        public string Jabatan { get; set; }
        public string Departemen { get; set; }
        public string Nama_Atasan { get; set; }
        public DateTime Tgl_Masuk { get; set; }
        public string String_Awal { get; set; }
        public string String_Akhir { get; set; }
        public DateTime Tgl_Awal { get; set; }
        public DateTime Tgl_Akhir { get; set; }
        bool? nilai1;
        public bool Nilai1
        {
            get { return nilai1 ?? false; }
            set { nilai1 = value; }
        }

        bool? nilai2;
        public bool Nilai2
        {
            get { return nilai2 ?? false; }
            set { nilai2 = value; }
        }
    }
    #endregion

    #region Unggah Berkas
    /// <summary>
    /// Kelas untuk menangani pengunggahan (upload) berkas.
    /// </summary>
    public class UploadModel
    {
        [Key]
        [Required(ErrorMessage = "Nama tabel tujuan harus diisi.")]
        public string Nama_Tabel { get; set; }  // tabel SQL Server (HRIS.Tables)

        [Required(ErrorMessage = "Nama tabel sumber harus diisi.")]
        public string Nama_Sumber { get; set; } // tabel Excel atau Access sumber data
        public HttpPostedFileBase UploadCtrl { get; set; }
        public string Nama_File { get; set; }

        [DataType(DataType.Password)]
        public string Pass_File { get; set; } // password file (jika ada)

        public string Parameter_File { get; set; } // parameter setting upload file
    }
    #endregion

    #region Counter
    public class ProgCounter
    {
        public string Kode { get; set; }
        public int TotalCount { get; set; }
        public int DailyCount { get; set; }
    }
    #endregion

    #region JSON Model (Web API)
    public class ObjectModel
    {
        public string Id { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string ThrownError { get; set; }
        public string InnerException { get; set; }
    }
    #endregion

    /* END MODEL */
    #endregion

    #region Model Umum
    public class SearchModel
    {
        public string Kategori { get; set; }
        public string Kriteria { get; set; }
        public DateTime? Tanggal { get; set; }
    }

    public class CommonModel<T> where T : class, new()
    {
        public CommonModel() 
        {
            
        }

        public T BaseModel
        {
            get
            {
                return Activator.CreateInstance<T>();
            }
        }

        public Type GetModelType()
        {
            return new T().GetType();
        }
    }

    public class CommonStructure<T> where T : struct
    {
        public CommonStructure()
        {

        }

        public T BaseStruct
        {
            get
            {
                return Activator.CreateInstance<T>();
            }
        }

        public Type GetStructType()
        {
            return new T().GetType();
        }
    }
    #endregion

    #endregion

    /* END MODEL */

    /* BEGIN HELPER */

    #region Helper
    // Definisi helper atau fungsi penolong
    // Digunakan sebagai penunjang operasi utama

    #region Database
    public class DBHelper
    {
        // Metode untuk menggabungkan nilai integer melalui enum string
        // Termasuk penghapusan ID duplikat dari data list
        public static IEnumerable<String> ConcatInteger(IEnumerable<int> values, String separator, int maxlength, bool skipdupe)
        {
            IDictionary<int, String> valuedict = null;
            var sb = new StringBuilder();

            if (skipdupe)
            {
                valuedict = new Dictionary<int, String>();
            }

            foreach (int value in values)
            {
                if (skipdupe)
                {
                    if (valuedict.ContainsKey(value)) continue;
                    valuedict.Add(value, "");
                }

                String s = value.ToString(CultureInfo.InvariantCulture);
                if ((sb.Length + separator.Length + s.Length) > maxlength)
                {
                    if (sb.Length > 0)
                        yield return sb.ToString();
                    sb.Length = 0;
                }
                if (sb.Length > 0)
                    sb.Append(s);
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        public int GetSelectedRowCount(MVCxGridView grid, String keyfieldname)
        {
            int selectedrows = 0;
            foreach (var keys in grid.GetCurrentPageRowValues(keyfieldname))
            {
                if (grid.Selection.IsRowSelectedByKey(keys))
                {
                    selectedrows++;
                }
            }
            return grid.Selection.FilteredCount - selectedrows;
        }
    }
    #endregion

    #region String Manipulator
    public static class StringHelper
    {
        public static string Right(this String str, int len)
        {
            return str.Substring(str.Length - len, len);
        }
    }
    #endregion

    #region Email
    /// <summary>
    /// Kelas penunjang (helper) untuk parameter pengiriman e-mail secara otomatis dari server.
    /// </summary>
    public class EmailHelper
    {
        // Setting email server untuk pengiriman pemberitahuan/notifikasi karyawan.

        /// <summary>
        /// Nama server atau domain tempat hosting layanan email.
        /// </summary>

#if DEBUG
        public static readonly string EmailHost = "smtp.test.com";
        // public static readonly string EmailHost = "smtp.gmail.com";
        // public static readonly string EmailHost = "smtp.gmail.com"; // uji coba saja
#else
        public static readonly string EmailHost = "email-smtp.ap-southeast-1.amazonaws.com";  // host atau alamat mail server
        // obsolete: smtp.gmail.com
#endif

        /// <summary>
        /// Nomor port yang digunakan email host untuk mengirimkan email pribadi maupun broadcast.
        /// </summary>
#if DEBUG
        public static readonly int EmailPort = 587; // port yang digunakan untuk SMTP, sebelumnya 587
#else
        public static readonly int EmailPort = 587; // port yang digunakan untuk SMTP, sebelumnya 587
#endif

        /// <summary>
        /// Alamat email yang digunakan sebagai dasar pengiriman email kepada karyawan.
        /// </summary>
#if DEBUG
        public static readonly string EmailUserName = "cuti.arista@gmail.com";
#else
        public static readonly string EmailUserName = "AKIAQ3NY6PNZL5BM6UF2"; // user ID atau akun yang digunakan pada mail server
        // AKIAQ3NY6PNZL5BM6UF2
        // obsolete: "cuti.arista@gmail.com"
#endif

        /// <summary>
        /// Password alamat email yang terdaftar pada email server (case sensitive).
        /// </summary>
#if DEBUG
        public static readonly string EmailPassword = "1315017401";
#else
        public static readonly string EmailPassword = "BLthsEpGy5bcvnVOm7ayY9h5pWRJC0r5KqkM1xUrfQPH"; // password akun yang digunakan pada mail server
        // BLthsEpGy5bcvnVOm7ayY9h5pWRJC0r5KqkM1xUrfQPH
        // obsolete: "1315017401"
#endif

        /// <summary>
        /// Memastikan server melakukan pengiriman melalui protokol SSL bila diizinkan.
        /// </summary>
        public static readonly bool EnableSsl = true;

        /// <summary>
        /// Informasi pengguna layanan untuk melakukan login ke email server.
        /// </summary>
        public static NetworkCredential InfoUser = new NetworkCredential(EmailUserName, EmailPassword);
    }
    #endregion

    #region File Manager
    /* BEGIN FILE MANAGER */
    /// <summary>
    /// Kelas penunjang (helper) untuk mengatur sistem manajemen berkas.
    /// </summary>
    public class FileManagerHelper
    {
        static FileSystemProvider FileProvider;
        public static readonly Object RootDirectory = "~/Files";  // direktori publik server tempat penampungan file umum
        public static readonly Object ImagesRootDirectory = "~/Files/Images"; // direktori publik tempat penampungan file gambar

        // ekstensi yang diperbolehkan untuk diunggah
        public static readonly String[] AllowedFileExtensions = new String[] {
            ".avi", ".bmp", ".jpg", ".jpeg", ".gif", ".rtf", ".txt", ".png", ".mpg", ".mp3", ".htm", ".html", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".pps", ".mdb", ".accdb", ".mde", ".accde", ".pdf", ".psd", ".wmv" 
        };

        public static FileManagerFeatureOptions FeatureOptions
        {
            get
            {
                if (HttpContext.Current.Session["FeatureOptions"] == null)
                    HttpContext.Current.Session["FeatureOptions"] = new FileManagerFeatureOptions();
                return (FileManagerFeatureOptions)HttpContext.Current.Session["FeatureOptions"];
            }
            set
            {
                HttpContext.Current.Session["FeatureOptions"] = value;
            }
        }

        public static FileSystemProvider DataFileProvider
        {
            get
            {
                if (FileProvider == null)
                    FileProvider = new FileSystemProvider(string.Empty);
                return FileProvider;
            }
        }

        public static List<SelectListItem> GetListViewMode()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem() { Text = FileListView.Thumbnails.ToString(), Value = FileListView.Thumbnails.ToString(), Selected = true },
                new SelectListItem() { Text = FileListView.Details.ToString(), Value = FileListView.Details.ToString() }
            };
        }
    }

    public class FileManagerFeatureOptions
    {
        FileManagerSettingsEditing settingsEditing;
        FileManagerSettingsToolbar settingsToolbar;
        FileManagerSettingsFolders settingsDirectories;
        MVCxFileManagerSettingsUpload settingsUpload;

        public FileManagerFeatureOptions()
        {
            this.settingsEditing = new FileManagerSettingsEditing(null)
            {
                AllowCreate = true,
                AllowMove = true,
                AllowDelete = true,
                AllowRename = true,
                AllowCopy = true,
                AllowDownload = true
            };
            this.settingsDirectories = new FileManagerSettingsFolders(null)
            {
                Visible = true,
                EnableCallBacks = true,
                ShowFolderIcons = true,
            };
            this.settingsToolbar = new FileManagerSettingsToolbar(null)
            {
                ShowCreateButton = true,
                ShowCopyButton = true,
                ShowDeleteButton = true,
                ShowMoveButton = true,
                ShowDownloadButton = true,
                ShowRenameButton = true,
                ShowRefreshButton = true,
                ShowFilterBox = true,
                ShowPath = true
            };
            this.settingsUpload = new MVCxFileManagerSettingsUpload();
            this.settingsUpload.UseAdvancedUploadMode = true;
            this.settingsUpload.AdvancedModeSettings.EnableMultiSelect = true;
            this.settingsUpload.AdvancedModeSettings.EnableDragAndDrop = true;
            this.settingsUpload.AdvancedModeSettings.EnableFileList = true;
            this.settingsUpload.AdvancedModeSettings.FileListPosition = UploadControlFileListPosition.Bottom;
        }

        public FileManagerSettingsEditing SettingsEditing { get { return settingsEditing; } }
        public FileManagerSettingsToolbar SettingsToolbar { get { return settingsToolbar; } }
        public FileManagerSettingsFolders SettingsFolders { get { return settingsDirectories; } }
        public MVCxFileManagerSettingsUpload SettingsUpload { get { return settingsUpload; } }
    }

    public class FileManagerDetailViewOptions
    {
        public FileManagerDetailViewOptions()
        {
            AllowColumnResize = true;
            AllowColumnDragDrop = true;
            AllowColumnSort = true;
            ShowHeaderFilterButton = false;
        }
        public bool AllowColumnResize { get; set; }
        public bool AllowColumnDragDrop { get; set; }
        public bool AllowColumnSort { get; set; }
        public bool ShowHeaderFilterButton { get; set; }
    }
    /* END FILE MANAGER */
    #endregion

    #region Schedule
    /* BEGIN SCHEDULE HELPER */
    /// <summary>
    /// Kelas penunjang (helper) untuk fungsi penjadwalan/organiser cuti tahunan.
    /// </summary>
    public class ScheduleHelper
    {
        #region Resource
        static MVCxResourceStorage resStorage;
        public static MVCxResourceStorage DefaultResource
        {
            get
            {
                if (resStorage == null)
                    resStorage = CreateResource();
                return resStorage;
            }
        }

        public static MVCxResourceStorage CreateResource()
        {
            var ResStorage = new MVCxResourceStorage();
            ResStorage.Mappings.ResourceId = "ID_Daftar";
            ResStorage.Mappings.Caption = "Jenis_Event";
            return ResStorage;
        }
        #endregion

        #region Storage
        static MVCxAppointmentStorage appStorage;
        public static MVCxAppointmentStorage DefaultStorage
        {
            get
            {
                if (appStorage == null)
                    appStorage = CreateStorage();
                return appStorage;
            }
        }

        public static MVCxAppointmentStorage CreateStorage()
        {
            var AppStorage = new MVCxAppointmentStorage();
            AppStorage.Mappings.AppointmentId = "ID_Daftar";
            AppStorage.Mappings.Start = "Tgl_Mulai";
            AppStorage.Mappings.End = "Tgl_Selesai";
            AppStorage.Mappings.Subject = "Nama_Event";
            AppStorage.Mappings.Description = "Nama_Event";
            AppStorage.Mappings.AllDay = "Harian";
            return AppStorage;
        }
        #endregion
    }
    /* END SCHEDULE HELPER */
    #endregion

    #region File Viewer
    /// <summary>
    /// Kelas penunjang (helper) untuk menampilkan berkas PDF dan berkas lainnya yang dapat dibuka langsung di window browser.
    /// </summary>
    public class FileViewerHelper
    {
        public static byte[] GetBytesFromFile(String PathFile)
        {
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(PathFile);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            catch (FileNotFoundException fnfex)
            {
                throw new FileNotFoundException("Berkas yang anda minta tidak ditemukan.");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
    }
    #endregion

    #region Tema Tampilan
    /// <summary>
    /// Pemilihan tema tampilan sistem (belum digunakan sampai saat ini).
    /// </summary>
    public enum ThemeSelector
    {
        Aqua,           // warna biru muda
        RedWine,        // warna merah
        PlasticBlue,    // warna biru gelap
        Glass,          // warna biru standar
        SoftOrange,      // warna oranye/jingga
        DevEx           // warna putih
    }

    public class SettingModel
    {
        public string ThemeName { get; set; } // nama tema yang dipilih
        public ThemeSelector SelectedTheme { get; set; }
    }
    #endregion

    #region Ribbon Interface Helper
    /// <summary>
    /// Kelas penunjang (helper) untuk fungsi-fungsi tampilan Ribbon bar. 
    /// Seluruh fungsi dalam kelas ini menggunakan generic dengan parameter tipe data.
    /// </summary>
    public class RibbonHelper
    {
        /// <summary>
        /// Pembuatan tombol Ribbon bar berukuran kecil.
        /// </summary>
        /// <typeparam name="T">T = RibbonButtonItem</typeparam>
        /// <param name="name">Nama tombol untuk proses JavaScript.</param>
        /// <param name="text">Teks yang ditampilkan pada tombol.</param>
        /// <param name="iconid">URL atau lokasi icon tombol.</param>
        /// <returns>Ribbon button item.</returns>
        public static T CreateSmallButtonItem<T>(String name, String text, String iconid) where T : RibbonButtonItem
        {
            return CreateButtonItem<T>(name, text, RibbonItemSize.Small, iconid);
        }

        /// <summary>
        /// Pembuatan tombol Ribbon bar dengan icon yang disediakan sendiri.
        /// </summary>
        /// <typeparam name="T">T = RibbonButtonItem</typeparam>
        /// <param name="name">Nama tombol untuk proses JavaScript.</param>
        /// <param name="text">Teks yang ditampilkan pada tombol.</param>
        /// <param name="size">Ukuran tombol (large/small size).</param>
        /// <param name="url">URL atau lokasi icon tombol.</param>
        /// <returns>Ribbon button item.</returns>
        public static T CreateButtonItem<T>(String name, String text, RibbonItemSize size, String url) where T : RibbonButtonItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            item.Text = text;
            item.Size = size;

            if (size == RibbonItemSize.Large)
                item.LargeImage.Url = url;
            else
                item.SmallImage.Url = url;
            return item;
        }

        // membuat tombol ribbon berukuran kecil + icon ID tertentu
        public static T CreateSmallIconButton<T>(String name, String text, String iconid) where T : RibbonButtonItem
        {
            return CreateIconButtonItem<T>(name, text, RibbonItemSize.Small, iconid);
        }

        /// <summary>
        /// Pembuatan tombol Ribbon bar dengan icon bawaan DX.
        /// </summary>
        /// <typeparam name="T">T = RibbonButtonItem</typeparam>
        /// <param name="name">Nama tombol untuk proses JavaScript.</param>
        /// <param name="text">Teks yang ditampilkan pada tombol.</param>
        /// <param name="size">Ukuran tombol (large/small size).</param>
        /// <param name="iconid">Icon ID bawaan DX.</param>
        /// <returns>Ribbon button item.</returns>
        public static T CreateIconButtonItem<T>(String name, String text, RibbonItemSize size, String iconid) where T : RibbonButtonItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            item.Text = text;
            item.Size = size;

            if (size == RibbonItemSize.Large)
                item.LargeImage.IconID = iconid;
            else
                item.SmallImage.IconID = iconid;
            return item;
        }

        /// <summary>
        /// Pembuatan combo box dalam Ribbon bar.
        /// </summary>
        /// <typeparam name="T">T = RibbonComboBoxItem</typeparam>
        /// <param name="name">Nama combo box.</param>
        /// <param name="dds">Tipe drop down yang ditampilkan.</param>
        /// <returns>Combo box item.</returns>
        public static T CreateComboBoxItem<T>(String name, DropDownStyle dds, SelectList list) where T : RibbonComboBoxItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            // item.SubGroupName = name;
            item.PropertiesComboBox.DropDownStyle = dds;
            item.ClientEnabled = true;

            foreach (var items in list)
            {
                item.Items.Add(new ListEditItem() { Text = items.Text, Value = items.Value });
            }

            return item;
        }

        public static T CreateComboBoxItem<T>(String name, DropDownStyle dds, Object list, String textfield, String valuefield) where T: RibbonComboBoxItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            // item.SubGroupName = name;
            item.PropertiesComboBox.DropDownStyle = dds;
            item.PropertiesComboBox.DataSource = list;
            item.PropertiesComboBox.TextField = textfield;
            item.PropertiesComboBox.ValueField = valuefield;
            item.ClientEnabled = true;
            
            return item;
        }

        /// <summary>
        /// Pembuatan text box dalam Ribbon bar.
        /// </summary>
        /// <typeparam name="T">T = RibbonTextBoxItem</typeparam>
        /// <param name="name">Nama text box.</param>
        /// <param name="text">Teks yang akan muncul pada text box.</param>
        /// <param name="val">Nilai default yang diisikan dalam text box.</param>
        /// <returns>Text box item.</returns>
        public static T CreateTextBoxItem<T>(String name, String text, String val) where T : RibbonTextBoxItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            // item.SubGroupName = name;
            item.ClientEnabled = true;
            item.Text = text;
            item.Value = val;
            return item;
        }

        public static T CreateCheckBoxItem<T>(String name, String valchecked, String valunchecked, Type valuetype) where T : RibbonCheckBoxItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            item.SubGroupName = name;
            item.ClientEnabled = true;
            item.PropertiesCheckBox.ValueChecked = valchecked;
            item.PropertiesCheckBox.ValueUnchecked = valunchecked;
            item.PropertiesCheckBox.ValueType = valuetype;

            return item;
        }

        public static T CreateRadioButtonItem<T>(String name, String text, String group, RibbonItemSize size) where T : RibbonOptionButtonItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            // item.SubGroupName = name;
            item.ClientEnabled = true;
            item.OptionGroupName = group;
            item.Text = text;
            item.Size = size;
            return item;
        }

        public static T CreateSpinEditItem<T>(String name, String text, String val, decimal increment) where T : RibbonSpinEditItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            item.Text = text;
            item.Value = val;
            item.PropertiesSpinEdit.Increment = increment;
            return item;
        }

        public static T CreateDateEditItem<T>(String name) where T: RibbonDateEditItem
        {
            var item = Activator.CreateInstance<T>();
            item.Name = name;
            item.PropertiesDateEdit.DropDownButton.Image.Url = "~/Images/Kalender.png";
            return item;
        }
    }
    #endregion

    #region Exception Helper
    public class ExceptionHelper : Exception
    {
        public string GetMessage(Exception e)
        {
            return e.Message;
        }

        public string GetInnerException(Exception e)
        {
            return e.InnerException.ToString();
        }

        public string PrintStackTrace()
        {
            return System.Environment.StackTrace;
        }
    }
    #endregion

    #region Skema Tabel
    public class TabelData
    {
        public String NamaTabel { get; set; }
        public String Alias { get; set; }
        public String SkemaTabel { get; set; }
    }
    #endregion

    // Kelas mirror khusus untuk write data lewat bulk insert

    #region Bulk Insert
    public class DataKaryawan
    {
        [Key]
        public string NIK { get; set; }
        public string Nama_Karyawan { get; set; }
        public string Password { get; set; }
        public string Jenis_Kelamin { get; set; }
        public string Tempat_Lahir { get; set; }
        public Nullable<DateTime> Tgl_Lahir { get; set; }
        public string Provinsi { get; set; }
        public string Kota { get; set; }
        public string Alamat { get; set; }
        public string Agama { get; set; }
        public string Status_Nikah { get; set; }
        public string Email { get; set; }
        public string Email_Perusahaan { get; set; }
        public string Perusahaan { get; set; }
        public string Cabang { get; set; }
        public string Jabatan { get; set; }
        public string Departemen { get; set; }
        public string Nama_Atasan { get; set; }
        public string Nama_Supervisor { get; set; }
        public string Nama_Advisor { get; set; }
        public Nullable<DateTime> Tgl_Masuk { get; set; }
        public Nullable<DateTime> Tgl_Resign { get; set; }
        public string Status_Karyawan { get; set; }
        public string Status_Kerja { get; set; }
        public Nullable<int> Email_Valid { get; set; }
        public string Pembuat { get; set; }
        public string Privilege { get; set; }
        public Nullable<DateTime> Last_Login { get; set; }
        public Nullable<DateTime> Last_Session { get; set; }
        public Nullable<int> Aktif_Login { get; set; }
        public string Area_Kerja { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class DataRiwayat
    {
        [Key]
        public string ID_Cuti { get; set; }
        public string NIK { get; set; }
        public string Nama_Karyawan { get; set; }
        public string Jenis_Cuti { get; set; }
        public Nullable<DateTime> Tgl_Pengajuan { get; set; }
        public Nullable<DateTime> Tgl_Setuju { get; set; }
        public Nullable<DateTime> Tgl_Mulai { get; set; }
        public Nullable<DateTime> Tgl_Selesai { get; set; }
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
        public Nullable<DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
    }

    #endregion

    #endregion

    /* END HELPER */

    #region Atribut

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class CustomRequiredAttribute : RequiredAttribute
    {
        public CustomRequiredAttribute()
        {
            this.ErrorMessage = "";
        }

        public CustomRequiredAttribute(String message)
        {
            this.ErrorMessage = message;
        }

        // @Override
        public override boolean IsValid(object value)
        {
            return base.IsValid(value);
        }

        // @Override
        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CustomDisplayFormatAttribute : DisplayFormatAttribute
    {
        string AlternativeDataFormat { get; set; }

        public CustomDisplayFormatAttribute()
        {

        } 

        // @Override
        public override boolean Match(object obj)
        {
            return base.Match(obj);
        }
    }


    #endregion

    #region Kelas Opsi
    public sealed class Optional<T>
    {
        private Optional()
        {
            this.value = default(T);
        }

        private Optional(T value)
        {
            this.value = value;
        }

        private static readonly Optional<T> EMPTY = new Optional<T>();

        private readonly T value;

        public static Optional<T> Empty<T>()
        {
            var t = EMPTY as Optional<T>;
            return t;
        }

        // @Override
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is Optional<T>))
            {
                return false;
            }

            Optional<T> other = (Optional<T>)obj;
            return object.Equals(value, other.value);
        }

        public T Get()
        {
            if (value == null)
            {
                throw new InvalidOperationException("No value present");
            }
            return value;
        }

        // @Override
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsPresent()
        {
            return value != null;
        }

        public Optional<T> Of<T>(T value)
        {
            return new Optional<T>(value);
        }

        public T OrElse(T other)
        {
            return value != null ? value : other;
        }

        // @Override
        public override string ToString()
        {
            return value != null
                ? string.Format("Optional[{0}]", value)
                : "Optional.Empty";
        }
    }
    #endregion

}