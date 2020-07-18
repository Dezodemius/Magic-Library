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
    public DirectoryInfo BookShelfPath { get; }

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
    /// <returns>True, если удалось копировать книгу. False, если книга уже существует на полке.</returns>
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
      var bookEntityForSerializing = new Book
      {
        Name = Path.GetFileNameWithoutExtension(bookPath),
        Text = TextLayerExtractor.ExtractTextLayer(bookPath)
      };
      
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
      
      var filePath = new FileInfo(Path.Combine(BookShelfPath.FullName, bookName));
      if (!BookShelfPath.GetFiles().Contains(filePath))
        return false;

      File.Delete(filePath.FullName);

      return true;
    }

    /// <summary>
    /// Получить все книги с книжной полки.
    /// </summary>
    /// <returns>Коллекция наименований книг.</returns>
    public IEnumerable<string> GetAllBooks()
    {
      var bookNames = new List<string>();
      foreach (var file in BookShelfPath.GetFiles())
        if (file.Extension == "pdf")
          bookNames.Add(file.Name);

      return bookNames;
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
    private void EnsureDirectory(string path)
    {
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
        BookShelfPath.Attributes = FileAttributes.Hidden | FileAttributes.Directory;
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Приватный конструктор.
    /// </summary>
    private BookManager()
    {
      BookShelfPath = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), BookShelfName));
      EnsureDirectory(BookShelfPath.FullName);
    }

    #endregion
  }
}