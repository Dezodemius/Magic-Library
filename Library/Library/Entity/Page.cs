using System;
using Nest;

namespace Library.Entity
{
  /// <summary>
  /// Страница книги.
  /// </summary>
  public class Page
  {
    /// <summary>
    /// Номер страницы.
    /// </summary>
    public int Number { get; }

    /// <summary>
    /// ИД книги, к которой пренадлежит страница.
    /// </summary>
    public Guid BookId { get; }

    /// <summary>
    /// Вложение страницы.
    /// </summary>
    public Attachment Attachment { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="number">Номер страницы.</param>
    /// <param name="bookId">ИД книги, к которой принадлежит страница.</param>
    /// <param name="contentInBase64">Контент страницы в base64.</param>
    public Page(int number, Guid bookId, string contentInBase64)
    {
      Number = number;
      BookId = bookId;
      Attachment = new Attachment
      {
        Content = contentInBase64
      };
    }
  }
}