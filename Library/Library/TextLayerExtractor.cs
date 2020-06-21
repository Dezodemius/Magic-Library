using System;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Library
{
  /// <summary>
  /// Объект для извлечения текстового слоя из документа.
  /// </summary>
  public static class TextLayerExtractor
  {
    /// <summary>
    /// Извлечь текстовый слой из PDF-документа.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Текстовый слой.</returns>
    public static string ExtractFromPdf(string fileName)
    {
      using var pdfReader = new PdfReader(fileName);
      
      var text = new StringBuilder();
      var strategy = new SimpleTextExtractionStrategy();

      for (var i = 1; i < pdfReader.NumberOfPages; i++)
        text.Append(PdfTextExtractor.GetTextFromPage(pdfReader, i, strategy));

      return text.ToString();
    }
  }
}