using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class SingleCoreLoad : ILoad
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll")]
    static extern bool SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        var originalAffinity = Process.GetCurrentProcess().ProcessorAffinity;

        // Set the current thread to use only one core (Core 0)
        SetThreadAffinityMask(GetCurrentThread(), new IntPtr(1));

        try
        {
            foreach (var file in csvFiles)
            {
                var loadStart = Stopwatch.StartNew();
                var dataTable = loader.LoadCsv(file); // Execute synchronously
                dataTables.Add(dataTable);
                loadStart.Stop();
                Console.WriteLine($"Archivo {file.Name} cargado en {loadStart.ElapsedMilliseconds} ms");
            }
        }
        finally
        {
            // Restore the original processor affinity
            Process.GetCurrentProcess().ProcessorAffinity = originalAffinity;
        }

        return dataTables;
    }
}
