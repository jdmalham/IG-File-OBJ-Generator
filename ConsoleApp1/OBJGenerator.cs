using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
class OBJGenerator
{
    static void Main(string[] args)
    {
        var watch = new Stopwatch();watch.Start();
        StreamReader file = File.OpenText("C:\\Users\\Owner\\source\\repos\\ConsoleApp1\\ConsoleApp1\\IGdata\\Event_1096322990");
        JsonTextReader reader = new JsonTextReader(file);JObject o2 = (JObject)JToken.ReadFrom(reader);
        
        string[] calorimetryItems = { "EBRecHits_V2", "EERecHits_V2", "ESRecHits_V2", "HBRecHits_V2" };
        
        List<List<CalorimetryData>> boxObjectsGesamt = new List<List<CalorimetryData>>();
        
        foreach (string name in calorimetryItems)
        {
            boxObjectsGesamt = IGBoxes.calorimetryParse(o2, name, boxObjectsGesamt);
        }
        
        foreach (var thing in boxObjectsGesamt)
        {
                string name = thing[0].name;
                List<string> Contents = IGBoxes.generateCalorimetryModels(thing);
                File.WriteAllText($"C:\\Users\\Owner\\Desktop\\{name}.obj",String.Empty);
                File.WriteAllLines($"C:\\Users\\Owner\\Desktop\\{name}.obj",Contents);
        }

        List<TrackExtrasData> listicle = IGPhotons.trackExtrasParse(o2);

        IGPhotons.trackCubicBezierCurve(listicle,32);

        var n = IGPhotons.photonParse(o2);
        IGPhotons.generatePhotonModels(n);
        
        List<MuonChamberData> list = IGBoxes.muonChamberParse(o2);
        IGBoxes.generateMuonChamberModels(list);
        
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms");
    }
}