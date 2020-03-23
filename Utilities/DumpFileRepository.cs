using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Utilities
{
    public class DumpFileRepository : IDumpsRepository
    {
        public IEnumerable<DumpDetails> GetAllDumpDetails(WebPage webPage)
        {
            var directoryInfo = new DirectoryInfo(GetDirectoryPath(webPage));

            if(!directoryInfo.Exists)
            {
                directoryInfo.Create();
                yield break;
            }
            foreach(var fileInfo in directoryInfo.GetFiles())
            {
                //Poniższy streamReader musi zakończyć się przed yieldem
                //w związku ze wspominaną na wykładzie budową interfejsu IEnumerable
                DumpDetails dump;
                using (var streamReader = new StreamReader(fileInfo.FullName))
                {
                    dump = JsonSerializer.Deserialize<DumpDetails>(streamReader.ReadToEnd());
                }
                yield return dump;
            }
        }

        public Dump GetDump(DumpDetails dumpDetails)
        {
            using var streamReader = new StreamReader(GetDumpFilePath(dumpDetails));
            return JsonSerializer.Deserialize<Dump>(streamReader.ReadToEnd());
        }

        public void InsertDump(Dump dump)
        {
            using var streamWriter = new StreamWriter(GetDumpFilePath(dump));
            var dumpJson = JsonSerializer.Serialize(dump);
            streamWriter.Write(dumpJson);
        }

        public void UpdateDump(Dump dump)
        {
            var dumpFilePath = GetDumpFilePath(dump);

            if (!File.Exists(dumpFilePath))
                throw new FileNotFoundException();

            File.Delete(dumpFilePath);

            using var streamWriter = new StreamWriter(dumpFilePath);
            var dumpJson = JsonSerializer.Serialize(dump);
            streamWriter.Write(dumpJson);
        }

        private string GetDumpFilePath(DumpDetails dumpDetails)
        {
            //Poniżej określone są ścieżki w jakim pliku ma być zapisany dump.
            var directoryPath = GetDirectoryPath(dumpDetails.WebPage);
            var filePath = $"{dumpDetails.DateTime.ToString("yyyyMMddHHmmssfff")}.dat";
            return $"{directoryPath}\\{filePath}";
        }

        private string GetDirectoryPath(WebPage webPage) => $"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\')}\\{webPage.Name}";
    }
}
