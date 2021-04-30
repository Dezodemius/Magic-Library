using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Library.Client.Model;
using Library.Client.Utils;
using Microsoft.Win32;

namespace Library.Client.ViewModel
{
  /// <summary>
  /// ViewModel для показа найденных Highlight-ов.
  /// </summary>
  public class HighlightsViewModel : ViewModel
  {
    /// <summary>
    /// Имя окна.
    /// </summary>
    public override string Name { get; } = "Highlights";

    /// <summary>
    /// Выбранная книга.
    /// </summary>
    public BookWithPages SelectedBook { get; set; }
    
    /// <summary>
    /// Список хайлайтов.
    /// </summary>
    public List<HighlightWithPages> Highlights { get; set; }

    private DelegateCommand _openPageCommand;
    
    /// <summary>
    /// Команда для открытия страницы.
    /// </summary>
    public DelegateCommand OpenPageCommand =>
        _openPageCommand ??= new DelegateCommand(OpenPage, OpenPageCanExecute);

    /// <summary>
    /// Признак того, что можно открыть страницу.
    /// </summary>
    /// <param name="arg">Какой-то аргумент.</param>
    /// <returns>Сейчас всегда возращает <c>True</c>.</returns>
    private bool OpenPageCanExecute(object arg)
    {
      return true;
    }

    /// <summary>
    /// Открыть страницу в браузере.
    /// </summary>
    /// <param name="obj">Книга.</param>
    private void OpenPage(object obj)
    {
      if (obj is HighlightWithPages highlightWithPages)
      {
        var bookPath = Path.Combine(BookManager.Instance.BookShelfPath.FullName, SelectedBook.Name + ".pdf");
        var browserPath = GetSystemDefaultBrowser();
        var argument = @$"""file:///{bookPath}#page={highlightWithPages.Page.ToString()}""";
        var ieProcessInfo = new ProcessStartInfo(browserPath, argument);
        if (File.Exists(bookPath))
          Process.Start(ieProcessInfo); 
      }
    }
    
    /// <summary>
    /// Метод для получения пути к браузеру по умолчанию.
    /// </summary>
    /// <remarks><see href="https://www.dreamincode.net/forums/blog/143/entry-2054-get-default-browser-more-in-c?__cf_chl_captcha_tk__=3dc39ae30437c8455a9fd1602b28d87d6027d775-1619811676-0-AS80dDoSpAyN3-ESBn2Slvc8iTlXrSD3aFP7RSfjcGGdqlvON6wHGP4PpSGPG-HaJRxdm4FOV8GxqWDH3IjwPr0a08blha_HqSp-bZjZsAFn52B8qqcGh3oRAqxBjNG36iXz2puSGbEntIGvLmOwVn5jZi1lt7MqLhlI8HBzmCgti-IDBXXNQVXsZuEkHmB4fhG_HL6mZU37bWFaVCsPfpQyCiQp9ygrTylWIIF7LpluKhE-Fk9a8sEGZeMqAB573pXlYXXmWm5g34m1JUQVoFP_L_DncHNFAQ2Q-F6G8E24phSyc_8XkHq0C0P3wudvBrQ6aTKNmc66HsGGXrgyJ1QueN8_Tym_ZtzlGa2i_-N0vkLMJF927BWfCOFi4z7t7YcS9CwIDTPb_FxAUMWBI5iC5-jwUqXxbRb5H1pOfrIoXgOO19XNIiNdYHLxd1BzEKk1OyRBtgYW5KwJN4s309CfTwEEr51UKQQX1epCgsW133SLikcfC9t2f8qpsLuJ6jUUsm2tiyapG43x8Kkm1glkUYT7JgwQ6yH9sS3FJDcr9RqZtYa8jvMfewJ1evstmIgPNUen6Tv0EQ_Ec6g9mMOSYk-uZ6iGq6eHLp1lfrmaxrdgTvLaW2-Sc-XyLUjjWiWGopoAmhd07px8ncbTjHOjikJqMvlCo1fvwNurrVE8pkhBvIDUxlFOhVPOcMNFlQ#/"/></remarks>
    /// <returns>Путь к испоняемому файлу браузера.</returns>
    private string GetSystemDefaultBrowser()
    {
      string name;
      RegistryKey regKey = null;

      try
      {
        regKey = Registry.ClassesRoot.OpenSubKey("HTTP\\shell\\open\\command", false);

        name = regKey?.GetValue(null).ToString().ToLower().Replace("" + (char)34, "");

        if (!name.EndsWith("exe"))
          name = name.Substring(0, name.LastIndexOf(".exe", StringComparison.Ordinal) + 4);

      }
      catch (Exception ex)
      {
        name = $"ERROR: An exception of type: {ex.GetType()} occurred in method: {ex.TargetSite} in the following module: {this.GetType()}";
      }
      finally
      {
        regKey?.Close();
      }
      
      return name;
    }
  }
}