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
    // Basis-Pfade
    private static readonly string BaseDir = Environment.CurrentDirectory;
    private static readonly string DbFolder = Path.Combine(BaseDir, AppConstants.DataBasePath);
    private static readonly string DbFilePath = Path.Combine(DbFolder, "Berufsmesse.db");
    private static readonly string ConnString = $"Data Source={DbFilePath};Version=3;";

    /// <summary>
    /// Stellt sicher, dass Verzeichnisse bestehen, und legt bei Bedarf die Datenbank samt Tabellen an.
    /// </summary>
    public static void CreateDatabase()
    {
        // 1) Verzeichnisse anlegen
        EnsureDirectory(AppConstants.PDFImportOrdner);
        EnsureDirectory(AppConstants.PDFOutputOrdner);
        EnsureDirectory(AppConstants.CSVOutput);
        EnsureDirectory(AppConstants.DataBasePath);

        // 2) Wenn DB-Datei schon existiert, beenden
        if (File.Exists(DbFilePath))
            return;

        // 3) Neue Datenbankdatei + Verbindung
        SQLiteConnection.CreateFile(DbFilePath);
        using var connection = new SQLiteConnection(ConnString);
        connection.Open();

        // 4) SQL-Statements nacheinander ausführen
        ExecuteSql(connection, "PRAGMA foreign_keys = ON;");

        ExecuteSql(connection, @"
                CREATE TABLE IF NOT EXISTS Klasse (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Klassenname TEXT NOT NULL
                );");

        ExecuteSql(connection, @"
                CREATE TABLE IF NOT EXISTS Firma (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Firmenname TEXT NOT NULL,
                    Branche TEXT NOT NULL
                );");

        ExecuteSql(connection, @"
                CREATE TABLE IF NOT EXISTS Schueler (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Nachname TEXT NOT NULL,
                    id_klasse INTEGER NOT NULL,
                    FOREIGN KEY(id_klasse) REFERENCES Klasse(Id)
                );");

        ExecuteSql(connection, @"
                CREATE TABLE IF NOT EXISTS Schueler_zu_Firma (
                    id_firma INTEGER NOT NULL,
                    id_schueler INTEGER NOT NULL,
                    PRIMARY KEY(id_firma, id_schueler),
                    FOREIGN KEY(id_firma)   REFERENCES Firma(Id),
                    FOREIGN KEY(id_schueler) REFERENCES Schueler(Id)
                );");

        Console.WriteLine("Datenbank und Tabellen erfolgreich erstellt.");
    }

    /// <summary>
    /// Führt ein einzelnes SQL-Statement aus.
    /// </summary>
    private static void ExecuteSql(SQLiteConnection connection, string sql)
    {
        using var cmd = new SQLiteCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Legt ein Verzeichnis an, falls es noch nicht existiert.
    /// </summary>
    private static void EnsureDirectory(string relativePath)
    {
        var fullPath = Path.Combine(BaseDir, relativePath);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);
    }
}
