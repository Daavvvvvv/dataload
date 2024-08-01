using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ConcurrentSingleCoreLoadStrategy : ILoadStrategy
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        var tasks = csvFiles.Select(file => Task.Run(() =>
        {
            var loadStart = DateTime.Now;
            var dataTable = loader.LoadCsv(file);
            lock (dataTables)
            {
                dataTables.Add(dataTable);
            }
            var loadEnd = DateTime.Now;
            Console.WriteLine($"Archivo {file.Name} cargado en {loadEnd - loadStart}");
        })).ToArray();
        await Task.WhenAll(tasks);
        return dataTables;
    }
}
