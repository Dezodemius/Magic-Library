using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Library.Client.Settings;
using Library.Client.Utils;
using Library.Client.ViewModel;
using Library.Utils;
using NLog;

namespace Library.Client.View
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    /// <summary>
    /// Логгер.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      if (ApplicationSettings.NeedRunElasticOnStartup)
      {
        ElasticsearchServiceManager.Instance.EnsureElasticsearchServiceRun();
        WaitRunPendingElasticsearch();
      }

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
      Log.Error(message);
      MessageBox.Show(message, "Возникло исключение", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;

      Environment.Exit(-1);
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
  }
}
