using BerufsmesseProjekt.Common;
using BerufsmesseProjekt.Models;
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
    public static void Import()
    {
        Console.WriteLine("Bitte legen Sie alle PDFs in den Ordner: PDF_IMPORT");
        Console.WriteLine("Wenn Sie damit fertig sind, drücken Sie eine Taste!");
        Console.ReadKey();

        string ordnerPfad = Path.Combine(Environment.CurrentDirectory, AppConstants.PDFImportOrdner);
        string zielOrdner = Path.Combine(ordnerPfad, "Verarbeitet");

        Directory.CreateDirectory(zielOrdner);

        var pdfExtraction = new List<PdfModel>();

        foreach (string pdfDatei in Directory.GetFiles(ordnerPfad, "*.pdf"))
        {
            Console.WriteLine($"\n--- {Path.GetFileName(pdfDatei)} ---");

            bool valid = true;
            string vorname = null, nachname = null, klasse = null;
            bool cbTargon = false, cbSicher = false, cbHolz = false;

            try
            {
                // PDF auslesen
                using (var reader = new PdfReader(pdfDatei))
                using (var pdf = new PdfDocument(reader))
                {
                    var formular = PdfAcroForm.GetAcroForm(pdf, true);
                    var felder = formular.GetAllFormFields();

                    vorname = GetFeldwert(felder, "Vorname");
                    nachname = GetFeldwert(felder, "Nachname");
                    klasse = GetFeldwert(felder, "Klasse");

                    cbTargon = IstCheckboxGecheckt(felder, "cbTargon");
                    cbSicher = IstCheckboxGecheckt(felder, "cbSicher");
                    cbHolz = IstCheckboxGecheckt(felder, "cbHolz");

                    Console.WriteLine($"Vorname: {vorname}");
                    Console.WriteLine($"Nachname: {nachname}");
                    Console.WriteLine($"Klasse: {klasse}");
                    Console.WriteLine($"Targon: {(cbTargon ? "Ja" : "Nein")}");
                    Console.WriteLine($"Sicher AG: {(cbSicher ? "Ja" : "Nein")}");
                    Console.WriteLine($"Holz AG: {(cbHolz ? "Ja" : "Nein")}");
                }

                // 1) Pflichtfelder prüfen
                if (string.IsNullOrWhiteSpace(vorname) ||
                    string.IsNullOrWhiteSpace(nachname) ||
                    string.IsNullOrWhiteSpace(klasse) ||
                   !(cbTargon || cbSicher || cbHolz))
                {
                    Console.WriteLine("⚠ Ungültiger Datensatz: alle Felder müssen gefüllt sein und mindestens eine Firma ausgewählt.");
                    valid = false;
                }

                // 2) Doppeltes verhindern
                if (valid && pdfExtraction.Any(p =>
                       p.Vorname.Equals(vorname, StringComparison.OrdinalIgnoreCase) &&
                       p.Nachname.Equals(nachname, StringComparison.OrdinalIgnoreCase) &&
                       p.Klasse.Equals(klasse, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("⚠ Doppelter Eintrag erkannt – wird übersprungen.");
                    valid = false;
                }

                // 3) Nur valid in Liste aufnehmen und PDF verschieben
                if (valid)
                {
                    pdfExtraction.Add(new PdfModel
                    {
                        Vorname = vorname,
                        Nachname = nachname,
                        Klasse = klasse,
                        Firmen = new List<bool> { cbTargon, cbSicher, cbHolz }
                    });

                    string zielDatei = Path.Combine(zielOrdner, Path.GetFileName(pdfDatei));
                    File.Move(pdfDatei, zielDatei);
                    Console.WriteLine("→ PDF wurde verschoben nach 'Verarbeitet'.");
                }
                else
                {
                    Console.WriteLine("→ PDF bleibt im Import-Ordner.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Fehler beim Auslesen: {ex.Message}");
                Console.WriteLine("→ PDF bleibt im Import-Ordner.");
            }
        }

        // Nach Durchlauf alle validen Einträge in die DB schreiben
        InsertToDatabaseService.InsertPDFToDatabase(pdfExtraction);
    }

    static string GetFeldwert(IDictionary<string, PdfFormField> felder, string name)
        => felder.ContainsKey(name)
           ? felder[name].GetValueAsString().Trim()
           : string.Empty;

    static bool IstCheckboxGecheckt(IDictionary<string, PdfFormField> felder, string name)
        => felder.TryGetValue(name, out var f)
           && (f.GetValueAsString() == "ja"
            || f.GetValueAsString() == "On"
            || f.GetValueAsString() == "1"
            || f.GetValueAsString().Equals("true", StringComparison.OrdinalIgnoreCase));
}
