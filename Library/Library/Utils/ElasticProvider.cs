using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Library.Entity;
using Library.Resources;
using Nest;
using NLog;
using Page = Library.Entity.Page;

namespace Library.Utils
{
  /// <summary>
  /// Провайдер Elasticsearch NEST.
  /// </summary>
  public class ElasticProvider
  {
    #region Константы
    
    /// <summary>
    /// Имя индекса с книгами.
    /// </summary>
    private const string BooksIndexName = "books";
    
    /// <summary>
    /// Имя индекса со страницами.
    /// </summary>
    private const string PagesIndexName = "pages";
    
    /// <summary>
    /// Имя пайплайна ingest-attachment.
    /// </summary>
    private const string PipelineAttachmentName = "attachment";

    /// <summary>
    /// Имя ingest-attachment.
    /// </summary>
    private const string IngestAttachmentName = "ingest-attachment";
    
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

    #region Вспомогательные
    
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
    /// Получить все плагины ES.
    /// </summary>
    /// <returns>Список плагинов.</returns>
    private IEnumerable<CatPluginsRecord> GetPlugins()
    {
      var plugins = Client.Cat.Plugins();
      _log.Info(string.Join(Environment.NewLine, plugins.Records));
      return plugins.Records;
    }
    
    /// <summary>
    /// Проверить наличие требуемых плагинов в ES.
    /// </summary>
    /// <exception cref="RequiredPluginNotInstalledException">Возникает в случае отсутсвия требуемых плагинов.</exception>
    private void CheckRequiredPlugins()
    {
      if (GetPlugins().All(p => p.Component != IngestAttachmentName))
        throw new RequiredPluginNotInstalledException($"'{IngestAttachmentName}' not installed.");
    }

    /// <summary>
    /// Разместить pipeline ingest-attachment.
    /// </summary>
    private void PutPipeline()
    {
      Client.Ingest.PutPipeline(PipelineAttachmentName, p => p
        .Processors(ps => ps
          .Attachment<Page>(a => a
            .TargetField(PipelineAttachmentName)
            .Field(PipelineAttachmentName))));
      
      _log.Info($"{PipelineAttachmentName} pipeline was put.");
    }
    
    /// <summary>
    /// Получить количество проиндексированных книг.
    /// </summary>
    /// <returns>Число книг.</returns>
    public int GetBooksCount()
    {
      var response = Client.Count<Book>();

      _log.Debug($"Books count: {response.Count}");
      return (int)response.Count;
    }
    
    #endregion

    #region Индексация
    
    /// <summary>
    /// Индексировать книгу.
    /// </summary>
    /// <param name="book">Экземпляр книги.</param>
    public void Index(Book book)
    {
      var indexResponse = Client
        .Index(book, i => i
          .Index(Indices.Index(BooksIndexName))
          .Pipeline(PipelineAttachmentName));
      
      _log.Debug($"{book.Name} is indexed: {indexResponse.IsValid}.");
    }

    /// <summary>
    /// Индексировать страницу книги.
    /// </summary>
    /// <param name="page">Страница книги.</param>
    public void Index(Page page)
    {
      var indexResponse = Client
        .Index(page, i => i
          .Index(Indices.Index(PagesIndexName))
          .Pipeline(PipelineAttachmentName));
      
      _log.Debug($"Page #{page.Number} is indexed: {indexResponse.IsValid}.");
    }
    
    /// <summary>
    /// Индексировать несколько документов.
    /// </summary>
    /// <param name="books">Список книг.</param>
    public void BulkIndex(IEnumerable<Book> books)
    {
      var bulkRequest = Client
        .Bulk(b => b
          .Index(Indices.Index(BooksIndexName))
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
    /// Индексировать несколько документов.
    /// </summary>
    /// <param name="pages">Список книг.</param>
    public void BulkIndex(IEnumerable<Page> pages)
    {
      var bulkResponse = Client
        .Bulk(p => p
          .Index(Indices.Index(PagesIndexName))
          .IndexMany(pages)
          .Pipeline(PipelineAttachmentName));

      if (bulkResponse.Errors)
      {
        foreach (var itemWithError in bulkResponse.ItemsWithErrors)
        {
          _log.Debug("Failed to index document {0}: {1}", itemWithError.Id, itemWithError.Error);
        }
      }
      else
      {
        _log.Debug($"Bulk Indexing. {pages.Count()} indexed.");
      }
    }
    
    #endregion

    #region Поиск

    /// <summary>
    /// Выполнить поиск по книгам.
    /// </summary>
    /// <remarks>Поиск идёт относительно наименований книг.</remarks>
    /// <param name="searchPhrase">Поисковая фраза.</param>
    /// <returns>Результат поиска.</returns>
    public ISearchResponse<Page> Search(string searchPhrase)
    {
      using (var client = new HttpClient())
      {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new HttpRequestMessage(HttpMethod.Get, @"http://localhost:9200/pages/_search/")
        {
          Content = new StringContent(@"{
        ""query"": {
          ""match"": {""attachment.content"" : ""enhanced""}
        },
        ""highlight"": {""fields"" : {""attachment.content"" : {}, ""bookId"" : {}}}
      }")
        };
        var response1 = client.SendAsync(request);
        var a = response1.Result;
      }
      ISearchResponse<Page> response;
      try
      {
        _log.Debug($"Searching: {searchPhrase} in index: {PagesIndexName}");
        response = Client.Search<Page>(s => s.Index(Indices.Index(PagesIndexName))
          .Query(q => q
            .Match(c => c
              .Field(f =>  f.Attachment.Content)
              .Query(searchPhrase)))
          .Highlight(h => h
            .Fields(f => f.Field(b => b.Attachment.Content))));
      }
      catch (Exception e)
      {
        _log.Error(e, e.Message);
        throw;
      }
      return response;
    }
    
    /// <summary>
    /// Найти все сущности в индексе.
    /// </summary>
    /// <returns>Список всех сущностей в индексе.</returns>
    public IEnumerable<Book> GetAllBooks()
    {
      var response = Client.Search<Book>(s => s.Query(q => q.MatchAll()));
      _log.Debug($"Search all. Found {response.Documents.Count} documents.");
      return response.Documents;
    }
    
    /// <summary>
    /// Найти все страницы книг в индексе.
    /// </summary>
    /// <returns>Список всех страниц в индексе.</returns>
    public IEnumerable<Page> GetAllPages(Guid guid)
    {
      var response = Client.Search<Page>(s => s
        .Query(q => q
          .Term(m => m
            .Field(f => f.BookId)
            .Value(guid))));
      _log.Debug($"Search all. Found {response.Documents.Count} documents.");
      return response.Documents;
    }
    
    #endregion

    #region Удаление

    /// <summary>
    /// Удалить несколько документов.
    /// </summary>
    /// <param name="books">Список книг.</param>
    public void BulkDelete(IEnumerable<Book> books)
    {
      var bulkRequest = Client
        .Bulk(b => b
          .Index(BooksIndexName)
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
    /// <returns>True, если книга удалена успешно.</returns>
    public bool DeleteBook(Book book)
    {
      var response = Client.DeleteByQuery<Book>(q => q
        .Query(rq => rq
          .Term(f => f.Id, book.Id)));
      
      _log.Debug($"Book {book.Name} deleted: {response.IsValid}");
      return response.IsValid;
    }

    #endregion

    /// <summary>
    /// Создать и настроить индекс /books.
    /// </summary>
    private void CreateBooksIndex()
    {
      if (!Client.Indices.Exists(Indices.Index(BooksIndexName)).Exists)
      {
        Client.Indices.Create(Indices.Index(BooksIndexName),
          i => i
            .Map<Book>(m => m
              .AutoMap()));

        _log.Info($"Index {BooksIndexName} created.");
      }
      else
      {
        _log.Info($"Index '{BooksIndexName}' already exists.");
      }
    }
    
    /// <summary>
    /// Создать и настроить индекс /pages.
    /// </summary>
    private void CreatePagesIndex()
    {
      if (!Client.Indices.Exists(Indices.Parse(PagesIndexName)).Exists)
      {
        Client.Indices.Create(Indices.Index(PagesIndexName),
          i => i
            .Map<Page>(m => m
              .AutoMap()
              ));

        _log.Info($"Index {PagesIndexName} created.");
      }
      else
      {
        _log.Info($"Index '{PagesIndexName}' already exists.");
      }
    }
    
    #endregion

    #region Конструкторы
    
    /// <summary>
    /// Приватный конструктор.
    /// </summary>
    private ElasticProvider()
    {
      var settings = new ConnectionSettings(new Uri(LibraryResources.ElasticDefaultAddress))
        .DefaultIndex(PagesIndexName)
        .ThrowExceptions();
      
      Client = new ElasticClient(settings);

      CreateBooksIndex();
      CreatePagesIndex();
      
      CheckRequiredPlugins();
      PutPipeline();
    }

    #endregion
  }
}