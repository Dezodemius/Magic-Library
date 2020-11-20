using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    /// Извлечь весь текст PDF-документа.
    /// </summary>
    /// <param name="pdfPath">Имя файла.</param>
    /// <returns>Текстовый слой в base64.</returns>
    public static string ExtractTextLayer(string pdfPath)
    {
      var pdfBytes = File.ReadAllBytes(pdfPath);
      return Convert.ToBase64String(pdfBytes);
    }

    /// <summary>
    /// Получить словарь номеров страниц с их текстом в base64.
    /// </summary>
    /// <param name="pdfPath">Путь к файлу.</param>
    /// <returns>Словарь.</returns>
    public static Dictionary<int, string> GetTextLayerWithPages(string pdfPath)
    {
      var pagesWithNumbers = new Dictionary<int, string>();
      
      using var pdfDocument = new PdfDocument(pdfPath);
      for (var i = 0; i < pdfDocument.PageCount; i++)
      {
        var pageTextBytes = Encoding.UTF8.GetBytes(pdfDocument.Pages[i].GetText());
        var pageTextInBase64 = Convert.ToBase64String(pageTextBytes);
        
        pagesWithNumbers.Add(i, pageTextInBase64);
      }

      return pagesWithNumbers;
    }
  }
}