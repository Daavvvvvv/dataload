using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class MultiCoreLoad : ILoad
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var dataTables = new List<DataTable>();
        var tasks = new List<Task>();
        var loadTimes = new Dictionary<string, TimeSpan>();
        var loadStartTime = DateTime.Now;

        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {loadStartTime:HH:mm:ss:fff}");

        foreach (var file in csvFiles)
        {
            tasks.Add(Task.Run(async () =>
            {
                var fileStartTime = DateTime.Now;
                int coreId = Thread.GetCurrentProcessorId();
                int threadId = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"Iniciando carga del archivo {file.Name} en núcleo {coreId} y en hilo {threadId} a las {fileStartTime:HH:mm:ss:fff}");

                try
                {
                    var dataTable = await loader.LoadCsvInPartsAsync(file);
                    lock (dataTables)
                    {
                        dataTables.Add(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error al procesar el archivo {file.FullName}: {ex.Message}");
                }

                var fileEndTime = DateTime.Now;
                var duration = fileEndTime - fileStartTime;
                loadTimes[file.Name] = duration;
                Console.WriteLine($"Archivo {file.Name} finalizado en {duration:mm\\:ss} a las {fileEndTime:HH:mm:ss:fff} en núcleo {coreId} y en hilo {threadId}");
            }));
        }

        await Task.WhenAll(tasks);

        var loadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {loadEndTime:HH:mm:ss:fff}");

        // Análisis de popularidad
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

        var totalLoadDuration = loadEndTime - loadStartTime;
        Console.WriteLine($"\nDuración total de la carga de archivos: {totalLoadDuration:mm\\:ss}");

        return dataTables;
    }
}
