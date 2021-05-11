using System;
using System.Configuration;

namespace Library.Client.Settings
{
  /// <summary>
  /// Настройки приложения.
  /// </summary>
  public static class ApplicationSettings
  {
    #region Свойства

    /// <summary>
    /// Признак необходимости запустить ES при старте.
    /// </summary>
    internal static bool NeedRunElasticOnStartup => GetSetting<bool>("START_ELASTIC_ON_STARTUP", true);

    /// <summary>
    /// Признак необходимости закрыть ES после выхода из приложения.
    /// </summary>
    internal static bool NeedCloseElasticOnExit => GetSetting<bool>("CLOSE_ELASTIC_ON_EXIT", true);

    /// <summary>
    /// Путь, где находится сервис ES.
    /// </summary>
    internal static string ElasticsearchAppFileName => GetSetting<string>("ELASTICSEARCH_SERVICE_PATH", "../../elasticsearch-7.8.0/bin/elasticsearch-service.bat");

    /// <summary>
    /// Путь к логам приложения.
    /// </summary>
    internal static string LogFilename => GetSetting<string>("LOG_FILENAME", string.Empty);
    
    /// <summary>
    /// Признак необходимости удалить сервис ES при старте.
    /// </summary>
    internal static bool NeedReinstallService => GetSetting<bool>("NEED_REINSTALL_SERVICE_ON_STARTUP", true);
    #endregion

    #region Методы

    /// <summary>
    /// Получить настройку из App.config.
    /// </summary>
    /// <param name="settingName">Имя настройки.</param>
    /// <param name="defaultValue">Значение по умолчанию.</param>
    /// <typeparam name="T">Тип настройки.</typeparam>
    /// <returns>Значение настройки.</returns>
    private static T GetSetting<T>(string settingName, T defaultValue = default(T))
    {
      var setting = ConfigurationManager.AppSettings.Get(settingName);
      if (setting == null)
        return defaultValue;
      var convertedSetting = (T) Convert.ChangeType(setting, typeof(T));
      return convertedSetting;
    }

    #endregion
  }
}
