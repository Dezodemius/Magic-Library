using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Library.Client.View
{
  /// <summary>
  /// Логика взаимодействия для SearchControl.xaml.
  /// </summary>
  public partial class SearchControl : UserControl
  {
    public SearchControl()
    {
      InitializeComponent();
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
}
