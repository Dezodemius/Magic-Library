using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbIndexingUtil
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DbManufacturer.ExtractFromDb();

            Console.ReadKey();
        }
    }
}
