namespace BerufsmesseProjekt.Common;

public class AppConstants
{
    public static string SQLConnectionString {  get; set; }
    public static string DataBasePath = "Database";
    public static string PDFImportOrdner = "PDF_IMPORT";
    public static string PDFOutputOrdner = "PDF_OUTPUT";
    public static string CSVOutput = "CSV_OUTPUT";

    public static int HolzKGId { get; set; }
    public static int SicherAGId { get; set; }
    public static int TargonId { get; set; }
}
