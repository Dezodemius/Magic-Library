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
    /// Вложение книги, которое содержит в себе текстовый слой в формате base64.
    /// </summary>
    public Attachment Attachment { get; set; }

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
      return Id == other.Id && Name == other.Name && Attachment == other.Attachment;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return Id.GetHashCode() + Name.GetHashCode() + Attachment.GetHashCode();
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
    /// <param name="textInBase64">Текст книги (должен быть в base64!!!).</param>
    public Book(int id, string name, string textInBase64)
    {
      Id = id;
      Name = name;
      Attachment = new Attachment
      {
        Content = textInBase64
      };
    }

    #endregion
  }
}