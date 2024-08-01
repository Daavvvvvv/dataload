using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

public class ConcurrentMultiCoreLoadStrategy : ILoadStrategy
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        await Task.Run(() =>
        {
            Parallel.ForEach(csvFiles, file =>
            {
                var loadStart = DateTime.Now;
                var dataTable = loader.LoadCsv(file);
                lock (dataTables)
                {
                    dataTables.Add(dataTable);
                }
                var loadEnd = DateTime.Now;
                Console.WriteLine($"Archivo {file.Name} cargado en {loadEnd - loadStart}");
            });
        });
        return dataTables;
    }
}
