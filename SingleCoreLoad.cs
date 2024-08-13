using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class SingleCoreLoad : ILoad
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll")]
    static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        var tasks = new List<Task>();

        foreach (var file in csvFiles)
        {
            tasks.Add(Task.Run(() =>
            {
                var threadHandle = GetCurrentThread();
                var originalAffinity = SetThreadAffinityMask(threadHandle, new IntPtr(2));

                try
                {
                    var loadStart = Stopwatch.StartNew();
                    var dataTable = loader.LoadCsv(file);
                    loadStart.Stop();

                    lock (dataTables)
                    {
                        dataTables.Add(dataTable);
                    }

                    Console.WriteLine($"Archivo {file.Name} cargado en {loadStart.ElapsedMilliseconds} ms por núcleo {Thread.GetCurrentProcessorId()}");
                }
                finally
                {
                    SetThreadAffinityMask(threadHandle, originalAffinity);
                }
            }));
        }

        await Task.WhenAll(tasks);

        return dataTables;
    }
}