using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IGtoOBJGen
{
    internal class IGBoxes
    {
        private static string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public static List<MuonChamberData> muonChamberParse(JObject data)
        {
            var dataList = new List<MuonChamberData>();
            
            foreach (var igChamberData in data["Collections"]["MuonChambers_V1"])
            {
                MuonChamberData muonChamberData = new MuonChamberData();
                var children = igChamberData.Children().Values<double>().ToList();
                
                muonChamberData.name = "MuonChambers_V1";
                muonChamberData.detid = (int)children[0];
                muonChamberData.front_1 = new double[] {children[1],children[2],children[3]};
                muonChamberData.front_2 = new double[] {children[4],children[5],children[6]};
                muonChamberData.front_3 = new double[] {children[7],children[8],children[9]};
                muonChamberData.front_4 = new double[] {children[10],children[11],children[12]};
                muonChamberData.back_1 = new double[] {children[13],children[14],children[15]};
                muonChamberData.back_2 = new double[] {children[16],children[17],children[18]};
                muonChamberData.back_3 = new double[] {children[19],children[20],children[21]};
                muonChamberData.back_4 = new double[] {children[22],children[23],children[24]};
                
                dataList.Add(muonChamberData);
            }
            return dataList;
        }
        public static void generateMuonChamberModels(List<MuonChamberData> data)
        {
            List<string> dataStrings = new List<string>();
            int counter = 1;
            string name = data[0].name;
            
            foreach (var chamber in data)
            {
                dataStrings.Add($"o {name}");
                
                dataStrings.Add($"v {String.Join(' ',chamber.front_1)}");
                dataStrings.Add($"v {String.Join(' ',chamber.front_2)}");
                dataStrings.Add($"v {String.Join(' ',chamber.front_3)}");
                dataStrings.Add($"v {String.Join(' ',chamber.front_4)}");
                dataStrings.Add($"v {String.Join(' ',chamber.back_1)}");
                dataStrings.Add($"v {String.Join(' ',chamber.back_2)}");
                dataStrings.Add($"v {String.Join(' ',chamber.back_3)}");
                dataStrings.Add($"v {String.Join(' ',chamber.back_4)}");
                
                dataStrings.Add($"f {counter} {counter + 1} {counter + 2} {counter + 3}");
                dataStrings.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                dataStrings.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                dataStrings.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
                dataStrings.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
                dataStrings.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                dataStrings.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                dataStrings.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
                dataStrings.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
                dataStrings.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
                dataStrings.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
                dataStrings.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");
                counter += 8;
            }
            
            File.WriteAllText($"{path}\\test_obj\\{name}.obj", String.Empty);
            File.WriteAllLines($"{path}\\test_obj\\{name}.obj", dataStrings);
        }
        public static List<List<CalorimetryData>> calorimetryParse(JObject data, string name, List<List<CalorimetryData>> dataList)
        {
            var mediatingList = new List<CalorimetryData>();
            
            foreach (var item in data["Collections"][name])
            {
                CalorimetryData ebHitsData = new CalorimetryData();
                var children = item.Children().Values<double>().ToList();
                
                ebHitsData.name = name;
                ebHitsData.energy = children[0];
                ebHitsData.eta = children[1];
                ebHitsData.phi = children[2];
                ebHitsData.time = children[3];
                ebHitsData.detid = (int)children[4];
                ebHitsData.front_1 = new double[] { children[5], children[6], children[7] };
                ebHitsData.front_2 = new double[] { children[8], children[9], children[10] };
                ebHitsData.front_3 = new double[] { children[11], children[12], children[13] };
                ebHitsData.front_4 = new double[] { children[14], children[15], children[16] };
                ebHitsData.back_1 = new double[] { children[17], children[18], children[19] };
                ebHitsData.back_2 = new double[] { children[20], children[21], children[22] };
                ebHitsData.back_3 = new double[] { children[23], children[24], children[25] };
                ebHitsData.back_4 = new double[] { children[26], children[27], children[28] };
                mediatingList.Add(ebHitsData);
            }
            
            dataList.Add(mediatingList);
            return dataList;
        }
        public static List<string> generateCalorimetryModels(List<CalorimetryData> inputData)
        {
            List<string> geometryData = new List<string>();
            List<string> faceDeclarations = new List<string>();
            int counter = 1;
            
            foreach (CalorimetryData box in inputData)
            {
                //Don't you just love giant blocks of nearly identical code?
                geometryData.Add($"o {box.name}");
                geometryData.Add($"v {box.front_1[0] * box.energy} {box.front_1[1] * box.energy} {box.front_1[2] * box.energy}");
                geometryData.Add($"v {box.front_2[0] * box.energy} {box.front_2[1] * box.energy} {box.front_2[2] * box.energy}");
                geometryData.Add($"v {box.front_3[0] * box.energy} {box.front_3[1] * box.energy} {box.front_3[2] * box.energy}");
                geometryData.Add($"v {box.front_4[0] * box.energy} {box.front_4[1] * box.energy} {box.front_4[2] * box.energy}");
                geometryData.Add($"v {box.back_1[0] * box.energy} {box.back_1[1] * box.energy} {box.back_1[2] * box.energy}");
                geometryData.Add($"v {box.back_2[0] * box.energy} {box.back_2[1] * box.energy} {box.back_2[2] * box.energy}");
                geometryData.Add($"v {box.back_3[0] * box.energy} {box.back_3[1] * box.energy} {box.back_3[2] * box.energy}");
                geometryData.Add($"v {box.back_4[0] * box.energy} {box.back_4[1] * box.energy} {box.back_4[2] * box.energy}");
                
                faceDeclarations.Add($"f {counter} {counter + 1} {counter + 2} {counter + 3}");
                faceDeclarations.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                faceDeclarations.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                faceDeclarations.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
                faceDeclarations.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
                faceDeclarations.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                faceDeclarations.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                faceDeclarations.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
                faceDeclarations.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
                faceDeclarations.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
                faceDeclarations.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
                faceDeclarations.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");
                
                counter += 8;
            }
            foreach (var item in faceDeclarations)
            {
                geometryData.Add(item);
            }
            return geometryData;
        }
    }
}
