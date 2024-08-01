using System; 
using System.Threading.Tasks; 

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Validar argumentos: se necesita al menos -f FOLDER, y opcionalmente -s o -m
            if (args.Length < 2 && !(args.Length == 2 && args[0] == "-f"))
            {
                ShowUsage();
                Environment.Exit(0);
                return;
            }

            string folder = string.Empty;
            string option = string.Empty;

            // Procesar argumentos
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
                Environment.Exit(1);
                return;
            }

            // Determinar estrategia de carga
            ILoad loadStrategy = option switch
            {
                "-s" => new SingleCoreLoad(),
                "-m" => new MultiCoreLoad(),
                _ => new SequentialLoad()
            };

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
