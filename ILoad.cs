using System.Data;


public interface ILoad
{
    Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader);
}
