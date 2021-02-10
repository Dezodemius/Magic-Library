using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using Library.Client.Settings;
using NLog;

namespace Library.Client.Utils
{
  /// <summary>
  /// Менеджер сервиса Elasticsearch.
  /// </summary>
  public class ElasticsearchServiceManager
  {
    #region Singltone

    private static ElasticsearchServiceManager _instance;

    /// <summary>
    /// Экземпляр класса.
    /// </summary>
    public static ElasticsearchServiceManager Instance => _instance ??= new ElasticsearchServiceManager();

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Сервис Elasticsearch.
    /// </summary>
    private readonly ServiceController _elasticsearchService;

    /// <summary>
    /// Логгер.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    #endregion

    #region Методы

    /// <summary>
    /// Убедиться в работе сервиса Elasticsearch.
    /// </summary>
    /// <exception cref="InvalidOperationException">Возникает в случае неудачи запуска сервиса.
    /// Возможно, нет прав администратора.
    /// </exception>
    internal void EnsureElasticsearchServiceRun()
    {
      try
      {
        if (_elasticsearchService == null)
          InstallElasticService();
        StartElasticService();
      }
      catch (InvalidOperationException innerException)
      {
        throw new InvalidOperationException(
          "Не удалось запустить сервис Elasticsearch. Пожалуйста, установите и запустите сервис вручную.", 
          innerException);
      }
    }

    /// <summary>
    /// Остановить работу сервиса Elasticsearch.
    /// </summary>
    internal void StopElasticService()
    {
      if (_elasticsearchService.Status == ServiceControllerStatus.Running)
      {
        Log.Info("Остановка сервиса Elasticsearch.");
        _elasticsearchService.Stop();
        _elasticsearchService.WaitForStatus(ServiceControllerStatus.Stopped);
      }
      else
      {
        Log.Info("Сервис Elasticsearch уже остановлен.");
      }
    }

    /// <summary>
    /// Стартовать сервис Elasticsearch.
    /// </summary>
    private void StartElasticService()
    {
      if (_elasticsearchService.Status == ServiceControllerStatus.Stopped)
      {
        Log.Info("Старт сервиса Elasticsearch.");
        _elasticsearchService.Start();
        _elasticsearchService.WaitForStatus(ServiceControllerStatus.Running);
      }
      else
      {
        Log.Info("Сервис Elasticsearch уже запущен.");
      }
    }

    /// <summary>
    /// Запустить процесс по установке сервиса Elasticsearch.
    /// </summary>
    private static void InstallElasticService()
    {
      var elasticServiceProcessInfo = new ProcessStartInfo
      {
        FileName = ApplicationSettings.ElasticsearchAppFileName,
        UseShellExecute = false,
        CreateNoWindow = false,
        Arguments = "install"
      };

      Log.Info("Старт процесса по установке сервиса Elasticsearch.");
      var elasticServiceProcess = Process.Start(elasticServiceProcessInfo);
      elasticServiceProcess?.WaitForExit();
      Log.Info("Сервис Elasticsearch установлен.");
    }

    #endregion

    #region Конструктор

    /// <summary>
    /// Приватный конструктор.
    /// </summary>
    private ElasticsearchServiceManager()
    {
      _elasticsearchService = ServiceController.GetServices()
        .FirstOrDefault(s => s.DisplayName.Contains("elasticsearch-service-x64"));
    }

    #endregion
  }
}
