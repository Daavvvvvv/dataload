using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class MultiCoreLoad : ILoad
{
    public async Task<List<DataTable>> LoadFiles(List<FileInfo> csvFiles, CsvLoader loader)
    {
        var tasks = new List<Task>();

        var loadStartTime = DateTime.Now;
        Console.WriteLine($"Hora de inicio de la carga del primer archivo: {loadStartTime:HH:mm:ss:fff}");

        foreach (var file in csvFiles)
        {
            tasks.Add(Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    Arguments = $"\"{file.FullName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.Error.WriteLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.Error.WriteLine($"El proceso para el archivo {file.Name} terminó con errores.");
                }
            }));
        }

        await Task.WhenAll(tasks);

        var loadEndTime = DateTime.Now;
        Console.WriteLine($"Hora de finalización de la carga del último archivo: {loadEndTime:HH:mm:ss:fff}");

        // Los DataTables se manejan en los procesos hijos
        return null;
    }
}
