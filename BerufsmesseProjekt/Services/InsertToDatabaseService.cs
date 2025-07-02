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
        INSERT OR IGNORE INTO Firma (Firmenname, Branche)
        VALUES (@Firmenname, @Branche)";
        const string selectByName = "SELECT Id FROM Firma WHERE Firmenname = @Firmenname";

        var idSetMapping = new Dictionary<string, Action<int>>(StringComparer.OrdinalIgnoreCase)
    {
        { "Holz KG",   id => AppConstants.HolzKGId   = id },
        { "Sicher AG", id => AppConstants.SicherAGId = id },
        { "Targon",    id => AppConstants.TargonId   = id }
    };

        using var tx = connection.BeginTransaction();

        foreach (var firma in firmenListe)
        {
            // Versuchen zu inserten (wird ignoriert, wenn Firma bereits existiert)
            using (var insertCmd = new SQLiteCommand(insertQuery, connection, tx))
            {
                insertCmd.Parameters.AddWithValue("@Firmenname", firma.Firmenname);
                insertCmd.Parameters.AddWithValue("@Branche", firma.Branche);
                insertCmd.ExecuteNonQuery();
            }

            // Danach immer die ID holen – egal ob neu oder schon da
            int id;
            using (var selectCmd = new SQLiteCommand(selectByName, connection, tx))
            {
                selectCmd.Parameters.AddWithValue("@Firmenname", firma.Firmenname);
                var result = selectCmd.ExecuteScalar();
                id = result != null ? Convert.ToInt32(result) : throw new Exception($"Firma-ID für '{firma.Firmenname}' konnte nicht ermittelt werden.");
            }

            if (idSetMapping.TryGetValue(firma.Firmenname, out var setAction))
                setAction(id);
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
        const string selLastId = "SELECT last_insert_rowid()";
        const string insJunct = @"
        INSERT OR IGNORE INTO Schueler_zu_Firma (id_firma, id_schueler)
        VALUES (@FirmaId, @SchuelerId)";

        using var tx = connection.BeginTransaction();

        foreach (var pdf in pdfContent)
        {
            // 🏫 Klasse holen oder neu anlegen
            int klasseId;
            using (var cmd = new SQLiteCommand(selKlasse, connection, tx))
            {
                cmd.Parameters.AddWithValue("@Klassenname", pdf.Klasse);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    klasseId = Convert.ToInt32(result);
                }
                else
                {
                    using var ins = new SQLiteCommand(insKlasse, connection, tx);
                    ins.Parameters.AddWithValue("@Klassenname", pdf.Klasse);
                    ins.ExecuteNonQuery();

                    using var getId = new SQLiteCommand(selLastId, connection, tx);
                    klasseId = Convert.ToInt32(getId.ExecuteScalar());
                }
            }

            // 👤 Schüler einfügen
            int schuelerId;
            using (var cmd = new SQLiteCommand(insSchueler, connection, tx))
            {
                cmd.Parameters.AddWithValue("@Vorname", pdf.Vorname);
                cmd.Parameters.AddWithValue("@Nachname", pdf.Nachname);
                cmd.Parameters.AddWithValue("@KlasseId", klasseId);
                cmd.ExecuteNonQuery();
            }
            using var getSchId = new SQLiteCommand(selLastId, connection, tx);
            schuelerId = Convert.ToInt32(getSchId.ExecuteScalar());

            // 🔗 Beziehungen zu Firmen einfügen (direkt mit IDs)
            foreach (var firmaId in pdf.Firmen.Distinct())
            {
                using var cmd = new SQLiteCommand(insJunct, connection, tx);
                cmd.Parameters.AddWithValue("@FirmaId", firmaId);
                cmd.Parameters.AddWithValue("@SchuelerId", schuelerId);
                cmd.ExecuteNonQuery();
            }
        }

        tx.Commit();
    }
}
