using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

public class SingleCoreLoad : ILoad
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        var tasks = new List<Task>();

        var loadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {loadStartTime:HH:mm:ss:fff}");

        foreach (var file in csvFiles)
        {
            var task = Task.Run(async () =>
            {
                // Establecer afinidad al núcleo 0
                System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)1;

                var dataTable = await loader.LoadCsvInPartsAsync(file);
                lock (dataTables)
                {
                    dataTables.Add(dataTable);
                }
                Console.WriteLine($"Archivo {file.Name} cargado por el hilo {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        var loadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {loadEndTime:HH:mm:ss:fff}");

        return dataTables;
    }
}
