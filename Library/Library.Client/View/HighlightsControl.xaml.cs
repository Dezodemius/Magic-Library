using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Library.Client.View
{
  /// <summary>
  /// Контрол для отображения Highlight-ов.
  /// </summary>
  public partial class HighlightsControl : UserControl
  {
    /// <summary>
    /// Конструктор.
    /// </summary>
    public HighlightsControl()
    {
      InitializeComponent();
    }
  }
  
  /// <summary>
  /// Конвертер Highlight-ов для отображения в списке.
  /// </summary>
  public class HighlightConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is IReadOnlyDictionary<string, IReadOnlyCollection<string>>  highlightWithPages)
        return highlightWithPages["attachment.content"].ToList().First();
      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}