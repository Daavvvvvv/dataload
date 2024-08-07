using System.Diagnostics;

public class DataLoader
{
    private string folderPath;
    private CsvLoader csvLoader;
    private ILoad loadInterface;

    public DataLoader(string folderPath, ILoad loadStrategy)
    {
        this.folderPath = folderPath;
        this.csvLoader = new CsvLoader();
        this.loadInterface = loadStrategy;
    }

    public async Task LoadFiles()
    {
        var csvFiles = GetCsvFiles();
        var startTime = DateTime.Now;

        Console.WriteLine($"Hora de inicio del programa: {startTime:HH:mm:ss:FFF}");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var firstFileLoadStartTime = DateTime.Now;
            Console.WriteLine($"Hora de inicio de la carga del primer archivo: {firstFileLoadStartTime:HH:mm:ss:FFF}");

            var dataTables = await loadInterface.LoadFiles(csvFiles, csvLoader);

            var lastFileLoadEndTime = DateTime.Now;
            Console.WriteLine($"Hora de finalización de la carga del último archivo: {lastFileLoadEndTime:HH:mm:ss:FFF}");

            stopwatch.Stop();
            var totalDuration = stopwatch.Elapsed;
            Console.WriteLine($"Tiempo total del proceso: {totalDuration.TotalMilliseconds} ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la carga de archivos: {ex.Message}");
            throw;
        }
    }

    private List<FileInfo> GetCsvFiles()
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"La carpeta {folderPath} no se encontró.");
        }

        var directoryInfo = new DirectoryInfo(folderPath);
        return directoryInfo.GetFiles("*.csv").ToList();
    }
}
