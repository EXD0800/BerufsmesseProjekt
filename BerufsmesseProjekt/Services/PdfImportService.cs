using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using System.Threading.Tasks;

namespace BerufsmesseProjekt.Services;

public class PdfImportService
{
    public void Import()
    {
        Console.WriteLine("Bitte geben Sie den Dateipfad an:");
        string path = Console.ReadLine();

        foreach (var file in Directory.GetFiles(path))
        {

        }
    }
}
