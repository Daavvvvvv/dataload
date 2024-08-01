using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

public interface ILoad
{
    Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader);
}
