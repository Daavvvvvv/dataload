using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

public class MultiCoreLoad : ILoad
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();

        await Task.Run(() =>
        {
            Parallel.ForEach(csvFiles, async file =>
            {
                var loadStart = Stopwatch.StartNew();
                var dataTable = loader.LoadCsv(file);
                lock (dataTables)
                {
                    dataTables.Add(dataTable);
                }
                loadStart.Stop();
                Console.WriteLine($"Archivo {file.Name} cargado en {loadStart.ElapsedMilliseconds} ms");
            });
        });

        return dataTables;
    }
}
