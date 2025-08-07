using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace KMC_Forge_BTL_Core_Agent.Utils
{
    public class PdfExtractor
    {
        public static string ExtractTextFromPdf(string pdfFilePath)
        {
            if (!File.Exists(pdfFilePath))
            {
                throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
            }

            var extractedText = new StringBuilder();

            try
            {
                using var pdfReader = new PdfReader(pdfFilePath);
                using var pdfDocument = new PdfDocument(pdfReader);

                int numberOfPages = pdfDocument.GetNumberOfPages();

                for (int page = 1; page <= numberOfPages; page++)
                {
                    var pdfPage = pdfDocument.GetPage(page);
                    var strategy = new SimpleTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfPage, strategy);

                    extractedText.AppendLine($"--- Page {page} ---");
                    extractedText.AppendLine(pageText);
                    extractedText.AppendLine();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting text from PDF: {ex.Message}", ex);
            }

            return extractedText.ToString();
        }
    }

}
