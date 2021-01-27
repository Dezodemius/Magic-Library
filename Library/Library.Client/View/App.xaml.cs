using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
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
    private readonly Logger _log = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Обработчик старта приложения.
    /// </summary>
    /// <param name="sender">Отправитель.</param>
    /// <param name="e">Параметр обработчика.</param>
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      ViewService.OpenViewModel(new MainWindowViewModel());
      ElasticSynchronizer.SynchronizeWithDisk();
    }

    /// <summary>
    /// Обработчик возникших исключений.
    /// </summary>
    /// <param name="sender">Отправитель.</param>
    /// <param name="e">Параметр обработчика.</param>
    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      var message = $"{e.Exception.Message}";
      _log.Error(message);
      MessageBox.Show(message, "Возникло исключение", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;
    }
  }
}