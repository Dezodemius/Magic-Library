using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text.pdf.parser;
using NLog;
using Tesseract;
using BitMiracle.Docotic.Pdf;
using iTextSharp.text.pdf;
using Page = Library.Entity.Page;
using PdfDocument = BitMiracle.Docotic.Pdf.PdfDocument;
using PdfPage = BitMiracle.Docotic.Pdf.PdfPage;

namespace Library.Utils
{
  /// <summary>
  /// Объект для извлечения текстового слоя из документа.
  /// </summary>
  public static class TextLayerExtractor
  {
    /// <summary>
    /// Достаточный уровень доверия к распознанному тексту.
    /// </summary>
    private const double SufficientRecognitionConfidence = 0.5;
    
    /// <summary>
    /// Список доступных языков для распознавания текста.
    /// </summary>
    private static readonly string[] AvailableLanguages = {"rus", "eng"};

    /// <summary>
    /// Логгер класса.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Получить список страниц с их текстом в base64.
    /// </summary>
    /// <param name="pdfPath">Путь к файлу.</param>
    /// <param name="bookId">ИД книги, к которой пренадлежит страница.</param>
    /// <param name="progressAction">Устанавливает прогресс загрузки книги.</param>
    /// <returns>Список страниц.</returns>
    public static IEnumerable<Page> GetTextLayerWithPages(string pdfPath, Guid bookId, Action<double> progressAction)
    {
      BitMiracle.Docotic.LicenseManager.AddLicenseData("51QVA-4ECP3-YFGX0-51B4S-3GBOB");
      
      var pages = new List<Page>();
      var reader = new PdfReader(pdfPath);
      var numberOfPages = reader.NumberOfPages;
      for (var pageNumber = 1; pageNumber < numberOfPages; pageNumber++)
      {
        var text = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy()).Normalize();
        if (string.IsNullOrEmpty(text))
        {
          using var pdfDocument = new PdfDocument(pdfPath);
          var page = pdfDocument.Pages[pageNumber];
          var recognizePageTexts = RecognizePageText(page);

          foreach (var pageText in recognizePageTexts)
            pages.Add(new Page(pageNumber, bookId, GetStringInBase64(pageText.ToString())));
        }
        else
          pages.Add(new Page(pageNumber, bookId, GetStringInBase64(text)));

        progressAction((double)pageNumber / numberOfPages);
      }

      return pages;
    }
    
    /// <summary>
    /// Получить текст в Base64.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <returns>Base64-строка.</returns>
    private static string GetStringInBase64(string text)
    {
      var pageTextBytes = Encoding.UTF8.GetBytes(text);
      var pageTextInBase64 = Convert.ToBase64String(pageTextBytes);
      return pageTextInBase64;
    }

    /// <summary>
    /// Распознать текст нечитаемого PDF-файла.
    /// </summary>
    /// <param name="page">Нечитаемая PDF-страница.</param>
    /// <returns>Список вариантов распознанной страницы на разных языках.</returns>
    private static IEnumerable<StringBuilder> RecognizePageText(PdfPage page)
    {
      var recognizedPages = new List<StringBuilder>();
      
      var options = PdfDrawOptions.Create();
      options.BackgroundColor = new PdfRgbColor(255, 255, 255);
      options.HorizontalResolution = 300;
      options.VerticalResolution = 300;

      // var pageImageName = $"{Path.GetTempFileName()}.png";
      using var memoryStream = new MemoryStream();
      page.Save(memoryStream, options);

      foreach (var language in AvailableLanguages)
      {
        var recognizedPageText = RecognizePageTextForLanguage(memoryStream.GetBuffer(), language, out _);
        recognizedPages.Add(recognizedPageText);
      }
      // File.Delete(pageImageName);
      return recognizedPages;
    }

    /// <summary>
    /// Распознать текст из не читаемого PDF.
    /// </summary>
    /// <param name="imageBytes">Путь к изображенияю страницы для распознавания.</param>
    /// <param name="language">Язык, который нужно распознать.</param>
    /// <param name="confidence">Доверие к результату распознавания.</param>
    /// <returns>Текст.</returns>
    private static StringBuilder RecognizePageTextForLanguage(byte[] imageBytes, string language, out double confidence)
    {
      var recognizedPages = new StringBuilder();
      
      using var engine = new TesseractEngine("tessdata", language, EngineMode.Default);
      using var img = Pix.LoadFromMemory(imageBytes);
      using var recognizedRusPage = engine.Process(img);
      
      recognizedPages.Append(recognizedRusPage.GetText());
      confidence = recognizedRusPage.GetMeanConfidence();
      
      Log.Debug($"Доверие к распознанному тексту на английском языке {confidence}");

      return recognizedPages;
    }
  }
}