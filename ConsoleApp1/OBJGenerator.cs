using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
using MathNet.Numerics.LinearAlgebra;

class OBJGenerator
{
    static void Main(string[] args)
    {
        bool inputState;
        bool adbState;
        string appdata;
        string datapath;
        string eventName;
        string strPath;
        Unzip zipper;
        StreamReader file;
        JsonTextReader reader;
        JObject o2;

        inputState = args.Length == 0;
        adbState = ADBCheck();
        appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Android\Sdk\platform-tools\adb.exe";
        strPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

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
        }

        //ConfigHandler.ParseCSV(@"C:\Users\uclav\Source\Repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\config.csv");
        //TODO: Figure out what the fuck delegate does and how I can use it to make sure memory is managed well
        //Console.CancelKeyPress += delegate { zipper.destroyStorage(); };

        if (inputState)
        {
            zipper = new Unzip(@"C:\Users\uclav\Desktop\IG\Hto4l_120-130GeV.ig");
            datapath = @"C:\Users\uclav\Desktop\IG\Hto4l_120-130GeV.ig";
        }
        else
        {
            zipper = new Unzip(args[0]);
            datapath = args[0];
        }

        //Timer stopwatch
        var watch = new Stopwatch();
        watch.Start();

        if (inputState == true)
        {
            file = File.OpenText(@"C:\Users\uclav\source\repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\IGdata\Event_1096322990");
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
        }
        
        strPath += "\\" + eventName;
        
        reader = new JsonTextReader(file);
        o2 = (JObject)JToken.ReadFrom(reader);

        file.Close();

        if (inputState == false)
        {
            File.Delete($"{args[0]}.tmp");
        }

        IGTracks trackHandler = new IGTracks(o2, eventName);
        IGBoxes boxHandler = new IGBoxes(o2, eventName);

        zipper.destroyStorage();

        try
        {
            Communicate bridge = new Communicate(appdata);
            bridge.UploadFiles(strPath);
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
        string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Android\Sdk\platform-tools\adb.exe";
        if (File.Exists(appdata) == false) { state = false; }
        return state;
    }
    //Read in the config file to check for special location
    private static string ADBRead()
    {
        string path;
        var yuh = File.ReadAllLines(@"C:\Users\uclav\Source\Repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\config.txt");
        if (yuh.Length != 0)
        {
            path = File.ReadAllLines(@"C:\Users\uclav\Source\Repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\config.txt").First();
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
        File.WriteAllText(@"C:\Users\uclav\Source\Repos\jdmalham\IG-File-OBJ-Generator\ConsoleApp1\config.txt", path);
        return path;
    }
}