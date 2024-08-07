using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            if (args.Length < 2 || (args.Length == 2 && args[0] != "-f"))
            {
                ShowUsage();
                Environment.Exit(1);
                return;
            }

            string folder = string.Empty;
            string option = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-f" && i + 1 < args.Length)
                {
                    folder = args[i + 1];
                    i++;
                }
                else if (args[i] == "-s" || args[i] == "-m")
                {
                    option = args[i];
                }
            }

            if (string.IsNullOrEmpty(folder))
            {
                ShowUsage();
                Environment.Exit(1);
                return;
            }

            ILoad loadStrategy;
            switch (option)
            {
                case "-s":
                    loadStrategy = new SingleCoreLoad();
                    break;
                case "-m":
                    loadStrategy = new MultiCoreLoad();
                    break;
                default:
                    loadStrategy = new SequentialLoad();
                    break;
            }

            var dataLoader = new DataLoader(folder, loadStrategy);

            await dataLoader.LoadFiles();
            Environment.Exit(0);
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("USO: dataload [OPCIONES] -f FOLDER");
        Console.WriteLine("OPCIONES:");
        Console.WriteLine("-s    Lectura concurrente en un solo core");
        Console.WriteLine("-m    Lectura concurrente en múltiples cores");
    }
}