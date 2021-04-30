using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Library.Client.ViewModel;

namespace Library.Client.View
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = new SearchViewModel();
    }
  }
  /// <summary>
  /// Конвертер индексов.
  /// </summary>
  /// <remarks>https://stackoverflow.com/questions/16060874/listview-row-numbers-without-binding/16061596.</remarks>
  public class IndexConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var item = (ListViewItem)value;
      var index = -1;
      if (ItemsControl.ItemsControlFromItemContainer(item) is ListView listView)
        index = listView.ItemContainerGenerator.IndexFromContainer(item) + 1;
      
      return index.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
  
  /// <summary>
  /// Конвертер страниц.
  /// </summary>
  public class PagesConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is List<float> pages)
        return string.Join(", ", pages);

      return "<empty>";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}