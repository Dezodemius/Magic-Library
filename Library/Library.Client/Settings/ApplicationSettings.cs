using System;
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
    internal static bool NeedRunElasticOnStartup
    {
      get
      {
        var defaultValue = bool.TrueString;
        var settingValue = GetSetting("START_ELASTIC_ON_STARTUP");
        return bool.Parse(settingValue ?? defaultValue);
      }
    }

    /// <summary>
    /// Признак необходимости закрыть ES после выхода из приложения.
    /// </summary>
    internal static bool NeedCloseElasticOnExit
    {
      get
      {
        var defaultValue = bool.TrueString;
        var settingValue = GetSetting("CLOSE_ELASTIC_ON_EXIT");
        return bool.Parse(settingValue ?? defaultValue);
      }
    }

    /// <summary>
    /// Путь, где находится сервис ES.
    /// </summary>
    internal static FileInfo ElasticsearchAppFileInfo
    {
      get
      {
        var settingValue = GetSetting("ELASTICSEARCH_SERVICE_PATH");
        if (settingValue == null)
          throw new ArgumentException("Не задан путь к сервису Elasticsearch. " +
                                      "Для указания пути используйте настойку ELASTICSEARCH_SERVICE_PATH");
        return new FileInfo(settingValue);
      }
    }

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
