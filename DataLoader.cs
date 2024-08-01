using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

public class DataLoader
{
    private string folderPath;
    private CsvLoader csvLoader;
    private ILoadStrategy loadStrategy;

    public DataLoader(string folderPath, ILoadStrategy loadStrategy)
    {
        this.folderPath = folderPath;
        this.csvLoader = new CsvLoader();
        this.loadStrategy = loadStrategy;
    }

    public async Task LoadFiles()
    {
        var csvFiles = GetCsvFiles();
        var startTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio del programa: {startTime:HH:mm:ss}");

        var firstFileLoadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {firstFileLoadStartTime:HH:mm:ss}");

        try
        {
            var dataTables = await loadStrategy.LoadFiles(csvFiles, csvLoader);

            var lastFileLoadEndTime = DateTime.Now;
            Console.WriteLine($"Hora de finalización de la carga del último archivo: {lastFileLoadEndTime:HH:mm:ss}");

            var totalDuration = lastFileLoadEndTime - startTime;
            Console.WriteLine($"Tiempo total del proceso: {totalDuration:mm\\:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la carga de archivos: {ex.Message}");
            throw;
        }
    }

    private List<FileInfo> GetCsvFiles()
    {
        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            return directoryInfo.GetFiles("*.csv").ToList();
        }
        else
        {
            throw new DirectoryNotFoundException($"La carpeta {folderPath} no se encontró.");
        }
    }
}
