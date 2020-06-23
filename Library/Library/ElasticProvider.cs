﻿using System;
using System.Collections.Generic;
using System.IO;
using Library.Entity;
using Nest;

namespace Library
{
  /// <summary>
  /// Провайдер Elasticsearch NEST.
  /// </summary>
  public class ElasticProvider
  {
    #region Поля и свойства

    /// <summary>
    /// Экземпляр клиента Elasticsearch.
    /// </summary>
    private readonly ElasticClient _client;

    #endregion
    
    #region Методы

    /// <summary>
    /// Выполнить поиск.
    /// </summary>
    /// <param name="searchPhrase">Поисковая фраза.</param>
    public ISearchResponse<Book> Search(string searchPhrase)
    {
      var response = _client.Search<Book>(s => s
        .Query(q => q
          .MultiMatch(c => c
            .Query(searchPhrase)
            .Fields(f => f.Field(b => b.Text)))));
      return response;
    }
    
    /// <summary>
    /// Выполнить поиск.
    /// </summary>
    public IEnumerable<Book> SearchAll()
    {
      return _client.Search<Book>(s => s.Query(q => q.MatchAll())).Documents;
    }

    /// <summary>
    /// Получить шаблон настрок ES.
    /// </summary>
    /// <returns>Шаблон индекса.</returns>
    public string GetTemplate()
    {
      return _client.Indices.GetTemplate().ToString();
    }

    /// <summary>
    /// Индексировать документ.
    /// </summary>
    /// <param name="book">Экземпляр книги.</param>
    public void Index(Book book)
    {
      var indexResponse = _client.Index(book, i => i.Index("books"));
      if (!indexResponse.IsValid)
        Console.WriteLine($"{book.Name} - не удалось индексировать.");
    }

    #endregion

    #region Конструкторы
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public ElasticProvider() : this(null) { }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="elasticUrl">Адрес ES.</param>
    public ElasticProvider(Uri elasticUrl)
    {
      var setting = new ConnectionSettings(elasticUrl)
        .DefaultIndex(LibraryConstants.DefaultIndexName);
      
      _client = new ElasticClient(setting);
      
      if (!_client.Indices.Exists(LibraryConstants.DefaultIndexName).Exists)
      {
        _client.Indices.Create(LibraryConstants.DefaultIndexName,
          i => i.Map<ElasticBook>(m => m.AutoMap()));
      }
    }
    
    #endregion
  }
}