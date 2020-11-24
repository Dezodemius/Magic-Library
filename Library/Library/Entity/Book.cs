using System;

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
    public Guid Id { get; set; }
    
    /// <summary>
    /// Название книги.
    /// </summary>
    public string Name { get; set; }

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
      return Id == other.Id && Name == other.Name;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return Id.GetHashCode() + Name.GetHashCode();
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
    public Book(Guid id, string name)
    {
      Id = id;
      Name = name;
    }

    #endregion
  }
}