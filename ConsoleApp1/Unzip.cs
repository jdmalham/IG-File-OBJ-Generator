using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    internal class Unzip
    {
        public static void unzipIG(string path)
        {
            string extractPath = tempDirectoryPath();
            ZipFile.ExtractToDirectory(path, extractPath);

        }
        private static string tempDirectoryPath()
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);

            return tempFolder;
        }
    }
}
