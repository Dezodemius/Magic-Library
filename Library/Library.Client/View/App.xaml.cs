using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Library.Client.Settings;
using Library.Client.Utils;
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

    /// <summary>
    /// Пррозрачное окно.
    /// </summary>
    private static Window _transparentWindow;

    #endregion

    #region Обработчики событий

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      ShowTransparentWindow();
      var splash = new SplashScreen("splashscreen.png");
      splash.Show(false);
      InitApplication();

      ElasticProvider.Instance.Initialize();
      ElasticSynchronizer.SynchronizeWithDisk();

      _transparentWindow.Close();
      _transparentWindow = null;
      splash.Close(TimeSpan.Zero);
    }

    private void App_OnExit(object sender, ExitEventArgs e)
    {
      if (ApplicationSettings.NeedCloseElasticOnExit)
        ElasticsearchServiceManager.Instance.StopElasticService();
      CloseApplication(0);
    }

    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      string message;
      if (e.Exception is AggregateException)
        message = $"{e.Exception.InnerException?.Message}";
      else
        message = e.Exception.Message;
      Log.Fatal($"{message}\n{e.Exception.StackTrace}");
      var messageBoxResult = MessageBox.Show(message, "Возникло исключение", MessageBoxButton.OK, MessageBoxImage.Error);

      e.Handled = false;

      if (messageBoxResult == MessageBoxResult.OK)
        CloseApplication(-1);
    }

    #endregion

    #region Методы

    /// <summary>
    /// Показать прозрачное окно для нормального вывода возникшего при старте приложения исключения.
    /// </summary>
    /// <remarks>
    /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/116bcd83-93bf-42f3-9bfe-da9e7de37546/messagebox-closes-immediately-in-dispatcherunhandledexception-handler?forum=wpf/>
    /// </remarks>
    private static void ShowTransparentWindow()
    {
      _transparentWindow = new Window
      {
          AllowsTransparency = true,
          Background = System.Windows.Media.Brushes.Transparent,
          WindowStyle = WindowStyle.None,
          Top = 0,
          Left = 0,
          Width = 1,
          Height = 1,
          ShowInTaskbar = false
      };

      _transparentWindow.Show();
    }

    /// <summary>
    /// Выполнить инициализацию приложения перед стартом.
    /// </summary>
    private static void InitApplication()
    {
      if (!string.IsNullOrEmpty(ApplicationSettings.LogFilename))
        ConfigureLogs();

      if (ApplicationSettings.NeedReinstallService)
      {
        ElasticsearchServiceManager.Instance.ReinstallElasticService();
        WaitRunPendingElasticsearch();
      }

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

    /// <summary>
    /// Завершить работу приложения.
    /// </summary>
    /// <param name="exitCode">Код завершения.</param>
    private static void CloseApplication(int exitCode)
    {
      if (Current != null)
        Current.Shutdown(exitCode);
      Environment.Exit(exitCode);
    }

    #endregion
  }
}
