using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AristaHRMTest
{
    public class Modules
    {
        public static void CheckAvailability(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentNullException("Pemeriksaan ketersediaan server membutuhkan alamat IP yang valid.");
            }

            var process = new Process();

            process.StartInfo.FileName = @"C:\Windows\system32\cmd.exe";
            process.StartInfo.Arguments = "ping " + ipAddress.ToString() + " -n 8";

            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            var reader = process.StandardOutput;
            process.WaitForExit();

            reader = process.StandardOutput;
            string result = reader.ReadToEnd();

            Console.WriteLine(result);
        }
    }
}
