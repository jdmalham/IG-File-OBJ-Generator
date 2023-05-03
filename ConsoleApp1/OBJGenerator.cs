using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
/*

What's going on in this file? It's just the main file. Why is it so messy? Good question. 

 */
class OBJGenerator
{
    static void Main(string[] args)
    {
        bool inputState = args.Length == 0;
        Unzip zipper;
        if (inputState)
        {
            zipper = new Unzip(@"C:\Users\uclav\Desktop\IG\Hto4l_120-130GeV.ig");
        }
        else
        {
            zipper = new Unzip(args[0]);
        }
        StreamReader file;
        string eventName;
        string strPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
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
            string destination = zipper.currentFile;
            string[] split = destination.Split('\\');
            eventName = split.Last();
            Console.WriteLine(eventName);

            string text = File.ReadAllText($"{destination}");
            string newText = text.Replace("nan,", "null,");

            File.WriteAllText($"{args[0]}.tmp", newText);
            file = File.OpenText($"{args[0]}.tmp");
        }
        strPath += "//" + eventName;
        JsonTextReader reader = new JsonTextReader(file);
        JObject o2 = (JObject)JToken.ReadFrom(reader);

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
            Communicate bridge = new Communicate(@"C:\Users\uclav\AppData\Local\Android\Sdk\platform-tools\adb.exe");
            bridge.UploadFiles(strPath);
        } catch (Exception e) {

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
}