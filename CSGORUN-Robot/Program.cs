using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Extensions;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CSGORUN_Robot
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.OutputEncoding = Encoding.Unicode;


            var settings = SettingsProvider.Provide();
            new ManualResetEvent(false).WaitOne();

            //var client = new Client.Client(settings.CSGORUN.Accounts[settings.CSGORUN.PrimaryAccountId ?? 0]);
            //try
            //{
            //    var resp = client.httpService.GetCurrentStateAsync().GetAwaiter().GetResult();
            //    Console.WriteLine(JsonSerializer.Serialize(resp, new JsonSerializerOptions() { WriteIndented = true }));
            //}
            //catch (HttpRequestRawException ex)
            //{
            //    var str = ex.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //    Console.WriteLine("Error with content: " + str);
            //}
        }
    }
}
