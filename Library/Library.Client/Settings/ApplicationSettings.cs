using System.Configuration;
using System.IO;

namespace Library.Client.Settings
{
  /// <summary>
  /// Настройки приложения.
  /// </summary>
  public static class ApplicationSettings
  {
    /// <summary>
    /// Признак необходимости запустить ES при старте.
    /// </summary>
    internal static bool NeedRunElasticOnStartup => bool.Parse(GetSetting("START_ELASTIC_ON_STARTUP"));

    /// <summary>
    /// Признак необходимости закрыть ES после выхода из приложения.
    /// </summary>
    internal static bool NeedCloseElasticOnExit => bool.Parse(GetSetting("CLOSE_ELASTIC_ON_EXIT"));

    /// <summary>
    /// Путь, где находится сервис ES.
    /// </summary>
    internal static FileInfo ElasticsearchAppFileInfo => new FileInfo(GetSetting("ELASTICSEARCH_SERVICE_PATH"));

    /// <summary>
    /// Получить настройку из App.config.
    /// </summary>
    /// <param name="settingName">Имя настройки.</param>
    /// <returns>Значение настройки. Имеет тип <c>string</c></returns>
    private static string GetSetting(string settingName)
    {
      return ConfigurationManager.AppSettings.Get(settingName);
    }
  }
}
