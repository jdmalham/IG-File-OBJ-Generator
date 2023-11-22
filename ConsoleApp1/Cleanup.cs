using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    public class CleanupParameters
    {
        public string TempName { get; set; }
        public string TargetPath { get; set; }
        public string DeletionPath { get; set; }
    }


    public class CleanupHandler
    {
        private CleanupParameters parameters;

        public CleanupHandler(string tempName, string targetPath, string deletionPath)
        {
            parameters = new CleanupParameters
            {
                TempName = tempName,
                TargetPath = targetPath,
                DeletionPath = deletionPath
            };
        }
    }
        //Cleanup class that handles cleanup processes for the parser.
    internal class Cleanup
    {
        private static dynamic temp_name;
        private static dynamic targetPath;
        private static dynamic deletionPath;

        public static void setParameters(dynamic temp_name_i, dynamic targetPath, dynamic deletionPath)
        {
            temp_name = temp_name_i;
            targetPath = targetPath;
            deletionPath = deletionPath;
        }

        public static void callCleanup()
        {
            // Code inside this block will be executed just before the program exits
            Console.WriteLine("Executing cleanup before exit...");
            try
            {
                if (deletionPath != null)
                {
                    CleanupTempFiles(temp_name, deletionPath);
                }
                else
                {
                    // Handle the case when deletionPath is null (if needed)
                    Console.WriteLine("deletionPath is null. Unable to perform cleanup.");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                Console.WriteLine($"An error occurred during cleanup: {ex.Message}");
            }
        }
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
