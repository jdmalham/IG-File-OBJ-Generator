using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    internal static class ConfigHandler
    {
        public static void ParseCSV(string path) 
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<ConfigValue>();
                Console.WriteLine(records);
            }
        }
    }
}
