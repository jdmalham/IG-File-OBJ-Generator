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
        StreamReader file;
        string eventName;
        string strPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var watch = new Stopwatch();
        watch.Start();

        IGTracks trackHandler = new IGTracks(); 
        if (args.Length == 0){
            file = File.OpenText("C:\\Users\\uclav\\Source\\Repos\\jdmalham\\IG-File-OBJ-Generator\\ConsoleApp1\\IGdata\\Event_1096322990");
            eventName = "Event_1096322990";
        } else {
            /*  Right so what's all this? We get the name of the event and then
            find and replace all occurrences of nan that are in the original file
            with null so that the JSON library can properly parse it. Store the revisions in a temp file that
            is deleted at the end of the program's execution so that the original file goes unchanged and can 
            still be used with iSpy  */

            string[] split = args[0].Split('\\');
            eventName = split.Last();
            
            string text = File.ReadAllText($"{args[0]}");
            string newText = text.Replace("nan,","null,");
            
            File.WriteAllText($"{args[0]}.tmp",newText );
            file = File.OpenText($"{args[0]}.tmp");
        }

        //Read in IG file as JSON Object. Thanks Newtonsoft
        JsonTextReader reader = new JsonTextReader(file);
        JObject o2 = (JObject)JToken.ReadFrom(reader);

        file.Close();

        if (args.Length > 0)
        {
            File.Delete($"{args[0]}.tmp");
        }

        // Types of calorimetry data we will get from the IG file
        string[] calorimetryItems = { "EBRecHits_V2", "EERecHits_V2", "ESRecHits_V2", "HBRecHits_V2" };
        
        List<List<CalorimetryData>> boxObjectsGesamt = new List<List<CalorimetryData>>(); 
        
        // Right here we are getting our calorimetry data from the IG file and adding them into boxObjectsGesamt
        foreach (string name in calorimetryItems)
        {
            boxObjectsGesamt = IGBoxes.calorimetryParse(o2, name, boxObjectsGesamt);
        }
        //Now make all the box shaped models from the box objects
        foreach (var thing in boxObjectsGesamt)
        {
            if (thing.Count() == 0) continue;
            string name = thing[0].name;
            if (name == "HBRecHits_V2")
            {
                var contents = IGBoxes.generateCalorimetryBoxes(thing);
                try
                {
                    File.WriteAllText($"{strPath}\\{eventName}\\{name}.obj", String.Empty);
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory($"{strPath}\\{eventName}");
                    File.WriteAllText($"{strPath}\\{eventName}\\{name}.obj", String.Empty);
                }
                File.WriteAllLines($"{strPath}\\{eventName}\\{name}.obj", contents);
                continue;
            }
            
            List<string> Contents = IGBoxes.generateCalorimetryTowers(thing);
            try
            {
                File.WriteAllText($"{strPath}\\{eventName}\\{name}.obj", String.Empty);
            } 
            catch (DirectoryNotFoundException) 
            {
                Directory.CreateDirectory($"{strPath}\\{eventName}");
                File.WriteAllText($"{strPath}\\{eventName}\\{name}.obj", String.Empty);
            }
            File.WriteAllLines($"{strPath}\\{eventName}\\{name}.obj",Contents);

        }

        //Get the data that is needed to generate track objects from the IG file (look at definition for explanation of the name)
        
        List<TrackExtrasData> listicle = trackHandler.trackExtrasParse(o2);

        trackHandler.trackCubicBezierCurve(listicle, 32, eventName);  //Create the cubic bezier curve object file based off the track data

        var n = trackHandler.photonParse(o2); // Get photon data
        trackHandler.generatePhotonModels(n, eventName); // This one is a mystery. I don't know what it does. I can't figure it out. WHAT DOES GENERATE PHOTON MODELS MEAN?????

        List<MuonChamberData> list = IGBoxes.muonChamberParse(o2); // Get muon chamber data
        IGBoxes.generateMuonChamberModels(list); // Generate muon chamber obj files
        
        
        List<JetData> jetList = IGBoxes.jetParse(o2);
        IGBoxes.generateJetModels(jetList);

        try
        {
            Communicate bridge = new Communicate(@"C:\Users\uclav\AppData\Local\Android\Sdk\platform-tools\adb.exe");
            bridge.DownloadFiles("Photons_V1.obj");
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
        //bridge.UploadFiles(trackHandler.filePaths);
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms"); // See how fast code runs. Code goes brrrrrrr on fancy office pc. It makes me happy. :)
    }
}