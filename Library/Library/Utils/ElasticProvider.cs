using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    #region Singltone

    private static ElasticProvider _instance;

    /// <summary>
    /// Экземпляр ElasticProvider.
    /// </summary>
    public static ElasticProvider Instance => _instance ??= new ElasticProvider();

    #endregion

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
    /// Инициализация клиента для работы с ES.
    /// </summary>
    public void Initialize()
    {
      CreateBooksIndex();
      CreatePagesIndex();
      
      CheckRequiredPlugins();
      PutPipeline();
    }

    /// <summary>
    /// Проверить соединение с сервисом ES.
    /// </summary>
    /// <returns></returns>
    public bool CheckElasticsearchConnection()
    {
      var pingResponse = Client.Ping();
      _log.Debug(pingResponse.ApiCall.Success);
      return pingResponse.ApiCall.Success;
    }
    
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
        var stopwords = this.GetStopwords();
        Client.Indices.Create(Indices.Index(PagesIndexName),
          i => i
            .Map<Page>(m => m
              .Properties(ps => ps
                .Number(n => n.Name(na => na.Number).Store())
                .Keyword(k => k.Name(n => n.BookId).Store())
                .Text(t => t
                  .Name(n => n.Attachment.Content)
                  .Analyzer("my_russian_morphology"))
                .Object<Attachment>(o => o
                  .Name(n => n.Attachment)
                  .Properties(p => p
                      .Text(t => t
                          .Name(n => n.Content)
                          .Analyzer("my_russian_morphology")
                          .Fields(f => f
                              .Text(t => t
                                  .Analyzer("standard")
                                  .Name("exact")))))
                  .AutoMap()))
              )
            .Settings(s => s
              .Analysis(a => a
                .TokenFilters(f => f
                  .Stop("morphology_stopwords", w => w.StopWords(stopwords)))
                .Analyzers(aa => aa
                  .Custom("my_russian_morphology", c => c
                    .Tokenizer("standard")
                    .Filters("lowercase", "russian_morphology", "english_morphology", "morphology_stopwords"))))));

        _log.Info($"Index {PagesIndexName} created.");
      }
      else
      {
        _log.Info($"Index '{PagesIndexName}' already exists.");
      }
    }

    /// <summary>
    /// Получить все стоп-слова для анализаторов.
    /// </summary>
    /// <returns>Стоп-слова.</returns>
    private StopWords GetStopwords()
    {
      using var reader = new StreamReader("stopwords.txt");
      return new StopWords(reader.ReadToEnd());
    }

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
          .Index(Indices.Index(BooksIndexName)));
      
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
    /// Выполнить поиск по страницам.
    /// </summary>
    /// <remarks>Поиск по содержанию страниц.</remarks>
    /// <param name="searchPhrase">Поисковая фраза.</param>
    /// <returns>Результат поиска.</returns>
    public ISearchResponse<Page> Search(string searchPhrase)
    {
      ISearchResponse<Page> response;
      try
      {
        _log.Debug($"Searching: {searchPhrase} in index: {PagesIndexName}");
        response = Client.Search<Page>(s => s.Index(Indices.Parse(PagesIndexName))
            .StoredFields(sf => sf.Field(f => f.BookId).Field(f => f.Number).Field(f => f.Attachment.Content))
            .Query(q => q.SimpleQueryString(sq => sq
                  .Query(searchPhrase)
                  .Fields(f => f.Field(p => p.Attachment.Content))
                  .Analyzer("my_russian_morphology")))
            .Sort(s => s
                .Ascending(f => f.BookId))
            .Size(10000)
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
      var searchRequest = new SearchRequest<Book>(Indices.Parse(BooksIndexName))
      {
        Query = new QueryContainer(new MatchAllQuery())
      };
      var response = Client.Search<Book>(searchRequest);
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
        foreach (var itemWithError in bulkRequest.ItemsWithErrors)
          _log.Debug($"Failed to delete document {itemWithError.Id}: {itemWithError.Error}");
      else
        _log.Debug("Bulk delete. All books deleted.");
    }
    
    /// <summary>
    /// Удалить книгу из индекса.
    /// </summary>
    /// <param name="book">Экземпляр книги.</param>
    /// <returns>True, если книга удалена успешно.</returns>
    public bool DeleteBookWithPages(Book book)
    {
      var bookDeletedResponse = DeleteBook(book);
      var pagesDeletedSuccessfully = DeletePages(book);
      return bookDeletedResponse && pagesDeletedSuccessfully;
    }

    /// <summary>
    /// Удалить страницы книги.
    /// </summary>
    /// <param name="book">Книга, страницы которой необходимо удалить.</param>
    /// <returns><c>True</c>, если удалены все страницы книги.</returns>
    private bool DeletePages(Book book)
    {
      var pagesDeletedResponse = Client.DeleteByQuery<Page>(q => q
        .Index(Indices.Parse(PagesIndexName))
        .Query(rq => rq
          .Term(f => f.BookId, book.Id)));
      var pagesDeletedSuccessfully = pagesDeletedResponse.Deleted != 0;
      _log.Debug($"Pages from {book.Name} deleted: {pagesDeletedSuccessfully}");
      return pagesDeletedSuccessfully;
    }

    /// <summary>
    /// Удалить книгу.
    /// </summary>
    /// <param name="book">Книга, которую необходимо удалить.</param>
    /// <returns><c>True</c>, книга удалена.</returns>
    private bool DeleteBook(Book book)
    {
      var bookDeletedResponse = Client.DeleteByQuery<Book>(q => q
        .Index(Indices.Parse(BooksIndexName))
        .Query(rq => rq
          .Term(f => f.Id, book.Id)));
      var bookDeletedSuccessfully = bookDeletedResponse.Deleted != 0; 
      _log.Debug($"Book {book.Name} deleted: {bookDeletedSuccessfully}");
      return bookDeletedSuccessfully;
    }

    #endregion
    
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
    }

    #endregion
  }
}