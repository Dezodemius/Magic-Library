using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbIndexingUtil
{
    /// <summary>
    /// Исключение, возникающее при использовании неизвестного провайдер БД.
    /// </summary>
    public class UnknownProviderNameException : Exception
    {
        public UnknownProviderNameException() : base()
        {
        }
    }
}
