using BerufsmesseProjekt.Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerufsmesseProjekt.Services;

public class CsvExportService
{
    public static void ExportAsCSV()
    {
        string connectionString = AppConstants.SQLConnectionString;
        string outputDir = Path.Combine(Environment.CurrentDirectory, AppConstants.CSVOutput);

        // Sicherstellen, dass der Ausgabe-Ordner existiert
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            // Firmen-IDs und zugehörige Namen
            Dictionary<int, string> companies = new Dictionary<int, string>
        {
            { AppConstants.HolzKGId,   "Holz KG"   },
            { AppConstants.SicherAGId, "Sicher AG" },
            { AppConstants.TargonId,   "Targon"    }
        };

            foreach ((int firmaId, string firmaName) in companies)
            {
                // Dateiname + Pfad im Ausgabe-Ordner
                string fileName = $"Teilnahmeliste_{firmaName}.csv";
                string filePath = Path.Combine(outputDir, fileName);

                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Kopfzeile
                    writer.WriteLine("Vorname,Nachname");

                    // Teilnehmer abfragen und sortieren
                    string sql = @"
                    SELECT s.Name, s.Nachname
                      FROM Schueler s
                      JOIN Schueler_zu_Firma szf 
                        ON s.Id = szf.id_schueler
                     WHERE szf.id_firma = @FirmaId
                  ORDER BY s.Name, s.Nachname";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@FirmaId", firmaId);

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string vorname = reader.GetString(0);
                                string nachname = reader.GetString(1);
                                writer.WriteLine($"{vorname},{nachname}");
                            }
                        }
                    }
                }
            }
        }
        Console.WriteLine("Teilnahmeliste_HolzKG.csv erfolgreich erstellt\r\nTeilnahmeliste_SicherAG.csv erfolgreich erstellt\r\nTeilnahmeliste_Targon.csv erfolgreich erstellt");
    }
}
