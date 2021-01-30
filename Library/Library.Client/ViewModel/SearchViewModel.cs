using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Library.Client.Utils;
using Library.Entity;
using Library.Utils;
using Microsoft.Win32;
using Nest;

namespace Library.Client.ViewModel
{
  /// <summary>
  /// ViewModel главного окна.
  /// </summary>
  public class SearchViewModel : ViewModel
  {
    #region Поля и свойства

    public override string Name { get; } = "Поиск";

    public ObservableCollection<BookWithPages> FoundedBooks { get; set; } = new ObservableCollection<BookWithPages>();

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
      if (string.IsNullOrEmpty(SearchPhrase))
        return;
      var searchResponse = ElasticProvider.Instance.Search(SearchPhrase);
      var booksWithPages = new Dictionary<Guid, List<float>>();
      foreach (var field in searchResponse.Fields)
      {
        var bookId = field.Value<Guid>(new Field("bookId"));
        var page = field.Value<float>(new Field("number"));
        if (!booksWithPages.ContainsKey(bookId))
          booksWithPages.Add(bookId, new List<float>());
        booksWithPages[bookId].Add(page);
      }
      UpdateMessageTextBox($"Найдено экземпляров: {booksWithPages.Count}");

      FoundedBooks.Clear();
      foreach (var bookId in booksWithPages.Keys)
      {
        var book = BookManager.Instance.GetBook(bookId);
        if (book == null)
          throw new NullReferenceException($"Book with ID {bookId} does not exist localy.");
        var pages = string.Join(", ", booksWithPages[bookId]);
        FoundedBooks.Add(new BookWithPages(book, pages));
      }
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
        Filter = "Книги (*.PDF)|*.PDF",
        Multiselect = true
      };

      if (openFileDialog.ShowDialog() != true)
        return;

      var booksForIndexing = new List<Book>();
      foreach (var pathToFile in openFileDialog.FileNames)
      {
        var bookId = Guid.NewGuid();
        BookManager.Instance.AddBook(pathToFile, bookId);
        var book = new Book(bookId, Path.GetFileNameWithoutExtension(pathToFile));

        booksForIndexing.Add(book);
        AppendToMessageTextBox($"{book.Name} успешно добавлена");
    
        var pages = TextLayerExtractor.GetTextLayerWithPages(pathToFile, book.Id);
        ElasticProvider.Instance.BulkIndex(pages);
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
      var foundedBooks = ElasticProvider.Instance.GetAllBooks();

      FoundedBooks.Clear();
      foreach (var book in foundedBooks)
        FoundedBooks.Add(new BookWithPages(book, string.Empty));
    }

    private static bool DeleteBookCanExecute(object arg)
    {
      if (arg is Book book)
        return BookManager.Instance.IsBookExisted(book);
      return false;
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
        if (ElasticProvider.Instance.DeleteBookWithPages(book))
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
      if (arg is Book book)
        return BookManager.Instance.IsBookExisted(book);
      return false;
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
