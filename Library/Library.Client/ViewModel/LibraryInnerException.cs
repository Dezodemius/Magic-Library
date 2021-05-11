using System;

namespace Library.Client.ViewModel
{
  /// <summary>
  /// Внутреннее исключение библиотеки.
  /// </summary>
  public class LibraryInnerException : Exception
  {
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="message">Текст исключения.</param>
    public LibraryInnerException(string message)
        : base(message)
    {
    }
  }
}