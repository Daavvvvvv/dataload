using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

public class MultiCoreLoad : ILoad
{
    public Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        Parallel.ForEach(csvFiles, file =>
        {
            var loadStart = Stopwatch.StartNew();
            var dataTable = loader.LoadCsv(file);

            lock (dataTables)
            {
                dataTables.Add(dataTable);
            }

            loadStart.Stop();

            Console.WriteLine($"Archivo {file.Name} cargado en {loadStart.ElapsedMilliseconds} ms por núcleo {Thread.GetCurrentProcessorId()}");

        });

        return Task.FromResult(dataTables);
    }
}