﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Library.Entity;
using Library.Utils;
using Newtonsoft.Json;
using NLog;

namespace Library
{
  /// <summary>
  /// Менеджер книг. Управляет движение книгв библиотеке.
  /// </summary>
  public sealed class BookManager
  {
    #region Константы

    /// <summary>
    /// Имя директории книжной полки.
    /// </summary>
    private const string BookShelfName = ".library";
    
    /// <summary>
    /// Расширение файла данных книги.
    /// </summary>
    private const string BookDataExtension = ".json";

    #endregion
    
    #region Поля и свойства

    private static BookManager _instance;

    /// <summary>
    /// Инстанс менеджера книг.
    /// </summary>
    public static BookManager Instance => _instance ??= new BookManager();
    
    /// <summary>
    /// Путь к директории книжной полки.
    /// </summary>
    public DirectoryInfo BookShelfPath { get; private set; }

    /// <summary>
    /// Счётчик книг в библиотеке.
    /// </summary>
    public static int BooksCounter => ElasticProvider.Instance.GetBooksCount();

    /// <summary>
    /// Логгер класса.
    /// </summary>
    private static readonly Logger Log  = LogManager.GetCurrentClassLogger();
    
    #endregion

    #region Методы

    /// <summary>
    /// Копировать книгу в Книжную полку.
    /// </summary>
    /// <param name="bookPath">Путь к книге.</param>
    public void AddBook(string bookPath)
    {
      try
      {
        var filePath = new FileInfo(bookPath);
        var bookNewPath = Path.Combine(BookShelfPath.FullName, filePath.Name);
        if (!File.Exists(bookNewPath))
        {
          File.Copy(filePath.FullName, bookNewPath);
          SerializeBook(bookNewPath);
        }
        else
        {
          Log.Debug($"The book to add already exists: {filePath.Name}");
        }
      }
      catch (Exception e)
      {
        Log.Error(e);
        throw;
      }
    }

    /// <summary>
    /// Сериализовать книгу в файл.
    /// </summary>
    /// <param name="bookPath">Путь к файлу для сериализации.</param>
    private static void SerializeBook(string bookPath)
    {
      var bookEntityForSerializing = new Book(GetNextId(), Path.GetFileNameWithoutExtension(bookPath),
        TextLayerExtractor.ExtractTextLayer(bookPath));

      var serializedBookDestinationPath = Path.Combine((new FileInfo(bookPath)).DirectoryName ?? string.Empty,
        Path.GetFileNameWithoutExtension(bookPath) + BookDataExtension);
      
      var serializer = new JsonSerializer();
      using var streamWriter = new StreamWriter(serializedBookDestinationPath);
      using var jsonWriter = new JsonTextWriter(streamWriter);
      
      serializer.Serialize(jsonWriter, bookEntityForSerializing);
    }

    /// <summary>
    /// Убрать книгу с книжной полки.
    /// </summary>
    /// <param name="bookName">Название книги.</param>
    /// <returns>True, если удалось успешно удалить книгу.</returns>
    public bool DeleteBook(string bookName)
    {
      if (string.IsNullOrEmpty(bookName))
        return false;
      
      var filePath = new FileInfo(Path.Combine(BookShelfPath.FullName, $"{bookName}.pdf"));
      if (BookShelfPath.GetFiles().Any(f => f.Name == filePath.Name))
      {
        File.Delete(filePath.FullName);
        File.Delete(Path.Combine(filePath.Directory?.ToString()!, $"{Path.GetFileNameWithoutExtension(filePath.Name)}.json"));
        return true;
      }
      
      return false;
    }

    /// <summary>
    /// Получить все книги с книжной полки.
    /// </summary>
    /// <returns>Коллекция наименований книг.</returns>
    public IEnumerable<Book> GetAllBooks()
    {
      var books = new List<Book>();
      foreach (var pdfFile in BookShelfPath.GetFiles("*.pdf"))
      {
        foreach (var bookDataFile in BookShelfPath.GetFiles($"*{BookDataExtension}"))
        {
          if (Path.GetFileNameWithoutExtension(pdfFile.Name) == Path.GetFileNameWithoutExtension(bookDataFile.Name))
          {
            using var reader = new StreamReader(bookDataFile.FullName);
            var book = JsonConvert.DeserializeObject<Book>(reader.ReadToEnd());
            books.Add(book);
          }
        }
      }

      return books;
    }

    /// <summary>
    /// Получить новый Id для сущности.
    /// </summary>
    /// <returns>Id.</returns>
    public static int GetNextId()
    {
      return BooksCounter + 1;
    }
    
    /// <summary>
    /// Проверить, что книга существует в библиотеке.
    /// </summary>
    /// <param name="bookName"></param>
    /// <returns>True, если в папке с книгами есть книга с таким же именем</returns>
    public bool IsBookExisted(string bookName)
    {
      foreach (var file in BookShelfPath.GetFiles())
      {
        if (Path.GetFileNameWithoutExtension(file.FullName) == bookName)
          return true;
      }

      return false;
    }
    
    /// <summary>
    /// Убедиться в наличии директории.
    /// </summary>
    private static void EnsureDirectory(string path)
    {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Приватный конструктор.
    /// </summary>
    private BookManager()
    {
      var bookShelfPath = Path.Combine(Directory.GetCurrentDirectory(), BookShelfName);
      EnsureDirectory(bookShelfPath);
      
      BookShelfPath = new DirectoryInfo(bookShelfPath)
      {
        Attributes = FileAttributes.Hidden | FileAttributes.Directory
      };
    }

    #endregion
  }
}