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

        // Parte adicional después de la carga en SingleCoreLoad para hacer el análisis de popularidad
        var allVideos = new List<VideoData>();
        var analyzer = new DataAnalyzer();

        foreach (var table in dataTables)
        {
            var region = Path.GetFileNameWithoutExtension(table.TableName);
            analyzer.AddDataTable(table, region);
            allVideos.AddRange(analyzer.GetVideos());
        }

        // Video más y menos popular globalmente
        var mostPopular = analyzer.GetMostPopularVideo();
        var leastPopular = analyzer.GetLeastPopularVideo();

        Console.WriteLine("\nVideos más y menos populares globalmente:");
        Console.WriteLine($"Video más popular: {mostPopular?.Title} con {mostPopular?.Views} vistas.");
        Console.WriteLine($"Video menos popular: {leastPopular?.Title} con {leastPopular?.Views} vistas.");

        // Video más popular por región
        Console.WriteLine("\nVideos más populares por región:");
        var groupedByRegion = allVideos.GroupBy(v => v.Region);
        foreach (var group in groupedByRegion)
        {
            var popularVideo = group.OrderByDescending(v => v.Views).FirstOrDefault();
            if (popularVideo != null)
            {
                Console.WriteLine($"Región: {popularVideo.Region}, Video más popular: {popularVideo.Title} con {popularVideo.Views} vistas.");
            }
        }


        var loadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {loadEndTime:HH:mm:ss:fff}");
        var totalLoadDuration = loadEndTime - loadStartTime;
        Console.WriteLine($"\nDuración total de la carga de archivos: {totalLoadDuration:mm\\:ss}");


        return dataTables;
    }
}
