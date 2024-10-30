using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class DataLoader
{
    private readonly string folder;
    private readonly ILoad loadStrategy;

    public DataLoader(string folder, ILoad loadStrategy)
    {
        this.folder = folder;
        this.loadStrategy = loadStrategy;
    }

    public async Task LoadFiles()
    {
        var csvFiles = new DirectoryInfo(folder).GetFiles("*.csv").ToList();

        if (csvFiles.Count == 0)
        {
            Console.WriteLine("No se encontraron archivos CSV en la carpeta especificada.");
            return;
        }

        var loader = new CsvLoader();
        await loadStrategy.LoadFiles(csvFiles, loader);
    }
}
