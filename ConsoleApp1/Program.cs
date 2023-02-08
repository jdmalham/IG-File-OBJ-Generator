// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

struct BoxData
{
    public string name;
    public List<double> front_1;
    public List<double> front_2;
    public List<double> front_3;
    public List<double> front_4;
    public List<double> back_1;
    public List<double> back_2;
    public List<double> back_3;
    public List<double> back_4;
};
class Program
{
    static void Main(string[] args)
    {
        StreamReader file = File.OpenText("C:\\Users\\Owner\\Desktop\\Functional Scripts\\IG Files\\BJetPlusX_Run2012C_0 (2)\\Events\\Run_198230\\Event_1096322990");
        JsonTextReader reader = new JsonTextReader(file);
        JObject o2 = (JObject)JToken.ReadFrom(reader);
        List<BoxData> dataList = new List<BoxData>();
        if (o2["Collections"]["MuonChambers_V1"] != null)
        {
            foreach (JToken d in o2["Collections"]["MuonChambers_V1"])
            {
                BoxData entry = new BoxData();
                entry.name = "MuonChambers";
                entry.front_1 = d[1].ToObject<List<double>>();
                entry.front_2 = d[2].ToObject<List<double>>();
                entry.front_3 = d[3].ToObject<List<double>>();
                entry.front_4 = d[4].ToObject<List<double>>();
                entry.back_1 = d[5].ToObject<List<double>>();
                entry.back_2 = d[6].ToObject<List<double>>();
                entry.back_3 = d[7].ToObject<List<double>>();
                entry.back_4 = d[8].ToObject<List<double>>();
                dataList.Add(entry);
            }
        }
        List<List<BoxData>> n = GetBoxData(o2);
        foreach (var thing in n)
        {
            if (thing != new List<BoxData>() { })
            {
                Console.WriteLine(thing.Count.ToString());
                string name = thing[0].name;
                List<string> Contents = GenerateBoxGeometry(thing);
                File.WriteAllText($"C:\\Users\\Owner\\Desktop\\{name}.obj", String.Empty);
                File.WriteAllLines($"C:\\Users\\Owner\\Desktop\\{name}.obj", Contents);
            }
        }
        //List<string> objContents = GenerateBoxGeometry(n);
        //File.WriteAllText("C:\\Users\\Owner\\Desktop\\SavedLists.obj", String.Empty);
        //File.WriteAllLines("C:\\Users\\Owner\\Desktop\\SavedLists.obj", objContents);
    }
    public static List<string> GenerateBoxGeometry(List<BoxData> inputData)
    {
        List<string> geometryData = new List<string>();
        List<string> faceDeclarations = new List<string>();
        int counter = 1;
        foreach (BoxData box in inputData)
        {
            geometryData.Add($"o {box.name}");
            geometryData.Add($"v {box.front_1[0]} {box.front_1[1]} {box.front_1[2]}");
            geometryData.Add($"v {box.front_2[0]} {box.front_2[1]} {box.front_2[2]}");
            geometryData.Add($"v {box.front_3[0]} {box.front_3[1]} {box.front_3[2]}");
            geometryData.Add($"v {box.front_4[0]} {box.front_4[1]} {box.front_4[2]}");
            geometryData.Add($"v {box.back_1[0]} {box.back_1[1]} {box.back_1[2]}");
            geometryData.Add($"v {box.back_2[0]} {box.back_2[1]} {box.back_2[2]}");
            geometryData.Add($"v {box.back_3[0]} {box.back_3[1]} {box.back_3[2]}");
            geometryData.Add($"v {box.back_4[0]} {box.back_4[1]} {box.back_4[2]}");
            faceDeclarations.Add($"f {counter} {counter+1} {counter+2} {counter+3}");
            faceDeclarations.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
            faceDeclarations.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
            faceDeclarations.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
            faceDeclarations.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
            faceDeclarations.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
            counter+=8;
        }
        foreach (var item in faceDeclarations)
        {
            geometryData.Add(item);
        }
        return geometryData;
    }
    public static List<List<BoxData>> GetBoxData(JObject igContents)
    {
        List<List<BoxData>> geometries = new List<List<BoxData>>();
        List<List<string>> boxTypes = new List<List<string>>();
        foreach (var item in igContents["Types"])
        {
            foreach(var thing in item)
            {
                int i = 0;
                foreach (var gross in thing)
                {
                    if (gross[0].ToString() == "front_1")
                    {
                        List<string> pairingData = new List<string>(){ item.Path.Split('.')[1], i.ToString()};
                        boxTypes.Add(pairingData);
                    }
                    i++;
                }
            }
        }
        foreach(var pair in boxTypes)
        {
            List<BoxData> geometryFromPair = new List<BoxData>();
            string name = pair[0];
            int index = int.Parse(pair[1]);
            foreach(JToken dataItem in igContents["Collections"][name])
            {
                BoxData entry = new BoxData();
                entry.name = name;
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