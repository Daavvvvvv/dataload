using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class CsvLoader
{
   
    public async Task<DataTable> LoadCsvInPartsAsync(FileInfo file)
    {
        var dataTable = new DataTable(file.Name);
        var semaphore = new SemaphoreSlim(2); 

        using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(fileStream))
        {
            string headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine))
            {
                throw new Exception("El archivo está vacío o no tiene encabezado.");
            }

            var headers = ParseCsvLine(headerLine);
            foreach (var header in headers)
            {
                dataTable.Columns.Add(header);
            }

            char[] buffer = new char[4096]; // Buffer de 4KB
            string remainder = string.Empty;
            var tasks = new List<Task>();

            while (!reader.EndOfStream)
            {
                int charsRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                string chunk = remainder + new string(buffer, 0, charsRead);
                var lines = chunk.Split(new[] { '\n' }, StringSplitOptions.None);

                remainder = lines[^1];
                string[] linesToProcess = lines[..^1];

                if (linesToProcess.Length > 0)
                {
                    await semaphore.WaitAsync();

                    var task = Task.Run(() =>
                    {
                        try
                        {
                            var localRows = new List<DataRow>();

                            foreach (var line in linesToProcess)
                            {
                                var trimmedLine = line.Trim();
                                if (string.IsNullOrEmpty(trimmedLine)) continue;

                                var values = ParseCsvLine(trimmedLine);

                                // Verificación adicional de longitud de fila
                                if (values.Length != dataTable.Columns.Count)
                                {
                                    Array.Resize(ref values, dataTable.Columns.Count);
                                    continue;
                                }

                                var row = dataTable.NewRow();
                                try
                                {
                                    for (int j = 0; j < dataTable.Columns.Count; j++)
                                    {
                                        row[j] = string.IsNullOrEmpty(values[j]) ? DBNull.Value : values[j];
                                    }
                                    localRows.Add(row);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error al agregar la fila: {ex.Message}. Fila omitida: {trimmedLine}");
                                }
                            }

                            if (localRows.Count > 0)
                            {
                                lock (dataTable)
                                {
                                    foreach (var row in localRows)
                                    {
                                        dataTable.Rows.Add(row);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    tasks.Add(task);
                }
            }

            if (!string.IsNullOrEmpty(remainder))
            {
                var values = ParseCsvLine(remainder.Trim());

                // Verificación adicional para la última fila incompleta
                if (values.Length == dataTable.Columns.Count)
                {
                    var row = dataTable.NewRow();
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        row[j] = string.IsNullOrEmpty(values[j]) ? DBNull.Value : values[j];
                    }
                    dataTable.Rows.Add(row);
                }
                else
                {
                    Console.WriteLine($"Advertencia: La última fila incompleta tiene {values.Length} valores, pero se esperaban {dataTable.Columns.Count}. Fila omitida: {remainder}");
                }
            }

            await Task.WhenAll(tasks);
        }

        return dataTable;
    }


    // Método para carga secuencial (usado en SequentialLoad)
    public DataTable LoadCsv(FileInfo file)
    {
        var dataTable = new DataTable(file.Name);

        using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(fileStream))
        {
            // Leer la primera línea (encabezado)
            string headerLine = reader.ReadLine();
            if (string.IsNullOrEmpty(headerLine))
            {
                throw new Exception("El archivo está vacío o no tiene encabezado.");
            }

            var headers = ParseCsvLine(headerLine);
            foreach (var header in headers)
            {
                dataTable.Columns.Add(header);
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = ParseCsvLine(line);

                var row = dataTable.NewRow();
                int columnCount = Math.Min(dataTable.Columns.Count, values.Length);
                for (int j = 0; j < columnCount; j++)
                {
                    row[j] = values[j];
                }
                dataTable.Rows.Add(row);
            }
        }

        return dataTable;
    }

    private string[] ParseCsvLine(string line)
    {
        if (string.IsNullOrEmpty(line))
            return Array.Empty<string>();

        var fields = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Verificar si la siguiente es otra comilla (comilla escapada)
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; 
                    }
                    else
                    {
                        inQuotes = false; 
                    }
                }
                else
                {
                    currentField.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true; // Comienzo de una sección entre comillas
                }
                else if (c == ',')
                {
                    // Fin del campo
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
        }
        fields.Add(currentField.ToString());

        return fields.ToArray();
    }

}
