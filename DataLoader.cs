using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

public class DataLoader
{
    private string folderPath;
    private List<FileInfo> csvFiles;
    private List<DataTable> dataTables;

    public DataLoader(string folderPath)
    {
        this.folderPath = folderPath;
        this.csvFiles = new List<FileInfo>();
        this.dataTables = new List<DataTable>();
    }

    public void LoadFilesSequential()
    {
        LoadFiles();
        var startTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio del programa: {startTime:HH:mm:ss}");

        var firstFileLoadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {firstFileLoadStartTime:HH:mm:ss}");

        foreach (var file in csvFiles)
        {
            var loadStart = DateTime.Now;
            var dataTable = LoadFile(file);
            dataTables.Add(dataTable);
            var loadEnd = DateTime.Now;
            Console.WriteLine($"Archivo {file.Name} cargado en {loadEnd - loadStart}");
        }

        var lastFileLoadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {lastFileLoadEndTime:HH:mm:ss}");

        var totalDuration = lastFileLoadEndTime - startTime;
        Console.WriteLine($"Tiempo total del proceso: {totalDuration:mm\\:ss}");
    }

    public async Task LoadFilesConcurrentSingleCore()
    {
        LoadFiles();
        var startTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio del programa: {startTime:HH:mm:ss}");

        var firstFileLoadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {firstFileLoadStartTime:HH:mm:ss}");

        var tasks = csvFiles.Select(file => Task.Run(() =>
        {
            var loadStart = DateTime.Now;
            var dataTable = LoadFile(file);
            lock (dataTables)
            {
                dataTables.Add(dataTable);
            }
            var loadEnd = DateTime.Now;
            Console.WriteLine($"Archivo {file.Name} cargado en {loadEnd - loadStart}");
        })).ToArray();
        await Task.WhenAll(tasks);

        var lastFileLoadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {lastFileLoadEndTime:HH:mm:ss}");

        var totalDuration = lastFileLoadEndTime - startTime;
        Console.WriteLine($"Tiempo total del proceso: {totalDuration:mm\\:ss}");
    }

    public void LoadFilesConcurrentMultiCore()
    {
        LoadFiles();
        var startTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio del programa: {startTime:HH:mm:ss}");

        var firstFileLoadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {firstFileLoadStartTime:HH:mm:ss}");

        Parallel.ForEach(csvFiles, file =>
        {
            var loadStart = DateTime.Now;
            var dataTable = LoadFile(file);
            lock (dataTables)
            {
                dataTables.Add(dataTable);
            }
            var loadEnd = DateTime.Now;
            Console.WriteLine($"Archivo {file.Name} cargado en {loadEnd - loadStart}");
        });

        var lastFileLoadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {lastFileLoadEndTime:HH:mm:ss}");

        var totalDuration = lastFileLoadEndTime - startTime;
        Console.WriteLine($"Tiempo total del proceso: {totalDuration:mm\\:ss}");
    }

    private void LoadFiles()
    {
        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            csvFiles = directoryInfo.GetFiles("*.csv").ToList();
        }
        else
        {
            throw new DirectoryNotFoundException($"La carpeta {folderPath} no se encontró.");
        }
    }

    private DataTable LoadFile(FileInfo file)
    {
        var dataTable = new DataTable(file.Name);

        using (var reader = new StreamReader(file.FullName))
        {
            bool isFirstRow = true;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (isFirstRow)
                {
                    foreach (var column in values)
                    {
                        dataTable.Columns.Add(column);
                    }
                    isFirstRow = false;
                }
                else
                {
                    var row = dataTable.NewRow();
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (i < values.Length)
                        {
                            row[i] = values[i];
                        }
                        else
                        {
                            row[i] = DBNull.Value; // Manejar columnas faltantes
                        }
                    }
                    dataTable.Rows.Add(row);
                }
            }
        }

        // Simulación de procesamiento pesado (opcional para pruebas)
        // System.Threading.Thread.Sleep(1000);

        return dataTable;
    }
}
