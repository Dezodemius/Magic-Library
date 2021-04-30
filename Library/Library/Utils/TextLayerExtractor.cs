using System;
using System.Collections.Generic;
using System.Text;
using BitMiracle.Docotic.Pdf;
using iTextSharp.text.pdf.parser;
using Library.Entity;
using NLog;
using PdfDocument = BitMiracle.Docotic.Pdf.PdfDocument;

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
    /// Получить список страниц с их текстом в base64.
    /// </summary>
    /// <param name="pdfPath">Путь к файлу.</param>
    /// <param name="bookId">ИД книги, к которой пренадлежит страница.</param>
    /// <returns>Список страниц.</returns>
    public static IEnumerable<Page> GetTextLayerWithPages(string pdfPath, Guid bookId)
    {
      var pages = new List<Page>();
      var reader = new iTextSharp.text.pdf.PdfReader(pdfPath);
      for (int i = 1; i <= reader.NumberOfPages; i++)
      {
        var text = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy())
            .Normalize(NormalizationForm.FormC);
        var pageTextBytes = Encoding.UTF8.GetBytes(text);
        var pageTextInBase64 = Convert.ToBase64String(pageTextBytes);
        
        pages.Add(new Page(i, bookId, pageTextInBase64));
      }

      return pages;
    }
  }
}