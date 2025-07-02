using BerufsmesseProjekt.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using BerufsmesseProjekt.Common;
using System.Threading.Tasks;

namespace BerufsmesseProjekt.Services;

public class InsertToDatabaseService
{
    private static readonly string BaseDir = Environment.CurrentDirectory;
    private static readonly string DbFolder = Path.Combine(BaseDir, AppConstants.DataBasePath);
    private static readonly string DbFilePath = Path.Combine(DbFolder, "Berufsmesse.db");
    private static readonly string ConnString = $"Data Source={DbFilePath};Version=3;";
    public static void InsertFirmen(List<FirmenModel> firmenListe)
    {
        string connectionString = ConnString;
        AppConstants.SQLConnectionString = ConnString;
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        const string insertQuery = @"
                INSERT INTO Firma (Firmenname, Branche)
                VALUES (@Firmenname, @Branche)";
        const string selectLastId = "SELECT last_insert_rowid()";

        using var tx = connection.BeginTransaction();
        foreach (var firma in firmenListe)
        {
            using var cmd = new SQLiteCommand(insertQuery, connection, tx);
            cmd.Parameters.AddWithValue("@Firmenname", firma.Firmenname);
            cmd.Parameters.AddWithValue("@Branche", firma.Branche);
            cmd.ExecuteNonQuery();

            using var getId = new SQLiteCommand(selectLastId, connection, tx);
            int id = Convert.ToInt32(getId.ExecuteScalar());

            switch (firma.Firmenname)
            {
                case "Holz KG" when AppConstants.HolzKGId == 0:
                    AppConstants.HolzKGId = id; break;
                case "Sicher AG" when AppConstants.SicherAGId == 0:
                    AppConstants.SicherAGId = id; break;
                case "Targon" when AppConstants.TargonId == 0:
                    AppConstants.TargonId = id; break;
            }
        }
        tx.Commit();
    }

    public static void InsertPDFToDatabase(List<PdfModel> pdfContent)
    {
        using var connection = new SQLiteConnection(AppConstants.SQLConnectionString);
        connection.Open();

        const string selKlasse = "SELECT Id FROM Klasse WHERE Klassenname = @Klassenname";
        const string insKlasse = "INSERT INTO Klasse (Klassenname) VALUES (@Klassenname)";
        const string insSchueler = @"
                INSERT INTO Schueler (Name, Nachname, id_klasse)
                VALUES (@Vorname, @Nachname, @KlasseId)";
        const string insJunct = @"
                INSERT OR IGNORE INTO Schueler_zu_Firma (id_firma, id_schueler)
                VALUES (@FirmaId, @SchuelerId)";

        int[] companyIds = {
                AppConstants.HolzKGId,
                AppConstants.SicherAGId,
                AppConstants.TargonId
            };

        using var tx = connection.BeginTransaction();
        foreach (var pdf in pdfContent)
        {
            // Klasse holen oder anlegen
            int klasseId;
            using (var cmd = new SQLiteCommand(selKlasse, connection, tx))
            {
                cmd.Parameters.AddWithValue("@Klassenname", pdf.Klasse);
                var result = cmd.ExecuteScalar();
                if (result != null)
                    klasseId = Convert.ToInt32(result);
                else
                {
                    using var ins = new SQLiteCommand(insKlasse, connection, tx);
                    ins.Parameters.AddWithValue("@Klassenname", pdf.Klasse);
                    ins.ExecuteNonQuery();
                    using var getId = new SQLiteCommand("SELECT last_insert_rowid()", connection, tx);
                    klasseId = Convert.ToInt32(getId.ExecuteScalar());
                }
            }

            // Schüler anlegen
            int schuelerId;
            using (var cmd = new SQLiteCommand(insSchueler, connection, tx))
            {
                cmd.Parameters.AddWithValue("@Vorname", pdf.Vorname);
                cmd.Parameters.AddWithValue("@Nachname", pdf.Nachname);
                cmd.Parameters.AddWithValue("@KlasseId", klasseId);
                cmd.ExecuteNonQuery();
            }
            using var getSchId = new SQLiteCommand("SELECT last_insert_rowid()", connection, tx);
            schuelerId = Convert.ToInt32(getSchId.ExecuteScalar());

            // Junction-Tabelle
            for (int i = 0; i < pdf.Firmen.Count && i < companyIds.Length; i++)
            {
                if (!pdf.Firmen[i]) continue;
                using var cmd = new SQLiteCommand(insJunct, connection, tx);
                cmd.Parameters.AddWithValue("@FirmaId", companyIds[i]);
                cmd.Parameters.AddWithValue("@SchuelerId", schuelerId);
                cmd.ExecuteNonQuery();
            }
        }
        tx.Commit();
    }
}
