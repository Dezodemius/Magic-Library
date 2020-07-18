using System;
using System.Collections.Generic;
using Library.Entity;
using Nest;
using NLog;
using NLog.Fluent;

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
    private ElasticClient Client { get; set; }

    /// <summary>
    /// Логгер класса.
    /// </summary>
    private readonly Logger Log = LogManager.GetCurrentClassLogger();

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
        Log.Debug($"Searching: {searchPhrase} in index: {DefaultIndex}");
        response = Client.Search<Book>(s => s
          .Query(q => q
            .MultiMatch(c => c
              .Query(searchPhrase)
              .Fields(f => f.Field(b => b.Text)))));
      }
      catch (Exception e)
      {
        Log.Error(e, e.StackTrace);
        throw;
      }

      return response;
    }
    
    /// <summary>
    /// Найти все книги в индексе.
    /// </summary>
    public IEnumerable<Book> SearchAll()
    {
      var response = Client.Search<Book>(s => s.Query(q => q.MatchAll()));
      Log.Debug($"Search all. Found {response.Documents.Count} documents.");
      return response.Documents;
    }

    /// <summary>
    /// Получить шаблон настрок ES.
    /// </summary>
    /// <returns>Шаблон индекса.</returns>
    public string GetTemplate()
    {
      var template = Client.Indices.GetTemplate();
      Log.Debug(template.ToString());
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
      
      Log.Debug($"{book.Name} is indexed: {indexResponse.IsValid}.");
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
          Log.Debug("Failed to index document {0}: {1}", itemWithError.Id, itemWithError.Error);
        }
      }
      else
      {
        Log.Debug("Bulk Indexing. All books indexed.");
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
      
      Log.Debug($"Book {book.Name} deleted: {response.IsValid}");
      return response.IsValid;
    }

    /// <summary>
    /// Получить
    /// </summary>
    /// <returns></returns>
    public int GetBooksCount()
    {
      var response = Client.Count<Book>();

      Log.Debug($"Books count: {response.Count}");
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
        Log.Info($"Index {DefaultIndex} created.");
      }
      else
      {
        Log.Info($"Index '{DefaultIndex}' already exists.");
      }
    }
    
    #endregion
  }
}