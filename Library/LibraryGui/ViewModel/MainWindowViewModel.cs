using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Library;
using LibraryGui.Utils;

namespace LibraryGui.ViewModel
{
  public class MainWindowViewModel : ViewModel
  {
    public override string Name { get; } = "Главное окно";
    
    public static ObservableCollection<string> FoundedBooks { get; set; } = new ObservableCollection<string>();
    
    public static string SearchPhrase { get; set; }

    private DelegateCommand _searchCommand;
    
    public DelegateCommand SearchCommand =>
      _searchCommand ?? (_searchCommand = new DelegateCommand(Search, SearchCanExecute));

    private bool SearchCanExecute(object arg)
    {
      return true;
    }

    private static void Search(object obj)
    {
      var esProvider = new ElasticProvider();
      var documents = esProvider.Search(SearchPhrase).Documents;
      
      if (!documents.Any())
      {
        MessageBox.Show("Ничего не найдено");
        return;        
      }
      
      foreach (var result in documents)
        FoundedBooks.Add(result.Name);
    }
    
    private DelegateCommand _addBookCommand;
    
    public DelegateCommand AddBookCommand =>
      _addBookCommand ?? (_addBookCommand = new DelegateCommand(AddBook, AddBookCanExecute));

    private static bool AddBookCanExecute(object arg)
    {
      return true;
    }

    private static void AddBook(object obj)
    {
      
    }
    
    private DelegateCommand _getAllBooksCommand;
    
    public DelegateCommand GetAllBooksCommand => 
      _getAllBooksCommand ?? (_getAllBooksCommand = new DelegateCommand(GetAllBooks, GetAllBooksCanExecute));

    private static bool GetAllBooksCanExecute(object arg)
    {
      return true;
    }

    private static void GetAllBooks(object obj)
    {
      
    }
  }
}