using iText;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerufsmesseProjekt.Services;

public class PdfImportService
{
    public void Import()
    {
        Console.WriteLine("Bitte geben Sie den Dateipfad an:");
        var ordnerPfad = @"C:\DeinPfadZurPDFSammlung"; // <-- hier anpassen!

        foreach (var pdfDatei in Directory.GetFiles(ordnerPfad, "*.pdf"))
        {
            Console.WriteLine($"\n--- {Path.GetFileName(pdfDatei)} ---");

            using (var reader = new PdfReader(pdfDatei))
            using (var pdf = new PdfDocument(reader))
            {
                var formular = PdfAcroForm.GetAcroForm(pdf, true);
                var felder = formular.GetAllFormFields();

                string vorname = GetFeldwert(felder, "Vorname");
                string nachname = GetFeldwert(felder, "Nachname");
                string klasse = GetFeldwert(felder, "Klasse");

                bool cbTargon = IstCheckboxGecheckt(felder, "cbTargon");
                bool cbSicher = IstCheckboxGecheckt(felder, "cbSicher");
                bool cbHolz = IstCheckboxGecheckt(felder, "cbHolz");

                Console.WriteLine($"Vorname: {vorname}");
                Console.WriteLine($"Nachname: {nachname}");
                Console.WriteLine($"Klasse: {klasse}");
                Console.WriteLine($"Targon: {(cbTargon ? "Ja" : "Nein")}");
                Console.WriteLine($"Sicher AG: {(cbSicher ? "Ja" : "Nein")}");
                Console.WriteLine($"Holz AG: {(cbHolz ? "Ja" : "Nein")}");
            }
        }
    }

    static string GetFeldwert(IDictionary<string, PdfFormField> felder, string name)
    {
        return felder.ContainsKey(name) ? felder[name].GetValueAsString() : "[nicht vorhanden]";
    }

    static bool IstCheckboxGecheckt(IDictionary<string, PdfFormField> felder, string name)
    {
        if (!felder.ContainsKey(name)) return false;

        var feld = felder[name];
        var wert = feld.GetValueAsString();
        return wert == "Yes" || wert == "On" || wert == "1";
    }
}
