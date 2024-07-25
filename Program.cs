using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2 || args[0] != "-f")
        {
            Console.WriteLine("Uso: dataload [OPCIONES] -f FOLDER");
            return;
        }

        string folder = args[1];
        List<string> csvFiles = GetCsvFiles(folder);

        foreach (string file in csvFiles)
        {
            List<List<string>> csvData = ReadCsvFile(file);
            Console.WriteLine($"Archivo: {Path.GetFileName(file)}");
            Console.WriteLine($"Filas leídas: {csvData.Count}");

            if (csvData.Count > 0)
            {
                Console.WriteLine($"Columnas: {csvData[0].Count}");

                // Mostrar las primeras 5 filas (o menos si hay menos de 5)
                int rowsToShow = Math.Min(8, csvData.Count);
                Console.WriteLine($"Primeras {rowsToShow} filas:");
                for (int i = 0; i < rowsToShow; i++)
                {
                    Console.WriteLine(string.Join(" | ", csvData[i].Take(8))); // Mostrar solo las primeras 5 columnas
                }
            }
            else
            {
                Console.WriteLine("El archivo está vacío.");
            }
            Console.WriteLine("--------------------");
        }
    }

    static List<string> GetCsvFiles(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Console.WriteLine($"La carpeta {folder} no existe.");
            return new List<string>();
        }
        return Directory.GetFiles(folder, "*.csv").ToList();
    }

    static List<List<string>> ReadCsvFile(string filePath)
    {
        List<List<string>> csvData = new List<List<string>>();

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    List<string> row = line.Split(',').Select(field => field.Trim()).ToList();
                    csvData.Add(row);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer el archivo {filePath}: {ex.Message}");
        }

        return csvData;
    }
}