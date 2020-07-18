using System.Windows;
using System.Windows.Threading;
using LibraryGui.Utils;
using LibraryGui.ViewModel;

namespace LibraryGui
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  { 
    /// <summary>
    /// Обработчик старта приложения.
    /// </summary>
    /// <param name="sender">Отправитель.</param>
    /// <param name="e">Параметр обработчика.</param>
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      ViewService.OpenViewModel(new SearchViewModel());
    }

    /// <summary>
    /// Обработчик возникших исключений.
    /// </summary>
    /// <param name="sender">Отправитель.</param>
    /// <param name="e">Параметр обработчика.</param>
    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      MessageBox.Show($"{e.Exception}\n{e.Exception.Message}", "Возникло исключение", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;
    }
  }
}