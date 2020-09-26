using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Library.Client.Utils
{
  /// <summary>
  /// Сервис управления окнами.
  /// </summary>
  public static class ViewService
  {
    #region Поля и свойства

    /// <summary>
    /// Коллекция открытых окон.
    /// </summary>
    private static readonly Collection<Window> OpenedWindows = new Collection<Window>();

    #endregion

    #region Методы

    /// <summary>
    /// Открыть представление.
    /// </summary>
    /// <param name="viewModel">Модель представления.</param>
    /// <param name="width">Ширина окна.</param>
    /// <param name="height">Высота окна.</param>
    public static void OpenViewModel(ViewModel.ViewModel viewModel, double width, double height)
    {
      Window openedWindow = OpenedWindows.SingleOrDefault(window => window.Content.Equals(viewModel));
      if (openedWindow == null)
      {
        openedWindow = new Window {Width = width, Height = height, Title = viewModel.Name, Content = viewModel};
        openedWindow.Closed += OpenedWindowClosed;
        OpenedWindows.Add(openedWindow);
        openedWindow.Show();
      }
      else
        openedWindow.Activate();
    }

    /// <summary>
    /// Открыть представление.
    /// </summary>
    /// <param name="viewModel">Модель представления.</param>
    public static void OpenViewModel(ViewModel.ViewModel viewModel)
    {
      OpenViewModel(viewModel, 640, 480);
    }

    /// <summary>
    /// Обработчик события на закрытие окна.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Параметры события.</param>
    private static void OpenedWindowClosed(object sender, EventArgs e)
    {
      Window window = sender as Window;
      if (window != null && OpenedWindows.Contains(window))
      {
        OpenedWindows.Remove(window);
        window.Closed -= OpenedWindowClosed;
      }
    }

    #endregion
  }
}