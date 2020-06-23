using System;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Library.Entity;

namespace Library
{
  /// <summary>
  /// Объект для извлечения текстового слоя из документа.
  /// </summary>
  public static class TextLayerExtractor
  {
    /// <summary>
    /// Извлечь PDF-документ в объект Book.
    /// </summary>
    /// <param name="pdfPath">Имя файла.</param>
    /// <returns>Текстовый слой.</returns>
    public static Book FromPdfToBook(string pdfPath)
    {
      using var pdfReader = new PdfReader(pdfPath);
      
      var text = new StringBuilder();
      var strategy = new SimpleTextExtractionStrategy();

      for (var i = 1; i < pdfReader.NumberOfPages; i++)
        text.Append(PdfTextExtractor.GetTextFromPage(pdfReader, i, new LocationTextExtractionStrategy()));

      Console.WriteLine("Не удалось определить автора книги. Введите название книги: ");
      var name = Console.ReadLine();
      return new Book(name, text.ToString());
    }
  }
}