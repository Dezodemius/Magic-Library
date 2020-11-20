using System;

namespace Library.Utils
{
  /// <summary>
  /// Исключение, возникающее при отсутствии нужного плагина в ES.
  /// </summary>
  public class RequiredPluginNotInstalledException : Exception
  {
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    public RequiredPluginNotInstalledException(string message) : base(message) 
    {
    }
  }
}