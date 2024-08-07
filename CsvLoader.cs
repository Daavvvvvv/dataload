using System.Data;

public class CsvLoader
{
    public DataTable LoadCsv(FileInfo file)
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
                    int columnCount = Math.Min(dataTable.Columns.Count, values.Length);
                    for (int i = 0; i < columnCount; i++)
                    {
                        row[i] = values[i];
                    }
                    for (int i = columnCount; i < dataTable.Columns.Count; i++)
                    {
                        row[i] = DBNull.Value;
                    }
                    dataTable.Rows.Add(row);
                }
            }
        }

        return dataTable;
    }
}
