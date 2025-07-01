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

    }
}
