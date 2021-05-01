using System;
using System.Collections.Generic;
using System.Linq;
using Library.Entity;
using NLog;

namespace Library.Utils
{
  /// <summary>
  /// Синхронайзер между индексом ES и диском.
  /// </summary>
  public static class ElasticSynchronizer
  {
    /// <summary>
    /// Логгер класса.
    /// </summary>
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Выполнить синхронизацию ES с диском.
    /// </summary>
    public static void SynchronizeWithDisk()
    {
      var booksOnDisk = BookManager.Instance.GetAllBooks();
      var booksInIndex = ElasticProvider.Instance.GetAllBooks();

      var booksOnlyOnDisk =  booksOnDisk.Except(booksInIndex.ToList(), new BookEqualityComparer());
      if (booksOnlyOnDisk.Any())
      {
        Log.Debug($"The number of books contained on disk but not indexed: {booksOnlyOnDisk.Count(b => b.Id != Guid.Empty)}");
        
        foreach (var book in booksOnlyOnDisk)
        {
          ElasticProvider.Instance.Index(book);
          var progress = 0.0;
          var bookPages = TextLayerExtractor.GetTextLayerWithPages(
            $"{BookManager.Instance.BookShelfPath}/{book.Name}.pdf", 
            book.Id, d => progress = d);
          ElasticProvider.Instance.BulkIndex(bookPages);
        }
        Log.Info("Missing books have been indexed.");
      }

      var booksOnlyInIndex = booksInIndex.Except(booksOnDisk, new BookEqualityComparer()).ToList();
      if (booksOnlyInIndex.Any())
      {
        Log.Debug($"The number of books contained in the index but not on disk: {booksOnlyOnDisk.Count(b => b.Id != Guid.Empty)}");
        foreach (var book in booksOnlyInIndex)
          ElasticProvider.Instance.DeleteBookWithPages(book);
        Log.Info("Extra books have been deleted from disk.");
      }
    }
  }

  /// <summary>
  /// Компаратор для книг.
  /// </summary>
  internal class BookEqualityComparer : IEqualityComparer<Book>
  {
    public bool Equals(Book x, Book y)
    {
      if (ReferenceEquals(x, y)) return true;
      if (ReferenceEquals(x, null)) return false;
      if (ReferenceEquals(y, null)) return false;
      if (x.GetType() != y.GetType()) return false;
      return x.Id.Equals(y.Id) && x.Name == y.Name;
    }

    public int GetHashCode(Book obj)
    {
      unchecked
      {
        return (obj.Id.GetHashCode() * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
      }
    }
  }
}