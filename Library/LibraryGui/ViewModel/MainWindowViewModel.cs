using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Library;
using Library.Entity;
using LibraryGui.Utils;
using Microsoft.Win32;

namespace LibraryGui.ViewModel
{
  public class MainWindowViewModel : ViewModel
  {
    public override string Name { get; } = "Главное окно";
    
    public static ObservableCollection<Book> FoundedBooks { get; set; } = new ObservableCollection<Book>();
    
    public static string SearchPhrase { get; set; }

    private static readonly Lazy<ElasticProvider> ElasticProvider = new Lazy<ElasticProvider>();
    
    private DelegateCommand _searchCommand;
    
    public DelegateCommand SearchCommand =>
      _searchCommand ?? (_searchCommand = new DelegateCommand(Search, SearchCanExecute));

    private bool SearchCanExecute(object arg)
    {
      return true;
    }

    private static void Search(object obj)
    {
      var documents = ElasticProvider.Value.Search(SearchPhrase).Documents;
      
      if (!documents.Any())
      {
        MessageBox.Show("Ничего не найдено");
        return;        
      }
      
      foreach (var result in documents)
        FoundedBooks.Add(result);
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
      var openFileDialog = new OpenFileDialog();
      if (openFileDialog.ShowDialog() == true)
        ElasticProvider.Value.Index(TextLayerExtractor.FromPdfToBook(openFileDialog.FileName));
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