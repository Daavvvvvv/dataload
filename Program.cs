using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Si se proporciona un solo argumento y es un archivo existente, procesamos ese archivo
            if (args.Length == 1 && File.Exists(args[0]))
            {
                string filePath = args[0];
                await ProcessSingleFile(filePath);
                Environment.Exit(0);
                return;
            }

            // Procesamiento de argumentos
            string folder = string.Empty;
            string option = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-f" && i + 1 < args.Length)
                {
                    folder = args[i + 1];
                    i++;
                }
                else if (args[i] == "-s" || args[i] == "-m")
                {
                    option = args[i];
                }
                else
                {
                    ShowUsage();
                    Environment.Exit(1);
                    return;
                }
            }

            if (string.IsNullOrEmpty(folder))
            {
                ShowUsage();
                Environment.Exit(1);
                return;
            }

            ILoad loadStrategy;
            switch (option)
            {
                case "-s":
                    loadStrategy = new SingleCoreLoad();
                    break;
                case "-m":
                    loadStrategy = new MultiCoreLoad();
                    break;
                default:
                    loadStrategy = new SequentialLoad();
                    break;
            }

            var dataLoader = new DataLoader(folder, loadStrategy);

            await dataLoader.LoadFiles();
            Environment.Exit(0);
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task ProcessSingleFile(string filePath)
    {
        try
        {
            var csvLoader = new CsvLoader();
            var dataTable = await csvLoader.LoadCsvInPartsAsync(new FileInfo(filePath));

            // Realizar análisis de datos
            var analyzer = new DataAnalyzer();
            analyzer.AddDataTable(dataTable, Path.GetFileNameWithoutExtension(filePath));

            var mostPopularVideo = analyzer.GetMostPopularVideo();
            var leastPopularVideo = analyzer.GetLeastPopularVideo();

            // Mostrar resultados
            Console.WriteLine($"Región: {Path.GetFileNameWithoutExtension(filePath)}");
            Console.WriteLine($"Video más popular: {mostPopularVideo.Title} con {mostPopularVideo.Views} vistas.");
            Console.WriteLine($"Video menos popular: {leastPopularVideo.Title} con {leastPopularVideo.Views} vistas.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al procesar el archivo {filePath}: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Uso del programa:");
        Console.WriteLine("  dataload -f <carpeta> [-s | -m]");
        Console.WriteLine("Opciones:");
        Console.WriteLine("  -f <carpeta>   Especifica la carpeta que contiene los archivos CSV a procesar.");
        Console.WriteLine("  -s             Ejecuta el programa en modo single-core concurrente.");
        Console.WriteLine("  -m             Ejecuta el programa en modo multi-core concurrente.");
        Console.WriteLine("Si no se especifica -s o -m, el programa se ejecuta en modo secuencial.");
    }
}
