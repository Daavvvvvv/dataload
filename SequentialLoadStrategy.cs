using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

public class SequentialLoadStrategy : ILoadStrategy
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        foreach (var file in csvFiles)
        {
            var loadStart = DateTime.Now;
            var dataTable = loader.LoadCsv(file);
            dataTables.Add(dataTable);
            var loadEnd = DateTime.Now;
            Console.WriteLine($"Archivo {file.Name} cargado en {loadEnd - loadStart}");
        }
        return await Task.FromResult(dataTables);
    }
}
