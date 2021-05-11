using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Логгер класса.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static readonly object Lock = new object();

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
      
      var pages = new ConcurrentQueue<Page>();
      var reader = new PdfReader(pdfPath);
      var numberOfPages = reader.NumberOfPages;
      var exceptions = new ConcurrentQueue<Exception>();
      
      Parallel.For(1, numberOfPages + 1, new ParallelOptions
      {
          MaxDegreeOfParallelism = Environment.ProcessorCount * 4
      },i =>
      {
        try
        {
          string text;
          lock (Lock)
          {
            text = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy()).Normalize();
          }
          if (string.IsNullOrEmpty(text))
          {
            using var pdfDocument = new PdfDocument(pdfPath);
            var page = pdfDocument.Pages[i - 1];
            page.Rotation = PdfRotation.None;
            var recognizedPageText = RecognizePageText(page);

            pages.Enqueue(new Page(i, bookId, GetStringInBase64(recognizedPageText)));
          }
          else
            pages.Enqueue(new Page(i, bookId, GetStringInBase64(text.Trim())));

          progressAction?.Invoke((double)1 / numberOfPages);
        }
        catch (Exception e)
        {
          exceptions.Enqueue(e);
        }
      });

      if (exceptions.Any())
        throw new AggregateException(exceptions);
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
    /// <returns>Распознанный текст со страницы.</returns>
    private static string RecognizePageText(PdfPage page)
    {
      var options = PdfDrawOptions.Create();
      options.BackgroundColor = new PdfRgbColor(255, 255, 255);
      options.HorizontalResolution = 200;
      options.VerticalResolution = 200;

      using var memoryStream = new MemoryStream();
      page.Save(memoryStream, options);
      
      using var engine = new TesseractEngine(@"tessdata\fast", "rus+eng", EngineMode.LstmOnly);
      using var img = Pix.LoadFromMemory(memoryStream.GetBuffer());
      using var recognizedPage = engine.Process(img);

      return recognizedPage.GetText();
    }
  }
}