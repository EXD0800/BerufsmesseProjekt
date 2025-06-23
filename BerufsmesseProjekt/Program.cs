using BerufsmesseProjekt.Services;

namespace BerufsmesseProjekt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            OnStartup();
        }
        public static void OnStartup()
        {
            DataBaseCreatorService.CreateDataBase();
            Menue();
        }

        public static void Menue()
        {
            string input;
            bool validInput = false;
            Console.Clear();

            do
            {
                Console.WriteLine("Willkommen im Berufsmessentool!");
                Console.WriteLine("Folgende Punkte stehen zur Verfügung:");
                Console.WriteLine("/n   - Ausgabe der Gruppenmitglieder");
                Console.WriteLine("/imp - Import von PDF-Dateien");
                Console.WriteLine("/exp - Export der Anmeldungen als CSV");
                Console.Write("Bitte wählen Sie einen Punkt: ");

                input = Console.ReadLine()?.Trim().ToLower();

                switch (input)
                {
                    case "/n":
                        Console.WriteLine("Paul Temiz, Oliver Schiwek");
                        Console.WriteLine("Drücken Sie eine Taste um fortzufahren");
                        Console.ReadKey();
                        Menue();
                        validInput = true;
                        break;
                    case "/imp":
                        // Importlogik aufrufen
                        Menue();
                        validInput = true;
                        break;
                    case "/exp":
                        // Exportlogik aufrufen
                        Menue();
                        validInput = true;
                        break;
                    default:
                        Console.WriteLine("Eingabe ungültig. Bitte erneut versuchen!");
                        break;
                }
            } while (!validInput);
        }
    }
}
