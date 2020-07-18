using System;
using System.Collections.Generic;
using Library.Entity;
using Nest;
using NLog;

namespace Library.Utils
{
  /// <summary>
  /// Провайдер Elasticsearch NEST.
  /// </summary>
  public class ElasticProvider
  {
    #region Константы
    
    /// <summary>
    /// Имя индекса по умолчанию.
    /// </summary>
    private const string DefaultIndex = "books";

    #endregion
    
    #region Поля и свойства

    private static ElasticProvider _instance;

    /// <summary>
    /// Экземпляр ElasticProvider.
    /// </summary>
    public static ElasticProvider Instance => _instance ??= new ElasticProvider();

    /// <summary>
    /// Экземпляр клиента Elasticsearch.
    /// </summary>
    private ElasticClient Client { get; }

    /// <summary>
    /// Логгер класса.
    /// </summary>
    private readonly Logger _log = LogManager.GetCurrentClassLogger();

    #endregion
    
    #region Методы

    /// <summary>
    /// Выполнить поиск.
    /// </summary>
    /// <param name="searchPhrase">Поисковая фраза.</param>
    public ISearchResponse<Book> Search(string searchPhrase)
    {
      ISearchResponse<Book> response;
      try
      {
        _log.Debug($"Searching: {searchPhrase} in index: {DefaultIndex}");
        response = Client.Search<Book>(s => s
          .Query(q => q
            .MultiMatch(c => c
              .Query(searchPhrase)
              .Fields(f => f.Field(b => b.Text)))));
      }
      catch (Exception e)
      {
        _log.Error(e, e.StackTrace);
        throw;
      }

      return response;
    }
    
    /// <summary>
    /// Найти все книги в индексе.
    /// </summary>
    public IEnumerable<Book> GetAll()
    {
      var response = Client.Search<Book>(s => s.Query(q => q.MatchAll()));
      _log.Debug($"Search all. Found {response.Documents.Count} documents.");
      return response.Documents;
    }

    /// <summary>
    /// Получить шаблон настрок ES.
    /// </summary>
    /// <returns>Шаблон индекса.</returns>
    public string GetTemplate()
    {
      var template = Client.Indices.GetTemplate();
      _log.Debug(template.ToString());
      return template.ToString();
    }

    /// <summary>
    /// Индексировать документ.
    /// </summary>
    /// <param name="book">Экземпляр книги.</param>
    public void Index(Book book)
    {
      var indexResponse = Client
        .Index(book, i => i
          .Index(DefaultIndex));
      
      _log.Debug($"{book.Name} is indexed: {indexResponse.IsValid}.");
    }

    /// <summary>
    /// Индексировать несколько документов.
    /// </summary>
    /// <param name="books">Список книг.</param>
    public void BulkIndex(IEnumerable<Book> books)
    {
      var bulkRequest = Client
        .Bulk(b => b
          .Index(DefaultIndex)
          .IndexMany(books));

      if (bulkRequest.Errors)
      {
        foreach (var itemWithError in bulkRequest.ItemsWithErrors)
        {
          _log.Debug("Failed to index document {0}: {1}", itemWithError.Id, itemWithError.Error);
        }
      }
      else
      {
        _log.Debug("Bulk Indexing. All books indexed.");
      }
    }

    /// <summary>
    /// Удалить несколько документов.
    /// </summary>
    /// <param name="books">Список книг.</param>
    public void BulkDelete(IEnumerable<Book> books)
    {
      var bulkRequest = Client
        .Bulk(b => b
          .Index(DefaultIndex)
          .DeleteMany(books));

      if (bulkRequest.Errors)
      {
        foreach (var itemWithError in bulkRequest.ItemsWithErrors)
        {
          _log.Debug("Failed to delete document {0}: {1}", itemWithError.Id, itemWithError.Error);
        }
      }
      else
      {
        _log.Debug("Bulk delete. All books deleted.");
      }
    }
    
    /// <summary>
    /// Удалить книгу из индекса.
    /// </summary>
    /// <param name="book">Экземпляр книги.</param>
    public bool DeleteBook(Book book)
    {
      var response = Client.DeleteByQuery<Book>(q => q
        .Query(rq => rq
          .Term(f => f.Id, book.Id)));
      
      _log.Debug($"Book {book.Name} deleted: {response.IsValid}");
      return response.IsValid;
    }

    /// <summary>
    /// Получить
    /// </summary>
    /// <returns></returns>
    public int GetBooksCount()
    {
      var response = Client.Count<Book>();

      _log.Debug($"Books count: {response.Count}");
      return (int)response.Count;
    }
    
    #endregion

    #region Конструкторы
    
    /// <summary>
    /// Приватный конструктор.
    /// </summary>
    private ElasticProvider()
    {
      var setting = new ConnectionSettings().DefaultIndex(DefaultIndex);
      
      Client = new ElasticClient(setting);
      
      if (!Client.Indices.Exists(DefaultIndex).Exists)
      {
        Client.Indices.Create(DefaultIndex, i => i.Map<ElasticBook>(m => m.AutoMap()));
        _log.Info($"Index {DefaultIndex} created.");
      }
      else
      {
        _log.Info($"Index '{DefaultIndex}' already exists.");
      }
    }
    
    #endregion
  }
}