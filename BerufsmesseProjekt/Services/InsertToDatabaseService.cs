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

    public static void InsertFirmen(List<FirmenModel> firmenListe)
    {
       string connectionString = AppConstants.SQLConnectionString;

    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
    {
        connection.Open();

        string insertQuery = @"INSERT INTO Firma (Firmenname, Branche)
                               VALUES (@Firmenname, @Branche)";
        string selectLastIdQuery = @"SELECT last_insert_rowid()";

        using (SQLiteTransaction transaction = connection.BeginTransaction())
        {
            foreach (FirmenModel firma in firmenListe)
            {
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@Firmenname", firma.Firmenname);
                    command.Parameters.AddWithValue("@Branche", firma.Branche);
                    command.ExecuteNonQuery();
                }

                int insertedId;
                using (SQLiteCommand selectCommand = new SQLiteCommand(selectLastIdQuery, connection, transaction))
                {
                    insertedId = Convert.ToInt32(selectCommand.ExecuteScalar());
                }

                // Die zugeordneten Ids speichern
                if (firma.Firmenname == "Holz KG" && AppConstants.HolzKGId == 0)
                {
                    AppConstants.HolzKGId = insertedId;
                }
                else if (firma.Firmenname == "Sicher AG" && AppConstants.SicherAGId == 0)
                {
                    AppConstants.SicherAGId = insertedId;
                }
                else if (firma.Firmenname == "Targon" && AppConstants.TargonId == 0)
                {
                    AppConstants.TargonId = insertedId;
                }
            }

            transaction.Commit();
        }
    }
    }
    public static void InsertPDFToDatabase(List<PdfModel> pdfContent)
    {
        string connString = AppConstants.SQLConnectionString;

        using (var connection = new SQLiteConnection(connString))
        {
            connection.Open();

            
            const string selectKlasseIdSql = "SELECT Id FROM Klasse WHERE Klassenname = @Klassenname";
            const string insertKlasseSql = "INSERT INTO Klasse (Klassenname) VALUES (@Klassenname)";

            
            const string insertSchuelerSql = @"
            INSERT INTO Schueler (Name, Nachname, id_klasse)
            VALUES (@Vorname, @Nachname, @KlasseId)";

            
            const string insertJunctionSql = @"
            INSERT OR IGNORE INTO Schueler_zu_Firma (id_firma, id_schueler)
            VALUES (@FirmaId, @SchuelerId)";

            
            int[] companyIds = new[] {
            AppConstants.HolzKGId,
            AppConstants.SicherAGId,
            AppConstants.TargonId
        };

            using (var tx = connection.BeginTransaction())
            {
                foreach (var pdf in pdfContent)
                {
            
                    int klasseId;
                    using (var cmd = new SQLiteCommand(selectKlasseIdSql, connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@Klassenname", pdf.Klasse);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            klasseId = Convert.ToInt32(result);
                        }
                        else
                        {
                            using (var ins = new SQLiteCommand(insertKlasseSql, connection, tx))
                            {
                                ins.Parameters.AddWithValue("@Klassenname", pdf.Klasse);
                                ins.ExecuteNonQuery();
                            }
                            using (var getId = new SQLiteCommand("SELECT last_insert_rowid()", connection, tx))
                            {
                                klasseId = Convert.ToInt32(getId.ExecuteScalar());
                            }
                        }
                    }

                    
                    int schuelerId;
                    using (var cmd = new SQLiteCommand(insertSchuelerSql, connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@Vorname", pdf.Vorname);
                        cmd.Parameters.AddWithValue("@Nachname", pdf.Nachname);
                        cmd.Parameters.AddWithValue("@KlasseId", klasseId);
                        cmd.ExecuteNonQuery();
                    }
                    using (var getId = new SQLiteCommand("SELECT last_insert_rowid()", connection, tx))
                    {
                        schuelerId = Convert.ToInt32(getId.ExecuteScalar());
                    }

                    for (int i = 0; i < pdf.Firmen.Count && i < companyIds.Length; i++)
                    {
                        if (!pdf.Firmen[i])
                            continue;

                        using (var cmd = new SQLiteCommand(insertJunctionSql, connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@FirmaId", companyIds[i]);
                            cmd.Parameters.AddWithValue("@SchuelerId", schuelerId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                tx.Commit();
            }
        }
    }
}
