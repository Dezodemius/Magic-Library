using System;
using System.Windows.Input;

namespace LibraryGui.Utils
{
  /// <summary>
  /// Команда.
  /// </summary>
  public class DelegateCommand : ICommand
  {
    #region Поля и свойства

    /// <summary>
    /// Действие команды.
    /// </summary>
    public Action<object> ExecuteAction { get; private set; }

    /// <summary>
    /// Обработчик возможности выполнения команды.
    /// </summary>
    public Func<object, bool> CanExecuteFunction { get; private set; }

    #endregion

    #region События

    /// <summary>
    /// Событие изменения признака воможности выполнения команды.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Выполнить команду.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    public void Execute(object parameter)
    {
      ExecuteAction?.Invoke(parameter);
    }

    /// <summary>
    /// Получить признак возможности выполнения команды.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns>Признак возможности выполнения команды.</returns>
    public bool CanExecute(object parameter)
    {
      return CanExecuteFunction == null || CanExecuteFunction(parameter);
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="executeAction">Действие команды.</param>
    /// <param name="canExecuteFunction">Обработчик возможности выполнения команды.</param>
    public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteFunction = null)
    {
      ExecuteAction = executeAction;
      CanExecuteFunction = canExecuteFunction;
    }

    #endregion
  }
}
