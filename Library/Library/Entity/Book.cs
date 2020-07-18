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
    public int Id { get; }
    
    /// <summary>
    /// Название книги.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Текст книги.
    /// </summary>
    public string Text { get; }

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
        var hashCode = Id;
        hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
        return hashCode;
      }
    }
    
    #endregion
    
    #region Конструкторы

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