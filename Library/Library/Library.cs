using System.IO;

namespace Library
{
  public sealed class Library
  {
    #region Поля и свойства

    private static Library _instance;

    public static Library Instance
    {
      get => _instance ?? new Library();

      private set => _instance = value;
    }

    #endregion

    #region Методы

    public static void AddBook(string bookPath)
    {
      var bookFilePath = new FileInfo(bookPath);
      bookFilePath.CopyTo(LibraryConstants.DefaultBookshelfDirectoryPath.ToString());
    }
    
    private static void CreateBookshelfIfNeeded()
    {
      var bookshelfPath = LibraryConstants.DefaultBookshelfDirectoryPath;
      if (!bookshelfPath.Exists)
        bookshelfPath.Create();
    }

    #endregion
    
    #region Конструкторы

    private Library()
    {
      CreateBookshelfIfNeeded();
    }

    #endregion
  }
}