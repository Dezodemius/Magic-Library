using System;
using System.Collections.Generic;
using System.IO;
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
    /// Клиент Elasticsearch.
    /// </summary>
    private static ElasticProvider _esProvider;

    /// <summary>
    /// Меню.
    /// </summary>
    private static void Menu()
    {
      Console.WriteLine(LibraryConstants.HelpMessage);
      var userInput = new ConsoleKeyInfo();
      while (userInput.Key != ConsoleKey.Escape)
      {
        userInput = Console.ReadKey(false);
        try
        {
          switch (userInput.Key)
          {
            case ConsoleKey.S:
              Search();
              break;
            case ConsoleKey.A:
              IndexBook();
              break;
            case ConsoleKey.D:
              BulkIndexing();
              break;
            case ConsoleKey.W:
              GetAllBooks();
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
    /// Получить все книги в индексе.
    /// </summary>
    private static void GetAllBooks()
    {
      var books = _esProvider.SearchAll();
      if (!books.Any())
        Console.WriteLine("Нет книг.");
      foreach (var book in books)
        Console.WriteLine(book.Name);
    }

    /// <summary>
    /// Индексировать книгу.
    /// </summary>
    private static void IndexBook()
    {
      Console.Write("Введите путь к файлу: ");
      var bookPath = Console.ReadLine();
      var bookForIndexing = TextLayerExtractor.FromPdfToBook(bookPath);
      _esProvider.Index(bookForIndexing);
    }

    /// <summary>
    /// Искать книгу по фразе.
    /// </summary>
    private static void Search()
    {
      Console.Write("Введите искомое слово: ");
      var searchPhrase = Console.ReadLine();
      var documents = _esProvider.Search(searchPhrase).Documents;
      foreach (var result in documents)
        Console.WriteLine(result.Name);
    }

    /// <summary>
    /// Bulk-индексация книг.
    /// </summary>
    private static void BulkIndexing()
    {
      var booksForIndexing = new List<Book>();
      while (true)
      {
        try
        {
          Console.Write("Введите путь к книге: ");
          var path = Console.ReadLine();
          if (string.IsNullOrEmpty(path))
            break;
          var fileInfo = new FileInfo(path);
          if (!fileInfo.Exists)
            Console.WriteLine("Путь не существует.");
          else
            booksForIndexing.Add(TextLayerExtractor.FromPdfToBook(fileInfo.FullName));
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      }
      _esProvider.BulkIndex(booksForIndexing);
    }

    /// <summary>
    /// Точка входа в программу.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    static void Main(string[] args)
    {
      _esProvider = new ElasticProvider();
      Menu();
    }
  }
}