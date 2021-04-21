using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Library.Client.Model;
using Library.Client.Utils;
using Library.Entity;
using Library.Utils;
using Microsoft.Win32;
using Nest;
using NLog;

namespace Library.Client.ViewModel
{
  /// <summary>
  /// ViewModel поискового окна.
  /// </summary>
  public class SearchViewModel : ViewModel
  {
    #region Поля и свойства

    /// <summary>
    /// Логгер.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Имя окна.
    /// </summary>
    public override string Name { get; } = "Поиск";

    /// <summary>
    /// Коллекция найденных книг.
    /// </summary>
    public ObservableCollection<BookWithPages> FoundedBooks { get; set; } = new ObservableCollection<BookWithPages>();

    private string _message;

    /// <summary>
    /// Сообщение.
    /// </summary>
    public string Message
    {
      get => _message;
      set
      {
        _message = value;
        OnPropertyChanged(nameof(Message));
      }
    }

    /// <summary>
    /// Поисковая фраза.
    /// </summary>
    public static string SearchPhrase { get; set; }

    #endregion

    #region Команды

    private DelegateCommand _searchCommand;

    /// <summary>
    /// Команда поиска.
    /// </summary>
    public DelegateCommand SearchCommand =>
      _searchCommand ??= new DelegateCommand(Search, SearchCanExecute);

    private DelegateCommand _addBookCommand;

    /// <summary>
    /// Команда добавления книги.
    /// </summary>
    public DelegateCommand AddBookCommand =>
      _addBookCommand ??= new DelegateCommand(AddBook, AddBookCanExecute);

    private DelegateCommand _getAllBooksCommand;

    /// <summary>
    /// Команда получения всех книг.
    /// </summary>
    public DelegateCommand GetAllBooksCommand =>
      _getAllBooksCommand ??= new DelegateCommand(GetAllBooks, GetAllBooksCanExecute);

    private DelegateCommand _deleteBookCommand;

    /// <summary>
    /// Команда удаления книги.
    /// </summary>
    public DelegateCommand DeleteBookCommand =>
      _deleteBookCommand ??= new DelegateCommand(DeleteBook, DeleteBookCanExecute);

    private DelegateCommand _openBookCommand;

    /// <summary>
    /// Команда открытия книги.
    /// </summary>
    public DelegateCommand OpenBookCommand =>
      _openBookCommand ??= new DelegateCommand(OpenBook, OpenBookCanExecute);

    private DelegateCommand _openBookHighlightsCommand;
    /// <summary>
    /// Команда открытия Highlights.
    /// </summary>
    public DelegateCommand OpenBookHighlightsCommand =>
      _openBookHighlightsCommand ??= new DelegateCommand(OpenBookHighlights, OpenBookHighlightsCanExecute);

    #endregion

    #region Методы

    /// <summary>
    /// Возможность выполнения поиска.
    /// </summary>
    /// <param name="arg">Аргументы.</param>
    /// <returns>Признак того, что поиск может выполняться.</returns>
    private bool SearchCanExecute(object arg)
    {
      return ElasticProvider.Instance.CheckElasticsearchConnection();
    }

    /// <summary>
    /// Выполнить поиск.
    /// </summary>
    /// <param name="obj">Объект.</param>
    private void Search(object obj)
    {
      FoundedBooks.Clear();
      if (string.IsNullOrEmpty(SearchPhrase))
        return;
      var searchResponse = ElasticProvider.Instance.Search(SearchPhrase);
      
      var currentBook = new BookWithPages();
      foreach (var hit in searchResponse.Hits)
      {
        var bookId = hit.Fields.Value<Guid>(new Field("bookId"));
        var pageNumber = hit.Fields.Value<float>(new Field("number"));
        if (FoundedBooks.All(b => b.Id != bookId))
        {
          var book = BookManager.Instance.GetBook(bookId);
          if (book == null)
            continue;
          currentBook = new BookWithPages(book, new List<float>());
          FoundedBooks.Add(currentBook);
        }
        currentBook.Pages.Add(pageNumber);
        currentBook.Highlights.Add(new HighlightWithPages {Highlight = hit.Highlight, Page = pageNumber});
      }
      UpdateMessageTextBox($"Найдено экземпляров: {FoundedBooks.Count}");
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
      return ElasticProvider.Instance.CheckElasticsearchConnection();
    }

    /// <summary>
    /// Добавить книгу.
    /// </summary>
    /// <param name="obj">Объект.</param>
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

    /// <summary>
    /// Получить все книги.
    /// </summary>
    /// <param name="obj">Объект.</param>
    private void GetAllBooks(object obj)
    {
      var foundedBooks = ElasticProvider.Instance.GetAllBooks();

      FoundedBooks.Clear();
      foreach (var book in foundedBooks)
        FoundedBooks.Add(new BookWithPages(book, new List<float>()));
    }

    /// <summary>
    /// Проверить возможность выполнить удаление книги.
    /// </summary>
    /// <param name="arg">Аргумент.</param>
    /// <returns>Признак того, что книгу можно удалить.</returns>
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
        var filepath = Path.Combine(BookManager.Instance.BookShelfPath.FullName, book.Name + ".pdf");
        System.Diagnostics.Process.Start(filepath);
        UpdateMessageTextBox($"Открыта {book.Name}");
      }
    }
    
    /// <summary>
    /// Возможность открыть Highlights книги.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns>True, если возможно.</returns>
    private static bool OpenBookHighlightsCanExecute(object arg)
    {
      if (arg is BookWithPages book)
        return BookManager.Instance.IsBookExisted(book) && book.Pages.Any() && book.Highlights.Any();
      return false;
    }

    /// <summary>
    /// Открытие Hihglights книги.
    /// </summary>
    /// <param name="obj">Объект книги.</param>
    private void OpenBookHighlights(object obj)
    {
      if (obj is BookWithPages book)
      {
        var highlightsViewModel = new HighlightsViewModel
        {
            SelectedBook = book.Highlights
        };
        ViewService.OpenViewModel(highlightsViewModel, book, 1024, 768);
        UpdateMessageTextBox($"Открыта страница с Highlights книги {book.Name}");
      }
    }

    #endregion
  }
}
