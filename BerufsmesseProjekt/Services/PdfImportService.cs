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
        int importCount = 0;

        foreach (string pdfDatei in Directory.GetFiles(ordnerPfad, "*.pdf"))
        {
            Console.WriteLine($"\n--- {Path.GetFileName(pdfDatei)} ---");

            bool valid = true;
            string vorname = null, nachname = null, klasse = null;
            List<int> gewaehlteFirmen = new();

            try
            {
                using var reader = new PdfReader(pdfDatei);
                using var pdf = new PdfDocument(reader);

                var formular = PdfAcroForm.GetAcroForm(pdf, true);
                var felder = formular.GetAllFormFields();

                vorname = GetFeldwert(felder, "Vorname");
                nachname = GetFeldwert(felder, "Nachname");
                klasse = GetFeldwert(felder, "Klasse");

                if (IstCheckboxGecheckt(felder, "cbTargon"))
                    gewaehlteFirmen.Add(AppConstants.TargonId);
                if (IstCheckboxGecheckt(felder, "cbSicher"))
                    gewaehlteFirmen.Add(AppConstants.SicherAGId);
                if (IstCheckboxGecheckt(felder, "cbHolz"))
                    gewaehlteFirmen.Add(AppConstants.HolzKGId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Fehler beim Einlesen: {ex.Message}");
                Console.WriteLine("→ PDF bleibt im Import-Ordner.");
                continue;
            }

            // Pflichtfelder prüfen
            if (string.IsNullOrWhiteSpace(vorname) ||
                string.IsNullOrWhiteSpace(nachname) ||
                string.IsNullOrWhiteSpace(klasse) ||
                gewaehlteFirmen.Count == 0)
            {
                Console.WriteLine("⚠ Ungültiger Datensatz: Alle Felder müssen gefüllt sein und mindestens eine Firma ausgewählt.");
                Console.WriteLine("→ PDF bleibt im Import-Ordner.");
                continue;
            }

            // Doppelte Einträge verhindern
            if (pdfExtraction.Any(p =>
                   p.Vorname.Equals(vorname, StringComparison.OrdinalIgnoreCase) &&
                   p.Nachname.Equals(nachname, StringComparison.OrdinalIgnoreCase) &&
                   p.Klasse.Equals(klasse, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("⚠ Doppelter Eintrag erkannt – wird übersprungen.");
                Console.WriteLine("→ PDF bleibt im Import-Ordner.");
                continue;
            }

            // In Liste aufnehmen und PDF verschieben
            pdfExtraction.Add(new PdfModel
            {
                Vorname = vorname,
                Nachname = nachname,
                Klasse = klasse,
                Firmen = gewaehlteFirmen
            });

            string zielDatei = Path.Combine(zielOrdner, Path.GetFileName(pdfDatei));
            File.Move(pdfDatei, zielDatei);
            importCount++;

            Console.WriteLine($"✔ Eingelesen: {vorname} {nachname} ({klasse}) – Firmen: {string.Join(", ", gewaehlteFirmen)}");
            Console.WriteLine("→ PDF wurde verschoben nach 'Verarbeitet'.");
        }

        InsertToDatabaseService.InsertPDFToDatabase(pdfExtraction);

        Console.WriteLine($"\n✅ Insgesamt erfolgreich verarbeitet: {importCount} PDF-Datei(en).");
        Console.WriteLine("Drücken Sie eine Taste zum Beenden...");
        Console.ReadKey();
    }

    static string GetFeldwert(IDictionary<string, PdfFormField> felder, string name)
        => felder.TryGetValue(name, out var f)
           ? f.GetValueAsString().Trim()
           : string.Empty;

    static bool IstCheckboxGecheckt(IDictionary<string, PdfFormField> felder, string name)
        => felder.TryGetValue(name, out var f)
           && (f.GetValueAsString() == "ja"
            || f.GetValueAsString() == "On"
            || f.GetValueAsString() == "1"
            || f.GetValueAsString().Equals("true", StringComparison.OrdinalIgnoreCase));
}