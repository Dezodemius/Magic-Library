using System;
using Nest;

namespace Library.Entity
{
  /// <summary>
  /// Книга.
  /// </summary>
  public class Book : IEquatable<Book>
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

    #region IEquatable

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) 
        return false;
      if (ReferenceEquals(this, obj)) 
        return true;
      if (obj.GetType() != GetType()) 
        return false;
      return Equals((Book) obj);
    }

    public bool Equals(Book other)
    {
      if (ReferenceEquals(null, other)) 
        return false;
      if (ReferenceEquals(this, other)) 
        return true;
      return Id == other.Id && Name == other.Name && Text == other.Text;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return Id.GetHashCode() + Name.GetHashCode() + Text.GetHashCode();
      }
    }
    
    #endregion
    
    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    public Book()
    {
      
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Id книги.</param>
    /// <param name="name">Название книги.</param>
    /// <param name="text">Текст книги.</param>
    public Book(int id, string name, string text)
    {
      Id = id;
      Name = name;
      Text = text;
    }

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