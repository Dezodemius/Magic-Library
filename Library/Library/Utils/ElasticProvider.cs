using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
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
          .Attachment<Book>(a => a
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
    /// Индексировать документ.
    /// </summary>
    /// <param name="book">Экземпляр книги.</param>
    public void Index(Book book)
    {
      var indexResponse = Client
        .Index(book, i => i
          .Index(DefaultIndex)
          .Pipeline(PipelineAttachmentName));
      
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
          .IndexMany(books)
          .Pipeline(PipelineAttachmentName));

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
    
    #endregion

    #region Поиск

    /// <summary>
    /// Выполнить поиск.
    /// </summary>
    /// <param name="searchPhrase">Поисковая фраза.</param>
    /// <returns>Результат поиска.</returns>
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
              .Analyzer("russian_morphology")
              .Fields(
                f => f.Field(b => b.Attachment.Content))))
          .Highlight(h => h
            .Fields(f => f.Field(b => b.Attachment.Content))));
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
    /// <returns>Список всех книг в индексе.</returns>
    public IEnumerable<Book> GetAll()
    {
      var response = Client.Search<Book>(s => s.Query(q => q.MatchAll()));
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

    #endregion

    #region Конструкторы
    
    /// <summary>
    /// Приватный конструктор.
    /// </summary>
    private ElasticProvider()
    {
      var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
        .DefaultIndex(DefaultIndex)
        .ThrowExceptions();
      
      Client = new ElasticClient(settings);
      
      if (!Client.Indices.Exists(DefaultIndex).Exists)
      {
        Client.Indices.Create(DefaultIndex, 
          i => i
            .Map<Book>(m => m
              .AutoMap()
              .Properties(p => p
                .Text(t => t
                  .Name(n => n.Attachment.Content)
                  .SearchAnalyzer("russian_morphology"))))
            .Settings(s => s
              .Analysis(a => a
                .TokenFilters(f => f
                  .Stemmer("russian_stemmer", s => s.Language("russian"))
                  .Stop("morphology_stopwords", w => w.StopWords("а,без,более,бы,был,была,были,было,быть,в,вам,вас,весь,во,вот,все,всего,всех,вы,где,да,даже,для,до,его,ее,если,есть,еще,же,за,здесь,и,из,или,им,их,к,как,ко,когда,кто,ли,либо,мне,может,мы,на,надо,наш,не,него,нее,нет,ни,них,но,ну,о,об,однако,он,она,они,оно,от,очень,по,под,при,с,со,так,также,такой,там,те,тем,то,того,тоже,той,только,том,ты,у,уже,хотя,чего,чей,чем,что,чтобы,чье,чья,эта,эти,это,я,a,an,and,are,as,at,be,but,by,for,if,in,into,is,it,no,not,of,on,or,such,that,the,their,then,there,these,they,this,to,was,will,with")))
                .Analyzers(aa => aa
                  .Custom("russian_morphology", c => c
                    .Tokenizer("standard")
                    .Filters("lowercase", "russian_morphology", "english_morphology", "morphology_stopwords", "russian_stemmer"))))));
       
        _log.Info($"Index {DefaultIndex} created.");
      }
      else
      {
        _log.Info($"Index '{DefaultIndex}' already exists.");
      }
      
      CheckRequiredPlugins();
      PutPipeline();
    }

    #endregion
  }
}