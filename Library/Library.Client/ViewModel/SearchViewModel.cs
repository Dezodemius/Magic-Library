using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Library.Client.Utils;
using Library.Entity;
using Library.Utils;
using Microsoft.Win32;

namespace Library.Client.ViewModel
{
  /// <summary>
  /// ViewModel главного окна.
  /// </summary>
  public class SearchViewModel : ViewModel
  {
    #region Поля и свойства

    public override string Name { get; } = "Поиск";

    public ObservableCollection<Book> FoundedBooks { get; set; } = new ObservableCollection<Book>();

    private string _message;

    public string Message
    {
      get => _message;
      set
      {
        _message = value;
        OnPropertyChanged(nameof(Message));
      }
    }

    public static string SearchPhrase { get; set; }

    #endregion

    #region Команды

    private DelegateCommand _searchCommand;

    /// <summary>
    /// Команда поиска.
    /// </summary>
    public DelegateCommand SearchCommand =>
      _searchCommand ?? (_searchCommand = new DelegateCommand(Search, SearchCanExecute));

    private DelegateCommand _addBookCommand;

    /// <summary>
    /// Команда добавления книги.
    /// </summary>
    public DelegateCommand AddBookCommand =>
      _addBookCommand ?? (_addBookCommand = new DelegateCommand(AddBook, AddBookCanExecute));

    private DelegateCommand _getAllBooksCommand;

    /// <summary>
    /// Команда получения всех книг.
    /// </summary>
    public DelegateCommand GetAllBooksCommand =>
      _getAllBooksCommand ?? (_getAllBooksCommand = new DelegateCommand(GetAllBooks, GetAllBooksCanExecute));

    private DelegateCommand _deleteBookCommand;

    /// <summary>
    /// Команда удаления книги.
    /// </summary>
    public DelegateCommand DeleteBookCommand =>
      _deleteBookCommand ?? (_deleteBookCommand = new DelegateCommand(DeleteBook, DeleteBookCanExecute));

    private DelegateCommand _openBookCommand;

    /// <summary>
    /// Команда открытия книги.
    /// </summary>
    public DelegateCommand OpenBookCommand =>
      _openBookCommand ?? (_openBookCommand = new DelegateCommand(OpenBook, OpenBookCanExecute));

    #endregion

    #region Методы

    private bool SearchCanExecute(object arg)
    {
      return true;
    }

    private void Search(object obj)
    {
      var documents = ElasticProvider.Instance.Search(SearchPhrase).Documents;

      UpdateMessageTextBox($"Найдено экземпляров: {documents.Count}");

      FoundedBooks.Clear();
      foreach (var result in documents)
        FoundedBooks.Add(result);
    }

    /// <summary>
    /// Обновить TextBox с сообщением.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    private void UpdateMessageTextBox(string message)
    {
      Message = message;
    }

    /// <summary>
    /// Добавить сообщение в TextBox с сообщением.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    private void AppendToMessageTextBox(string message)
    {
      Message = Message + Environment.NewLine + message;
    }

    /// <summary>
    /// Возможность добавить книгу.
    /// </summary>
    /// <param name="arg">Аргумент.</param>
    /// <returns>True, если возможно.</returns>
    private static bool AddBookCanExecute(object arg)
    {
      return true;
    }

    private void AddBook(object obj)
    {
      UpdateMessageTextBox("Начало добавления книг");
      var openFileDialog = new OpenFileDialog
      {
        Title = "Выберите файлы для индексации",
        Filter = "Книги (*.PDF)|*.PDF"
      };

      if (openFileDialog.ShowDialog() != true)
        return;

      var booksForIndexing = new List<Book>();
      foreach (var pathToFile in openFileDialog.FileNames)
      {
        BookManager.Instance.AddBook(pathToFile);
        var book = new Book(BookManager.GetNextId(), Path.GetFileNameWithoutExtension(pathToFile),
          TextLayerExtractor.ExtractTextLayer(pathToFile));

        booksForIndexing.Add(book);
        AppendToMessageTextBox($"{book.Name} успешно добавлена");
      }

      ElasticProvider.Instance.BulkIndex(booksForIndexing);
      AppendToMessageTextBox("Добавление книг завершено");
    }

    /// <summary>
    /// Возможность получить все книги.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns>True, если возможно.</returns>
    private static bool GetAllBooksCanExecute(object arg)
    {
      return true;
    }

    private void GetAllBooks(object obj)
    {
      var foundedBooks = ElasticProvider.Instance.GetAll();

      FoundedBooks.Clear();
      foreach (var book in foundedBooks)
        FoundedBooks.Add(book);
    }

    private static bool DeleteBookCanExecute(object arg)
    {
      return BookManager.Instance.IsBookExisted((arg as Book)?.Name);
    }

    /// <summary>
    /// Удаление книги.
    /// </summary>
    /// <param name="obj">Объект книги.</param>
    /// <remarks>Доступ из View взят отсюда:
    /// https://stackoverflow.com/questions/11082162/context-menu-for-removing-items-in-listview.</remarks>
    private void DeleteBook(object obj)
    {
      if (obj is Book book)
      {
        var messageText = string.Empty;

        if (BookManager.Instance.DeleteBook(book.Name))
          messageText += $"{book.Name} - успешно удалена с диска.\n";
        if (ElasticProvider.Instance.DeleteBook(book))
          messageText += $"{book.Name} - успешно удалена с индекса.\n";

        AppendToMessageTextBox(messageText);
      }
    }

    /// <summary>
    /// Возможность открыть книгу.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns>True, если возможно.</returns>
    private static bool OpenBookCanExecute(object arg)
    {
      return BookManager.Instance.IsBookExisted((arg as Book)?.Name);
    }

    /// <summary>
    /// Открытие книги.
    /// </summary>
    /// <param name="obj">Объект книги.</param>
    private void OpenBook(object obj)
    {
      if (obj is Book book)
      {
        System.Diagnostics.Process.Start(Path.Combine(BookManager.Instance.BookShelfPath.FullName,
          book.Name + ".pdf"));
        UpdateMessageTextBox($"Открыта {book.Name}");
      }
    }

    #endregion
  }
}