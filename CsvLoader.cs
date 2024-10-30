using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class CsvLoader
{
    // Método para carga asíncrona en partes (usado en SingleCoreLoad y MultiCoreLoad)
    public async Task<DataTable> LoadCsvInPartsAsync(FileInfo file)
    {
        var dataTable = new DataTable(file.Name);
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount); // Limitar el número de tareas concurrentes

        using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(fileStream))
        {
            // Leer la primera línea (encabezado)
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

                // Guardar el último elemento como posible línea incompleta
                remainder = lines[^1];

                // Procesar todas las líneas completas en una tarea separada
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

                                var row = dataTable.NewRow();
                                int columnCount = Math.Min(dataTable.Columns.Count, values.Length);
                                for (int j = 0; j < columnCount; j++)
                                {
                                    row[j] = values[j];
                                }
                                localRows.Add(row);
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

            // Procesar cualquier línea restante
            if (!string.IsNullOrEmpty(remainder))
            {
                var values = ParseCsvLine(remainder.Trim());
                var row = dataTable.NewRow();
                int columnCount = Math.Min(dataTable.Columns.Count, values.Length);
                for (int j = 0; j < columnCount; j++)
                {
                    row[j] = values[j];
                }
                dataTable.Rows.Add(row);
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
        int i = 0;
        int length = line.Length;

        while (i < length)
        {
            StringBuilder field = new StringBuilder();
            bool inQuotes = false;
            bool fieldStarted = false;

            while (i < length)
            {
                char c = line[i];

                if (!fieldStarted)
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                        fieldStarted = true;
                        i++; // Saltar la comilla inicial
                    }
                    else if (c == ',')
                    {
                        // Campo vacío
                        fields.Add(string.Empty);
                        i++; // Saltar la coma
                        fieldStarted = false;
                        break;
                    }
                    else
                    {
                        fieldStarted = true;
                        field.Append(c);
                        i++;
                    }
                }
                else
                {
                    if (inQuotes)
                    {
                        if (c == '"')
                        {
                            if (i + 1 < length && line[i + 1] == '"')
                            {
                                // Comilla escapada
                                field.Append('"');
                                i += 2;
                            }
                            else
                            {
                                // Fin del campo entrecomillado
                                inQuotes = false;
                                i++; // Saltar la comilla de cierre
                            }
                        }
                        else
                        {
                            field.Append(c);
                            i++;
                        }
                    }
                    else
                    {
                        if (c == ',')
                        {
                            i++; // Saltar la coma
                            break; // Fin del campo
                        }
                        else
                        {
                            field.Append(c);
                            i++;
                        }
                    }
                }
            }

            fields.Add(field.ToString());
        }

        return fields.ToArray();
    }
}
