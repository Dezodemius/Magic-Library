using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Library.Client.Settings;
using Library.Client.Utils;
using Library.Client.ViewModel;
using Library.Utils;
using NLog;
using NLog.Layouts;
using NLog.Targets;

namespace Library.Client.View
{
  /// <summary>
  /// Логика взаимодействия для App.xaml.
  /// </summary>
  public partial class App
  {
    #region Поля

    /// <summary>
    /// Логгер.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    #endregion

    #region Обработчики событий

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      InitApplication();

      ElasticProvider.Instance.Initialize();
      ElasticSynchronizer.SynchronizeWithDisk();

      ViewService.OpenViewModel(new MainWindowViewModel());
    }

    private void App_OnExit(object sender, ExitEventArgs e)
    {
      if (ApplicationSettings.NeedCloseElasticOnExit)
        ElasticsearchServiceManager.Instance.StopElasticService();
    }

    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      var message = $"{e.Exception.Message}";
      Log.Fatal($"{message}\n{e.Exception.StackTrace}");
      MessageBox.Show(message, "Возникло исключение", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;

      Environment.Exit(-1);
    }

    #endregion

    #region Методы

    /// <summary>
    /// Выполнить инициализацию приложения перед стартом.
    /// </summary>
    private static void InitApplication()
    {
      if (!string.IsNullOrEmpty(ApplicationSettings.LogFilename))
        ConfigureLogs();
      if (ApplicationSettings.NeedRunElasticOnStartup)
      {
        ElasticsearchServiceManager.Instance.EnsureElasticsearchServiceRun();
        WaitRunPendingElasticsearch();
      }
    }

    /// <summary>
    /// Конфигурировать конфиг NLog.
    /// </summary>
    private static void ConfigureLogs()
    {
      var config = LogManager.Configuration;
      ((FileTarget)LogManager.Configuration.FindTargetByName("file")).FileName = (Layout)ApplicationSettings.LogFilename;
      LogManager.Configuration = config;
    }

    /// <summary>
    /// Подождать, пока сервис ES будет готов для работы.
    /// </summary>
    private static void WaitRunPendingElasticsearch()
    {
      var stopWatch = new Stopwatch();
      while (true)
      {
        try
        {
          if (ElasticProvider.Instance.CheckElasticsearchConnection())
          {
            Log.Debug($"Elasticsearch поднялся за {stopWatch.Elapsed.Seconds} с");
            return;
          }
        }
        catch
        {
          Thread.Sleep(1000);
        }
      }
    }

    #endregion
  }
}
