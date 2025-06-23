using BerufsmesseProjekt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerufsmesseProjekt.Services;

public static class DataBaseCreatorService
{
    static readonly string file;
    public static bool CheckForDatabase() //Ausgehend davon, das die Datenbank nicht existiert
    { 
        if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, AppConstants.DataBasePath)))
        {
            return true;
        }

        return false;
    }

    public static bool IsFileInDirectory()
    {
        return File.Exists(Path.Combine(Environment.CurrentDirectory,AppConstants.DataBasePath,"Berufsmesse.db"));
    }

    public static void CreateDataBase()
    {
        if (CheckForDatabase() && IsFileInDirectory())
        {
            return;
        }
        else
        {
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, AppConstants.DataBasePath));

            //SQLite Datenbank wird erstellt
        }
    }
}
