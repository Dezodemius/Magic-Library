using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Library.Client.Settings;
using Library.Client.ViewModel;
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
    private static ServiceController _elasticsearchService;

    /// <summary>
    /// Логгер.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    #endregion

    #region Методы
    
    /// <summary>
    /// Переустановить сервис с ES.
    /// </summary>
    public void ReinstallElasticService()
    {
      RemoveElasticService();
      InstallElasticService();
      StartElasticService();
    }
    
    /// <summary>
    /// Запустить процесс по удалению сервиса Elasticsearch.
    /// </summary>
    private static void RemoveElasticService()
    {
      var elasticServiceProcessInfo = new ProcessStartInfo
      {
          FileName = ApplicationSettings.ElasticsearchAppFileName,
          UseShellExecute = true,
          CreateNoWindow = true,
          Arguments = "remove"
      };

      Log.Info("Старт процесса по удалению сервиса Elasticsearch.");
      var elasticServiceProcess = Process.Start(elasticServiceProcessInfo);
      elasticServiceProcess?.WaitForExit();
      Log.Info("Сервис Elasticsearch удалён.");
    }

    /// <summary>
    /// Запустить процесс по установке сервиса Elasticsearch.
    /// </summary>
    private static void InstallElasticService()
    {
      var elasticServiceProcessInfo = new ProcessStartInfo
      {
          FileName = ApplicationSettings.ElasticsearchAppFileName,
          UseShellExecute = true,
          CreateNoWindow = true,
          Arguments = "install"
      };

      Log.Info("Старт процесса по установке сервиса Elasticsearch.");
      var elasticServiceProcess = Process.Start(elasticServiceProcessInfo);
      elasticServiceProcess?.WaitForExit();
      Thread.Sleep(5000);
      Log.Info("Сервис Elasticsearch установлен.");
    }

    /// <summary>
    /// Стартовать сервис Elasticsearch.
    /// </summary>
    private static void StartElasticService()
    {
      if (_elasticsearchService.Status == ServiceControllerStatus.Stopped)
      {
        Log.Info("Старт сервиса Elasticsearch.");
        _elasticsearchService.Start();
        _elasticsearchService.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
      }
      else
      {
        Log.Info("Сервис Elasticsearch уже запущен.");
      }
    }

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
      catch (InvalidOperationException ex) when (ex.InnerException is Win32Exception inner)
      {
        const int accessDeniedCode = 5;
        var message = inner.NativeErrorCode == accessDeniedCode ? 
            $"Отказано в доступе.{Environment.NewLine}Запустите приложение от имени Администратора." : 
            "Не удалось запустить сервис Elasticsearch. Пожалуйста, установите и запустите сервис вручную.";
        throw new LibraryInnerException(message);
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

    #endregion

    #region Конструктор

    /// <summary>
    /// Приватный конструктор.
    /// </summary>
    private ElasticsearchServiceManager()
    {
      _elasticsearchService = ServiceController.GetServices()
          .FirstOrDefault(s => s.DisplayName.Contains("elasticsearch-service-x64"));
      if (_elasticsearchService != null)
        _elasticsearchService.MachineName = Environment.MachineName;
    }

    #endregion
  }
}
