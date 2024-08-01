using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

public class SingleCoreLoad : ILoad
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        var tasks = csvFiles.Select(file => Task.Run(() =>
        {
            var loadStart = Stopwatch.StartNew();
            var dataTable = loader.LoadCsv(file);
            lock (dataTables)
            {
                dataTables.Add(dataTable);
            }
            loadStart.Stop();
            Console.WriteLine($"Archivo {file.Name} cargado en {loadStart.ElapsedMilliseconds} ms");
        })).ToArray();
        await Task.WhenAll(tasks);
        return dataTables;
    }
}
