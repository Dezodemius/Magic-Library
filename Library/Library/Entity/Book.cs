using System;

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
    public Guid Id { get; set; }

    /// <summary>
    /// Название книги.
    /// </summary>
    public string Name { get; set; }

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