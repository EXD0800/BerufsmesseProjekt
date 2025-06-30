using BerufsmesseProjekt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace BerufsmesseProjekt.Services;

public static class DataBaseCreatorService
{
    public static bool CheckForDatabase() //Ausgehend davon, das die Datenbank nicht existiert
    {
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, AppConstants.PDFImportOrdner)))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, AppConstants.PDFImportOrdner));

        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, AppConstants.PDFOutputOrdner)))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, AppConstants.PDFOutputOrdner));

        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, AppConstants.CSVOutput)))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, AppConstants.CSVOutput));

        if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, AppConstants.DataBasePath)))
        {
            return true;
        }

        return false;
    }

    public static bool IsFileInDirectory()
    {
        return File.Exists(Path.Combine(Environment.CurrentDirectory, AppConstants.DataBasePath, "Berufsmesse.db"));
    }

    public async static void CreateDataBase()
    {
        if (CheckForDatabase() && IsFileInDirectory())
        {
            return;
        }
        else
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, AppConstants.DataBasePath));
                string databasePath = Path.Combine(Environment.CurrentDirectory, AppConstants.DataBasePath, "Berufsmesse.db");
                SQLiteConnection.CreateFile(databasePath);

                using var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;");
                connection.Open();

                string createSchuelerTable = @"CREATE TABLE IF NOT EXISTS Schueler(
                                           Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                           Name VARCHAR(255) NOT NULL,
                                           Nachname VARCHAR(255) NOT NULL,
                                           id_klasse INTEGER NOT NULL,
                                           FOREIGN KEY (id_klasse) REFERENCES Klasse(Id)
                                           )";

                string createFirmaTable = @"CREATE TABLE IF NOT EXISTS Firma(
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                        Firmenname VARCHAR(255) NOT NULL,
                                        Branche VARCHAR(255) NOT NULL
                                        )";

                string createKlasseTable = @"CREATE TABLE IF NOT EXISTS Klasse(
                                         Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                         Klassenname VARCHAR(255) NOT NULL
                                         )";

                string createSchuelerZuFirmaTable = @"CREATE TABLE IF NOT EXISTS Schueler_zu_Firma(
                                                  id_firma INTEGER,
                                                  id_schueler INTEGER,
                                                  PRIMARY KEY(id_firma,id_schueler),
                                                  FOREIGN KEY (id_firma) REFERENCES Firma(Id),
                                                  FOREIGN KEY (id_schueler) REFERENCES Schueler(Id)
                                                  )";

                var commands = new[] {
                createSchuelerTable,
                createFirmaTable,
                createKlasseTable,
                createSchuelerZuFirmaTable
            };

                var tasks = commands.Select(cmd => Task.Run(() =>
                {
                    try
                    {
                        using var command = new SQLiteCommand(cmd, connection);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fehler beim Ausführen eines SQL-Commands: {ex.Message}");
                    }
                }));

                await Task.WhenAll(tasks);

                Console.WriteLine("Tabellen wurden erfolgreich erstellt, Keule!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unerwarteter Fehler: {ex.Message}");
            }
        }
    }
}
