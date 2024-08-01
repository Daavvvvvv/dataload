using System;
using System.Data;
using System.IO;

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
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (i < values.Length)
                        {
                            row[i] = values[i];
                        }
                        else
                        {
                            row[i] = DBNull.Value; 
                        }
                    }
                    dataTable.Rows.Add(row);
                }
            }
        }


        return dataTable;
    }
}
