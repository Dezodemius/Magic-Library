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
      var booksInIndex = ElasticProvider.Instance.GetAll();

      var booksOnlyOnDisk = booksOnDisk.Except(booksInIndex.ToList());
      if (booksOnlyOnDisk.Any())
      {
        Log.Debug($"The number of books contained on disk but not indexed: {booksOnlyOnDisk.Count(b => b.Id > 0)}");
        ElasticProvider.Instance.BulkIndex(booksOnlyOnDisk);
        Log.Info("Missing books have been indexed.");
      }

      var booksOnlyInIndex = booksInIndex.Except(booksOnDisk).ToList();
      if (booksOnlyInIndex.Any())
      {
        Log.Debug($"The number of books contained in the index but not on disk: {booksOnlyOnDisk.Count(b => b.Id > 0)}");
        ElasticProvider.Instance.BulkDelete(booksOnlyInIndex);
        Log.Info("Extra books have been deleted from disk.");
      }
    }
  }
}