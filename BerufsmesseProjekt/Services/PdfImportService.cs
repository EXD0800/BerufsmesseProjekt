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
    public void Import()
    {
        Console.WriteLine("Bitte legen Sie alle PDFs in den Ordner: PDF_IMPORT");
        Console.WriteLine("Wenn Sie damit fertig sind, drücken Sie eine Taste!");
        Console.ReadKey();

        string ordnerPfad = Path.Combine(Environment.CurrentDirectory, AppConstants.PDFImportOrdner);
        string zielOrdner = Path.Combine(ordnerPfad, "Verarbeitet");

        if (!Directory.Exists(zielOrdner))
        {
            Directory.CreateDirectory(zielOrdner);
        }

        List<PdfModel> pdfExtraction = new List<PdfModel>();

        foreach (string pdfDatei in Directory.GetFiles(ordnerPfad, "*.pdf"))
        {
            Console.WriteLine($"\n--- {Path.GetFileName(pdfDatei)} ---");

            try
            {
                using (PdfReader reader = new PdfReader(pdfDatei))
                using (PdfDocument pdf = new PdfDocument(reader))
                {
                    PdfAcroForm formular = PdfAcroForm.GetAcroForm(pdf, true);
                    IDictionary<string, PdfFormField> felder = formular.GetAllFormFields();

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

                    pdfExtraction.Add(new PdfModel
                    {
                        Vorname = vorname,
                        Nachname = nachname,
                        Klasse = klasse,
                        Firmen = new List<bool> { cbTargon, cbSicher, cbHolz }
                    });
                }

                string zielDatei = Path.Combine(AppConstants.PDFOutputOrdner, Path.GetFileName(pdfDatei));
                File.Move(pdfDatei, zielDatei);
                Console.WriteLine("→ PDF wurde verschoben nach 'PDF_OUTPUT'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Fehler beim Verarbeiten von {Path.GetFileName(pdfDatei)}: {ex.Message}");
            }
        }
        InsertToDatabaseService.InsertPDFToDatabase(pdfExtraction);
    }

    static string GetFeldwert(IDictionary<string, PdfFormField> felder, string name)
    {
        return felder.ContainsKey(name) ? felder[name].GetValueAsString() : "[nicht vorhanden]";
    }

    static bool IstCheckboxGecheckt(IDictionary<string, PdfFormField> felder, string name)
    {
        if (!felder.ContainsKey(name)) return false;

        string wert = felder[name].GetValueAsString();
        return wert == "Yes" || wert == "On" || wert == "1" || wert.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
