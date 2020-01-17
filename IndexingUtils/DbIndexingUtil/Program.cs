using System;
using Core;

namespace DbIndexingUtil
{
    /// <summary>
    /// Основной объект для работы программы.
    /// </summary>
    public class Program
    {
        private static readonly DbManufacturer dbManufacturer = new DbManufacturer();

        /// <summary>
        /// Стандартная точка входа в программу.
        /// </summary>
        public static void Main()
        {
            try
            {
                var books = dbManufacturer.ExtractFromDb();
                ElasticsearchProvider.SendDataToElasticsearch(books);
            }
            catch (Exception e)
            {
                Console.WriteLine("Utility completed with error.\n" + e);
            }
            finally
            {
                Console.WriteLine("Press any key to exit. . .");
                Console.ReadKey();
            }
        }
    }
}
