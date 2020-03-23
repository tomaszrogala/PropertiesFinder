using System.Collections.Generic;

namespace Models
{
    public interface IDumpsRepository
    {
        /// <summary>
        /// Załaduj wszystkie dumpy, jednak nie ładuj poszczególnych ofert w celu optymalizacji pamięci
        /// </summary>
        /// <param name="webPage"></param>
        /// <returns></returns>
        IEnumerable<DumpDetails> GetAllDumpDetails(WebPage webPage);

        /// <summary>
        /// Pobierz konkretnego dumpa wraz z ofertami
        /// </summary>
        /// <param name="oldDumpDetails"></param>
        /// <returns></returns>
        Dump GetDump(DumpDetails dumpDetails);

        /// <summary>
        /// Wstaw dump do repozytorium
        /// </summary>
        /// <param name="dump"></param>
        void InsertDump(Dump dump);
        void UpdateDump(Dump dump);
    }
}
