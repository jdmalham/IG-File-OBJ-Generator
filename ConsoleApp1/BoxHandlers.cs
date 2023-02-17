using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxHandlers
{
    internal class IGBoxes
    {
        public static List<MuonChamberData> muonChamberParse(JObject data)
        {
            var watch = new Stopwatch();watch.Start();
            var dataList = new List<MuonChamberData>();
            foreach (var igChamberData in data["Collections"]["MuonChambers_V1"])
            {
                MuonChamberData muonChamberData = new MuonChamberData();
                var children = igChamberData.Children().Values<double>().ToList();
                muonChamberData.name = "MuonChambers_V1";
                muonChamberData.detid = (int)children[0];
                muonChamberData.front_1 = new List<double>() {children[1],children[2],children[3]};
                muonChamberData.front_2 = new List<double>() {children[4],children[5],children[6]};
                muonChamberData.front_3 = new List<double>() {children[7],children[8],children[9]};
                muonChamberData.front_4 = new List<double>() {children[10],children[11],children[12]};
                muonChamberData.back_1 = new List<double>() {children[13],children[14],children[15]};
                muonChamberData.back_2 = new List<double>() {children[16],children[17],children[18]};
                muonChamberData.back_3 = new List<double>() {children[19],children[20],children[21]};
                muonChamberData.back_4 = new List<double>() {children[22],children[23],children[24]};
                dataList.Add(muonChamberData);
            }
            Console.WriteLine($"Chamber Parse Time: {watch.ElapsedMilliseconds} ms");
            return dataList;
        }
        public static List<CalorimetryBoxData> calorimetryParse(JObject data)
        {
            var dataList = new List<CalorimetryBoxData>();
            return dataList;
        }
    }
}
