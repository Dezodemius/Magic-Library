using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NLog;

namespace Library.Utils
{
  /// <summary>
  /// Объект для извлечения текстового слоя из документа.
  /// </summary>
  public static class TextLayerExtractor
  {
    /// <summary>
    /// Логгер класса.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Извлечь текст PDF-документа.
    /// </summary>
    /// <param name="pdfPath">Имя файла.</param>
    /// <returns>Текстовый слой.</returns>
    public static string ExtractTextLayer(string pdfPath)
    {
      using var pdfReader = new PdfReader(pdfPath);
      
      Log.Debug($"Filename: {pdfPath}; Number of pages: {pdfReader.NumberOfPages}");
      
      var text = new StringBuilder();
      var strategy = new SimpleTextExtractionStrategy();

      for (var i = 1; i < pdfReader.NumberOfPages; i++)
        text.Append(PdfTextExtractor.GetTextFromPage(pdfReader, i, strategy));
      
      return text.ToString();
    }
  }
}