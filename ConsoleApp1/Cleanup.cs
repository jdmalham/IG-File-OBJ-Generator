using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    //Cleanup class that handles cleanup processes for the parser.
    internal class Cleanup
    {
        public static void CleanupTempFiles(string temp_name, string targetPath)
        {
            // Verify if the target directory exists
            if (Directory.Exists(targetPath))
            {
                //obtain list of items in the target directory
                string[] filesAndFolders = Directory.GetFileSystemEntries(targetPath);

                foreach (var fileOrFolder in filesAndFolders) //loop through items
                {
                    try
                    {
                        if (fileOrFolder.Contains(temp_name)) //if file or folder has the temp prefix, delete it
                        {
                            if (File.Exists(fileOrFolder))
                            {
                                File.Delete(fileOrFolder); // Delete file
                                //Console.WriteLine($"Deleted file: {fileOrFolder}");
                            }
                            else if (Directory.Exists(fileOrFolder))
                            {
                                Directory.Delete(fileOrFolder, true); // Delete directory and its contents recursively
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete: {fileOrFolder}. Error: {ex.Message}");
                    }
                }

                Console.WriteLine("Temp Files Deleted");
            }
            else
            {
                Console.WriteLine($"Target directory not found: {targetPath}");
            }
        }
    }
}
