using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

public class SequentialLoad : ILoad
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>(csvFiles.Count);
        foreach (var file in csvFiles)
        {
            var loadStart = Stopwatch.StartNew();
            var dataTable = await Task.Run(() => loader.LoadCsv(file));
            dataTables.Add(dataTable);
            loadStart.Stop();
            Console.WriteLine($"Archivo {file.Name} cargado en {loadStart.ElapsedMilliseconds} ms");
        }
        return dataTables;
    }
}
