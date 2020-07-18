using Nest;

namespace Library.Entity
{
  /// <summary>
  /// Книга.
  /// </summary>
  public class Book
  {
    #region Свойства

    /// <summary>
    /// Идентификатор книги.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Название книги.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Текст книги.
    /// </summary>
    public string Text { get; set; }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    public Book() { }

    #endregion
  }

  /// <summary>
  /// Объект книги для ES.
  /// </summary>
  [ElasticsearchType(RelationName = "book")]
  public abstract class ElasticBook : Book
  {
    [Text(Name = "id")]
    public new int Id { get; set; }
    
    [Text(Name = "name")]
    public new string Name { get; set; }
    
    [Text(Name = "text")]
    public new string Text { get; set; }
  }
}