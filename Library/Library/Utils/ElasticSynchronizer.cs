using System;
using System.Collections.Generic;
using System.Diagnostics;
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
      ElasticProvider.Instance.Search("");
      var booksOnDisk = BookManager.Instance.GetAllBooks();
      var booksInIndex = ElasticProvider.Instance.GetAllBooks();

      var booksOnlyOnDisk = booksOnDisk.Except(booksInIndex.ToList());
      if (booksOnlyOnDisk.Any())
      {
        Log.Debug($"The number of books contained on disk but not indexed: {booksOnlyOnDisk.Count(b => b.Id != Guid.Empty)}");
        ElasticProvider.Instance.BulkIndex(booksOnlyOnDisk);
        foreach (var book in booksOnlyOnDisk)
        {
          var bookPages = ElasticProvider.Instance.GetAllPages(book.Id);
          ElasticProvider.Instance.BulkIndex(bookPages);
        }
        Log.Info("Missing books have been indexed.");
      }

      var booksOnlyInIndex = booksInIndex.Except(booksOnDisk).ToList();
      if (booksOnlyInIndex.Any())
      {
        Log.Debug($"The number of books contained in the index but not on disk: {booksOnlyOnDisk.Count(b => b.Id != Guid.Empty)}");
        ElasticProvider.Instance.BulkDelete(booksOnlyInIndex);
        Log.Info("Extra books have been deleted from disk.");
      }
    }
  }
}