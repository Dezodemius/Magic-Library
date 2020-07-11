using System;
using Library;
using LibraryGui.Utils;

namespace LibraryGui.ViewModel
{
  public class MainWindowsViewModel : ViewModel
  {
    public override string Name { get; } = "Главное окно";
    
    private DelegateCommand _searchCommand;
    
    public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(Search, SearchCanExecute));

    private bool SearchCanExecute(object arg)
    {
      return true;
    }

    private void Search(object obj)
    {
      var esProvider = new ElasticProvider();
      var searchPhrase = Console.ReadLine();
      var documents = esProvider.Search(searchPhrase).Documents;
      foreach (var result in documents)
        Console.WriteLine(result.Name);
    }
  }
}