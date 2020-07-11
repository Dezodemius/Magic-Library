using System.Windows;
using LibraryGui.Utils;
using LibraryGui.ViewModel;

namespace LibraryGui
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  { 
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      ViewService.OpenViewModel(new MainWindowViewModel());
    }
  }
}