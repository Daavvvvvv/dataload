using System.Data;

public class CsvLoader
{
    public DataTable LoadCsv(FileInfo file)
    {
        var dataTable = new DataTable(file.Name);

        using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(fileStream))
        {
            bool isFirstRow = true;
            char[] buffer = new char[4096]; // 4KB buffer
            string remainder = string.Empty;

            while (!reader.EndOfStream)
            {
                int charsRead = reader.Read(buffer, 0, buffer.Length);
                string chunk = remainder + new string(buffer, 0, charsRead);
                var lines = chunk.Split(new[] { '\n' }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length - 1; i++) // Process all complete lines
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

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
                        int columnCount = Math.Min(dataTable.Columns.Count, values.Length);
                        for (int j = 0; j < columnCount; j++)
                        {
                            row[j] = values[j];
                        }
                        for (int j = columnCount; j < dataTable.Columns.Count; j++)
                        {
                            row[j] = DBNull.Value;
                        }
                        dataTable.Rows.Add(row);
                    }
                }

                remainder = lines[^1]; 
            }

            // Se procesa cualquier línea restante que no se haya completado
            if (!string.IsNullOrEmpty(remainder))
            {
                var values = remainder.Trim().Split(',');
                var row = dataTable.NewRow();
                int columnCount = Math.Min(dataTable.Columns.Count, values.Length);
                for (int j = 0; j < columnCount; j++)
                {
                    row[j] = values[j];
                }
                for (int j = columnCount; j < dataTable.Columns.Count; j++)
                {
                    row[j] = DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }
        }

        return dataTable;
    }

}
