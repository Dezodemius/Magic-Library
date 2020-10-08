using System;
using System.IO;
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
    /// <returns>Текстовый слой в base64.</returns>
    public static string ExtractTextLayer(string pdfPath)
    {
      var pdfBytes = File.ReadAllBytes(pdfPath);
      return Convert.ToBase64String(pdfBytes);
    }
  }
}