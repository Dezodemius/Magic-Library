using System;

namespace Library.Utils
{
  /// <summary>
  /// Исключение, возникающее при отсутствии нужного плагина в ES.
  /// </summary>
  public class RequiredPluginNotInstalled : Exception
  {
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    public RequiredPluginNotInstalled(string message) : base(message) 
    {
    }
  }
}