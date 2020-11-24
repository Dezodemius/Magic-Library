using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
      
      using var pdfDocument = new PdfDocument(pdfPath);
      for (var i = 0; i < pdfDocument.PageCount; i++)
      {
        var pageTextBytes = Encoding.UTF8.GetBytes(pdfDocument.Pages[i].GetText());
        var pageTextInBase64 = Convert.ToBase64String(pageTextBytes);

        pages.Add(new Page(i + 1, bookId, pageTextInBase64));
      }

      return pages;
    }
  }
}