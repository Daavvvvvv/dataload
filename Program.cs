using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            ShowUsage();
            return;
        }

        string folder = string.Empty;
        string option = string.Empty;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-f" && i + 1 < args.Length)
            {
                folder = args[i + 1];
                i++; // Saltar el siguiente argumento porque es el valor de la carpeta
            }
            else if (args[i] == "-s" || args[i] == "-m")
            {
                option = args[i];
            }
        }

        if (string.IsNullOrEmpty(folder))
        {
            ShowUsage();
            return;
        }

        var dataLoader = new DataLoader(folder);

        try
        {
            switch (option)
            {
                case "-s":
                    await dataLoader.LoadFilesConcurrentSingleCore();
                    break;
                case "-m":
                    dataLoader.LoadFilesConcurrentMultiCore();
                    break;
                default:
                    dataLoader.LoadFilesSequential();
                    break;
            }
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
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
