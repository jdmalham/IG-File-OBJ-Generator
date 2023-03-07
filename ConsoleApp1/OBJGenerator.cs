using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Xml.Linq;

/*

What's going on in this file? It's just the main file. Why is it so messy? Good question. 

 */
class OBJGenerator
{
    static void Main(string[] args)
    {
        var watch = new Stopwatch();watch.Start();

        //Read in IG file as JSON Object. Thanks Newtonsoft
        string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        StreamReader file = File.OpenText($"C:\\Users\\uclav\\Source\\Repos\\jdmalham\\IG-File-OBJ-Generator\\ConsoleApp1\\IGdata\\Event_1096322990");
        JsonTextReader reader = new JsonTextReader(file);JObject o2 = (JObject)JToken.ReadFrom(reader);
        string strPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        


        // Types of calorimetry data we will get from the IG file
        string[] calorimetryItems = { "EBRecHits_V2", "EERecHits_V2", "ESRecHits_V2", "HBRecHits_V2" };
        
        List<List<CalorimetryData>> boxObjectsGesamt = new List<List<CalorimetryData>>(); // "The box objects have been gathered" Very loose translation of the German
        
        // Right here we are getting our calorimetry data from the IG file and adding them into boxObjectsGesamt
        foreach (string name in calorimetryItems)
        {
            boxObjectsGesamt = IGBoxes.calorimetryParse(o2, name, boxObjectsGesamt);
        }
        //Now make all the box shaped models from the box objects
        foreach (var thing in boxObjectsGesamt)
        {
                string name = thing[0].name;
                List<string> Contents = IGBoxes.generateCalorimetryModels(thing);
                File.WriteAllText($"{strPath}\\{name}.obj",String.Empty);
                File.WriteAllLines($"{strPath}\\{name}.obj",Contents);
        }

        //Get the data that is needed to generate track objects from the IG file (look at definition for explanation of the name)

        List<TrackExtrasData> listicle = IGTracks.trackExtrasParse(o2);

        IGTracks.trackCubicBezierCurve(listicle,32);  //Create the cubic bezier curve object file based off the track data

        var n = IGTracks.photonParse(o2); // Get photon data
        IGTracks.generatePhotonModels(n); // This one is a mystery. I don't know what it does. I can't figure it out. WHAT DOES GENERATE PHOTON MODELS MEAN?????
        
        List<MuonChamberData> list = IGBoxes.muonChamberParse(o2); // Get muon chamber data
        IGBoxes.generateMuonChamberModels(list); // Generate muon chamber obj files
        
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms"); // See how fast code runs. Code goes brrrrrrr on big office pc. It makes me happy. :)
    }
}