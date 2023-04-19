using System.IO.Compression;

namespace IGtoOBJGen
{
    internal class Unzip
    {
        private string directoryName { get; set; }
        public  void unzipIG(string path)
        {
            string extractPath = tempDirectoryPath();
            ZipFile.ExtractToDirectory(path, extractPath);
            directoryName = extractPath;
        }
        private static string tempDirectoryPath()
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
        public static void getRuns(string path)
        {
            var directories = Directory.EnumerateDirectories(path);
            foreach (var d in directories)
            {
                Console.WriteLine(d);
            }
        }
    }
}
