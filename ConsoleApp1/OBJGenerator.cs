using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
using System.IO;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using System.Xml.Linq;
using System.Runtime.Loader;

class OBJGenerator
{
    static Assembly resources = Assembly.GetExecutingAssembly();
    static void Main(string[] args)
    {
        bool inputState;
        bool adbState;
        string appdata;
        string datapath;
        string eventName;
        string targetPath;
        Unzip zipper;
        StreamReader file;
        JsonTextReader reader;
        JObject o2;

        inputState = args.Length == 0;
        adbState = ADBCheck();
        appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Android\Sdk\platform-tools\adb.exe";
        if (args.Count() > 1)
        {
            targetPath = "";
            foreach(char flag in args[1].ToCharArray()) 
            {
                switch(flag)
                {
                    case 's':
                        targetPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        break;
                    default: targetPath = "hui"; 
                        Console.WriteLine("Invalid Argument");
                        Environment.Exit(1);
                        break;
                }
            }
        } else
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            targetPath = tempFolder;
            Console.CancelKeyPress += delegate { Directory.Delete(tempFolder, true); };
        }
        /*Console.WriteLine(adbState);
        if (adbState == false) {
            var stater = ADBRead();
            if (stater != null)
            {
                appdata = stater;
            }
            else
            {
                appdata = GetADBPathFromUser();
            }
        }*/

        //ConfigHandler.ParseCSV(@"C:\Users\uclav\Source\Repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\config.csv");
        

        if (inputState)
        {
            zipper = new Unzip(@"C:\Users\Joseph\Downloads\BJetPlusX_Run2012C_0.ig");
            datapath = @"C:\Users\uclav\Desktop\IG\Hto4l_120-130GeV.ig";
        }
        else
        {
            zipper = new Unzip(args[0]);
            datapath = args[0];
        }

        Console.CancelKeyPress += delegate { zipper.destroyStorage(); };
        zipper.Run();

        //Timer stopwatch
        var watch = new Stopwatch();
        watch.Start();

        if (inputState == true)
        {
            file = File.OpenText(@"C:\Users\Joseph\Downloads\MET\Run_202209\Event_20657260");
            eventName = "Event_1096322990";
        }
        else
        {
            /*  Right so what's all this? We get the name of the event and then
            find and replace all occurrences of nan that are in the original file
            with null so that the JSON library can properly parse it. Store the revisions in a temp file that
            is deleted at the end of the program's execution so that the original file goes unchanged and can 
            still be used with iSpy  */
            //zipper = new Unzip(args[0]);
            string destination = zipper.currentFile;
            string[] split = destination.Split('\\');
            eventName = split.Last();

            string text = File.ReadAllText($"{destination}");
            string newText = text.Replace("nan,", "null,");

            File.WriteAllText($"{args[0]}.tmp", newText);

            file = File.OpenText($"{args[0]}.tmp");
            Console.CancelKeyPress += delegate { file.Close(); File.Delete($"{args[0]}.tmp"); };
        }

        targetPath += "\\" + eventName;
        
        reader = new JsonTextReader(file);
        o2 = (JObject)JToken.ReadFrom(reader);

        file.Close();

        if (inputState == false)
        {
            File.Delete($"{args[0]}.tmp");
        }

        IGTracks t = new IGTracks(o2, targetPath);
        IGBoxes b = new IGBoxes(o2, targetPath);

        var totaljson = JsonConvert.SerializeObject(new {b.jetDatas,b.EEData, b.EBData, b.ESData, b.HEData, b.HBData, b.HOData, b.HFData, b.superClusters, t.globalMuonDatas, t.trackerMuonDatas, t.standaloneMuonDatas, t.electronDatas, t.trackDatas }, Formatting.Indented);
        File.WriteAllText($"{targetPath}//totalData.json",totaljson);
        //zipper.destroyStorage();
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms");
        try
        {
            Console.WriteLine(targetPath);
            Console.ReadLine();
            Communicate bridge = new Communicate(appdata);
            bridge.UploadFiles(targetPath);
        }
        catch (Exception e)
        {

            if (e is System.ArgumentOutOfRangeException)
            {
                Console.WriteLine("System.ArgumentOutOfRangeException thrown while trying to locate ADB.\nPlease check that ADB is installed and the proper path has been provided. The default path for Windows is C:\\Users\\[user]\\AppData\\Local\\Android\\sdk\\platform-tools\n");
            }
            else if (e is SharpAdbClient.Exceptions.AdbException)
            {
                Console.WriteLine("An ADB exception has been thrown.\nPlease check that the Oculus is connected to the computer.");
            }
            Environment.Exit(1);
        }
        
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms"); // See how fast code runs. Code goes brrrrrrr on fancy office pc. It makes me happy. :)
    }
    //Check for ADB in default location
    private static bool ADBCheck()
    {
        bool state = true;
        string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Android\Sdk\platform-tools\adb.exe";
        if (File.Exists(appdata) == false) { state = false; }
        return state;
    }
    //Read in the config file to check for special location
    private static string ADBRead()
    {
        string path;
        string resourcename = "IGtoOBJGen.config.txt";
        var yuh = File.ReadAllLines(@"C:\Users\uclav\Source\Repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\config.txt");
        if (yuh.Length != 0)
        {
            path = File.ReadAllLines(@"C:\Users\uclav\Source\Repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\config.txt").First();
            using (Stream stream = resources.GetManifestResourceStream(resourcename))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
            }
        } else
        {
            path = null;
        }
        return path;
    }
    //Called if the config file does not contain anything, allows the user to then specify what the path to be used from now on is.
    private static string GetADBPathFromUser()
    {
        string path;
        Console.WriteLine("No ADB path found. Please enter the local path for ADB, or install ADB to its default location:");
        path = Console.ReadLine();
        //Resources.config = path;
        return path;
    }
}