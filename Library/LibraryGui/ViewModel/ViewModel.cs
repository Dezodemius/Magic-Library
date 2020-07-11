using System.ComponentModel;

namespace LibraryGui.ViewModel
{
  /// <summary>
  /// Базовый класс модели представления.
  /// </summary>
  public abstract class ViewModel : INotifyPropertyChanged
  {
    #region Поля и свойства

    /// <summary>
    /// Имя модели представления.
    /// </summary>
    public abstract string Name { get; }

    #endregion

    #region INotifyPropertyChanged

    /// <summary>
    /// Событие на изменение свойства.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Сгенерировать событие на изменение свойства.
    /// </summary>
    /// <param name="propertyName">Имя изменённого свойства.</param>
    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}