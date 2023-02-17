using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using LineHandlers;
using BoxHandlers;
class OBJGenerator
{
    static void Main(string[] args)
    {
        var watch = new Stopwatch(); watch.Start();
        StreamReader file = File.OpenText("C:\\Users\\Owner\\Desktop\\Functional Scripts\\IG Files\\BJetPlusX_Run2012C_0 (2)\\Events\\Run_198230\\Event_1096322990");
        JsonTextReader reader = new JsonTextReader(file);JObject o2 = (JObject)JToken.ReadFrom(reader);
        List<List<CalorimetryBoxData>> boxObjectsGesamt = GetCalorimetryBoxData(o2);
        foreach (var thing in boxObjectsGesamt)
        {
            if (thing != new List<CalorimetryBoxData>() { })
            {
                string name = thing[0].name;
                List<string> Contents = GenerateBoxGeometry(thing);
                File.WriteAllText($"C:\\Users\\Owner\\Desktop\\{name}.obj",String.Empty);
                File.WriteAllLines($"C:\\Users\\Owner\\Desktop\\{name}.obj",Contents);
            }
        }
        var n = IGPhotons.photonParse(o2);
        IGPhotons.generatePhotonModelsData(n);
        List<MuonChamberData> list = IGBoxes.muonChamberParse(o2);
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms");
    }
    public static List<string> GenerateBoxGeometry(List<CalorimetryBoxData> inputData)
    {
        List<string> geometryData = new List<string>();
        List<string> faceDeclarations = new List<string>();
        int counter = 1;
        foreach (CalorimetryBoxData box in inputData)
        {
            geometryData.Add($"o {box.name}");
            geometryData.Add($"v {box.front_1[0] * box.energy} {box.front_1[1] * box.energy} {box.front_1[2] * box.energy}");
            geometryData.Add($"v {box.front_2[0] * box.energy} {box.front_2[1] * box.energy} {box.front_2[2] * box.energy}");
            geometryData.Add($"v {box.front_3[0] * box.energy} {box.front_3[1] * box.energy} {box.front_3[2] * box.energy}");
            geometryData.Add($"v {box.front_4[0] * box.energy} {box.front_4[1] * box.energy} {box.front_4[2] * box.energy}");
            geometryData.Add($"v {box.back_1[0] * box.energy} {box.back_1[1] * box.energy} {box.back_1[2] * box.energy}");
            geometryData.Add($"v {box.back_2[0] * box.energy} {box.back_2[1] * box.energy} {box.back_2[2] * box.energy}");
            geometryData.Add($"v {box.back_3[0] * box.energy} {box.back_3[1] * box.energy} {box.back_3[2] * box.energy}");
            geometryData.Add($"v {box.back_4[0] * box.energy} {box.back_4[1] * box.energy} {box.back_4[2] * box.energy}");
            faceDeclarations.Add($"f {counter} {counter+1} {counter+2} {counter+3}");
            faceDeclarations.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
            faceDeclarations.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
            faceDeclarations.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
            faceDeclarations.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
            faceDeclarations.Add($"f {counter+4} {counter + 7} {counter + 3} {counter}");
            faceDeclarations.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
            faceDeclarations.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
            faceDeclarations.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
            faceDeclarations.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
            faceDeclarations.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
            faceDeclarations.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");
            counter+=8;
        }
        foreach (var item in faceDeclarations)
        {
            geometryData.Add(item);
        }
        return geometryData;
    }
    public static List<List<CalorimetryBoxData>> GetCalorimetryBoxData(JObject igContents)
    {
        List<List<CalorimetryBoxData>> geometries = new List<List<CalorimetryBoxData>>();
        List<List<string>> boxTypes = new List<List<string>>();
        foreach (JProperty iSpyItem in igContents["Types"])
        {
            foreach(var subArray in iSpyItem)
            {
                int i = 0;
                foreach (var gross in subArray)
                {
                    if (gross[0].ToString() == "front_1" && subArray[0][0].ToObject<string>() == "energy")
                    {
                        List<string> pairingData = new List<string>(){ iSpyItem.Name, i.ToString()};
                        boxTypes.Add(pairingData);
                    }
                    i++;
                }
            }
        }
        foreach(var pair in boxTypes)
        {
            List<CalorimetryBoxData> geometryFromPair = new List<CalorimetryBoxData>();
            string name = pair[0];
            int index = int.Parse(pair[1]);
            foreach(JToken dataItem in igContents["Collections"][name])
            {
                CalorimetryBoxData entry = new CalorimetryBoxData();
                entry.name = name;
                entry.energy = dataItem[0].ToObject<double>();
                entry.front_1 = dataItem[index].ToObject<List<double>>();
                entry.front_2 = dataItem[index+1].ToObject<List<double>>();
                entry.front_3 = dataItem[index+2].ToObject<List<double>>();
                entry.front_4 = dataItem[index+3].ToObject<List<double>>();
                entry.back_1 = dataItem[index+4].ToObject<List<double>>();
                entry.back_2 = dataItem[index+5].ToObject<List<double>>();
                entry.back_3 = dataItem[index+6].ToObject<List<double>>();
                entry.back_4 = dataItem[index+7].ToObject<List<double>>();
                geometryFromPair.Add(entry);
            }
            if (geometryFromPair.Count != 0)
            {
                geometries.Add(geometryFromPair);
            }
        }
        return geometries;
    }
}