using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
struct MuonChamberData
{
    public string name;
    public int detid;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
};
struct CalorimetryData
{
    public string name;
    public double energy;
    public double eta;
    public double phi;
    public double time;
    public int detid;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
}
struct METData
{
    public double phi;
    public double pt;
    public double px;
    public double py;
    public double pz;
}
struct MuonData
{
    public string name;
    public double pt;
    public int charge;
    public List<double> position;
    public double phi;
    public double eta;
    public double caloEnergy;
}
struct PhotonData
{
    public string name;
    public double energy;
    public double et;
    public double eta;
    public double phi;
    public List<double> position;
}
namespace IGtoOBJGen
{
    internal class IGPhotons
    {
        public static List<PhotonData> photonParse(JObject data)
        {
            List<PhotonData> dataList = new List<PhotonData>();
            foreach (var igPhotonData in data["Collections"]["Photons_V1"])
            {
                PhotonData currentPhotonItem = new PhotonData();
                var children = igPhotonData.Children().Values<double>().ToList();
                currentPhotonItem.energy = children[0];
                currentPhotonItem.et = children[1];
                currentPhotonItem.eta = children[2];
                currentPhotonItem.phi = children[3];
                currentPhotonItem.position = new List<double>() { children[4], children[5], children[6] };
                dataList.Add(currentPhotonItem);
            }
            return dataList;
        }
        public static string makePhoton(PhotonData data)//string filePath)
        {
            double lEB = 3.0; //half-length of ECAL barrel in meters
            double rEB = 1.24; //radius of ECAL barrel in meters
            double eta = data.eta;
            double phi = data.phi;
            double energy = data.energy;
            double px = Math.Cos(phi);
            double py = Math.Sin(phi);
            double pz = Math.Sinh(eta);
            double x0 = data.position[0];
            double y0 = data.position[1];
            double z0 = data.position[2];
            double t = 0.0;
            if (Math.Abs(eta) > 1.48)
            {
                t = Math.Abs((lEB - z0) / pz);
            }
            else
            {
                double a = px * px + py * py;
                double b = 2 * x0 * px + 2 * y0 * py;
                double c = x0 * x0 + y0 * y0 - rEB * rEB;
                t = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            }
            List<double> pt1 = new List<double>() { x0, y0, z0 };
            List<double> pt2 = new List<double>() { x0 + px * t, y0 + py * t, z0 + pz * t };
            string Contents;
            Contents = $"v {x0} {y0} {z0}\nv {x0+0.001} {y0+0.001} {z0 + 0.001}\nv {x0 + px * t} {y0 + py * t} {z0 + pz * t}\nv {x0 + px * t + 0.001} {y0 + py * t + 0.001} {z0 + pz * t + 0.001}";
            return Contents;
        }
        public static void generatePhotonModels(List<PhotonData> dataList)
        {
            List<string> dataStrings = new List<string>();
            int counter = 1;
            foreach (var igPhotonData in dataList)
            {
                string objData = makePhoton(igPhotonData);
                dataStrings.Add(objData);
                dataStrings.Add($"f {counter} {counter+1} {counter + 3} {counter + 2}");
                counter += 4;
            }
            File.WriteAllText("C:\\Users\\Owner\\source\\repos\\ConsoleApp1\\ConsoleApp1\\objModels\\Photons_V1.obj", String.Empty);
            File.WriteAllLines("C:\\Users\\Owner\\source\\repos\\ConsoleApp1\\ConsoleApp1\\objModels\\Photons_V1.obj", dataStrings);
        }
    }
}
