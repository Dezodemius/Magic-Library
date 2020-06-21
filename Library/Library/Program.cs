using System;
using System.Linq;
using Library.Entity;

namespace Library
{
  /// <summary>
  /// Основной класс для работы.
  /// </summary>
  public static class Program
  {
    /// <summary>
    /// Меню.
    /// </summary>
    private static void Menu()
    { 
      var esProvider = new ElasticProvider();

      var userInput = new ConsoleKeyInfo();
      while (userInput.Key != ConsoleKey.Escape)
      {
        userInput = Console.ReadKey(false);
        try
        {
          switch (userInput.Key)
          {
            case ConsoleKey.S:
              Console.Write("Введите искомое слово: ");
              var searchPhrase = Console.ReadLine();
              var foundBook = esProvider.Search(searchPhrase);
              Console.WriteLine(foundBook.Documents.FirstOrDefault()?.Name);
              break;
            case ConsoleKey.A:
              Console.Write("Введите путь к файлу: ");
              var bookPath = Console.ReadLine();
              var textLayer = TextLayerExtractor.ExtractFromPdf(bookPath);
              esProvider.Index(new Book("Книга 1", textLayer));
              break;
            case ConsoleKey.B:
              var all = esProvider.SearchAll();
              foreach (var book in all)
                Console.WriteLine(book.Name);
              break;
            default:
              Console.WriteLine(LibraryConstants.HelpMessage);
              continue;
          }
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      }
    }
    
    /// <summary>
    /// Точка входа в программу.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    static void Main(string[] args)
    {
      Menu();
    }
  }
}