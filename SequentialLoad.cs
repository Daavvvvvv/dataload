using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

public class SequentialLoad : ILoad
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();

        var loadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {loadStartTime:HH:mm:ss:fff}");

        foreach (var file in csvFiles)
        {
            var dataTable = loader.LoadCsv(file); // Usamos LoadCsv para carga secuencial
            dataTables.Add(dataTable);
            Console.WriteLine($"Archivo {file.Name} cargado.");
        }

        var loadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {loadEndTime:HH:mm:ss:fff}");

        return dataTables;
    }
}
