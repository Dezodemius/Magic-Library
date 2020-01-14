using System;

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
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            dbManufacturer.ExtractFromDb();

            Console.WriteLine("Press any key to exit. . .");
            Console.ReadKey();
        }
    }
}
