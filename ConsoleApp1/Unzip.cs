using System.IO.Compression;

namespace IGtoOBJGen
{
    internal class Unzip
    {
        private string directoryName { get; set; }
        public Unzip(string filename)
        {
            unzipIG(filename);
        }
        public  void unzipIG(string path)
        {
            string extractPath = tempDirectoryPath();
            ZipFile.ExtractToDirectory(path, extractPath);
            directoryName = extractPath;
            string runFolder = selectFolderFromFolder(directoryName+"\\Events");
            string file = selectFileFromFolder(runFolder);
            
            Directory.Delete(directoryName);
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
        public static string selectFolderFromFolder(string path)
        {
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (string folder in folders)
            {
                int index = Array.IndexOf(folders, folder);
                Console.WriteLine($"{index}) {folder}");
            }
            Console.WriteLine("Enter ID # of desired path:");
            int selection = int.Parse(Console.ReadLine());
            return folders[selection];
        }
        public static string selectFileFromFolder(string path)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                int index = Array.IndexOf(files, file);
                Console.WriteLine($"{index}) {file}");
            }
            Console.WriteLine("Enter ID # of desired event file:");
            int selection = int.Parse(Console.ReadLine());
            Console.WriteLine(files[selection]);

            return files[selection];
        }
    }
}