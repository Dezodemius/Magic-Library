using System;
using Nest;

namespace Library.Entity
{
  /// <summary>
  /// Книга.
  /// </summary>
  public class Book
  {
    /// <summary>
    /// Название книги.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Текст книги.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    protected Book()
    {
      
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="name">Название книги.</param>
    public Book(string name) : this(name, string.Empty)
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="name">Название книги.</param>
    /// <param name="text">Текст книги.</param>
    public Book(string name, string text)
    {
      Name = name;
      Text = text;
    }
  }

  /// <summary>
  /// Объект книги для ES.
  /// </summary>
  [ElasticsearchType(RelationName = "book")]
  public abstract class ElasticBook : Book
  {
    /// <summary>
    /// Название книги.
    /// </summary>
    [Text(Name = "name")]
    public new string Name { get; set; }
    
    /// <summary>
    /// Текст книги.
    /// </summary>
    [Text(Name = "text")]
    public new string Text { get; set; }
  }
}