using Core;
using System;

namespace Library
{
    /// <summary>
    /// Главный объект приложения.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Стундартная точка входа в приложение.
        /// </summary>
        public static void Main()
        {
            bool isExit;
            do
            {
                Console.WriteLine("Please enter a search phrase: ");
                var searchPhrase = Console.ReadLine();
                var responseBookList = ElasticsearchProvider.Search(searchPhrase);

                if (responseBookList.Count > 0)                
                    foreach (var book in responseBookList)
                        Console.WriteLine($"{book.Id}\t{book.Author}\t{book.Name}");               

                Console.WriteLine("Press any key to reply or N for exit.");
                isExit = (Console.ReadKey(false).Key == ConsoleKey.N);
            } while (!isExit);            
        }
    }
}
