using Models;

namespace Interfaces
{
    public interface IDumpable
    {
        /// <summary>
        /// Wykonaj zrzut ofert ze strony
        /// </summary>
        /// <returns></returns>
        Dump GenerateDump();
    }
}
