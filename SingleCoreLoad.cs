using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

public class SingleCoreLoad : ILoad
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll")]
    static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentProcessorNumber();

    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        var threads = new List<Thread>();

        var loadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {loadStartTime:HH:mm:ss:fff}");

        foreach (var file in csvFiles)
        {
            var thread = new Thread(() =>
            {
                // Establecer afinidad al núcleo 0 (primer núcleo)
                IntPtr threadHandle = GetCurrentThread();
                UIntPtr affinityMask = new UIntPtr(1 << 0); // Núcleo 0
                SetThreadAffinityMask(threadHandle, affinityMask);

                var loadStart = Stopwatch.StartNew();
                var dataTable = loader.LoadCsv(file);

                lock (dataTables)
                {
                    dataTables.Add(dataTable);
                }

                loadStart.Stop();

                uint processorNumber = GetCurrentProcessorNumber();

                Console.WriteLine($"Archivo {file.Name} cargado en {loadStart.ElapsedMilliseconds} ms en hilo {Thread.CurrentThread.ManagedThreadId} en núcleo {processorNumber}");
            });

            threads.Add(thread);
            thread.Start();
        }

        // Esperar a que todos los hilos terminen
        foreach (var thread in threads)
        {
            thread.Join();
        }

        var loadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {loadEndTime:HH:mm:ss:fff}");

        return dataTables;
    }
}
