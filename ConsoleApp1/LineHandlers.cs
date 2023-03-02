using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Numerics;
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
                currentPhotonItem.position = new Vector3 ( (float)children[4], (float)children[5], (float)children[6] );
                
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
            double x0 = data.position.X;
            double y0 = data.position.Y;
            double z0 = data.position.Z;
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
        public static void trackCubicBezierCurve(List<TrackExtrasData> data, int numVerts) {
            List<string> dataList = new List<string>();
            List<int> exclusion_indeces = new List<int>();
            int n = 0;

            foreach (var item in data) {
                //Console.Write('h');
                string track = "";
                for (int i = 0; i <= numVerts; i++) {
                    float t = (float)(i) / (float)(numVerts);

                    Vector3 term1 = Vector3.Multiply(item.pos1, (float)Math.Pow(1 - t, 3));
                    Vector3 term2 = Vector3.Multiply(item.pos3, (float)(3 * t * (float)Math.Pow(1 - t, 2)));
                    Vector3 term3 = Vector3.Multiply(item.pos4, (float)(3 * t * t * (1 - t)));
                    Vector3 term4 = Vector3.Multiply(item.pos2, (float)Math.Pow(t, 3));
                    Vector3 point = term1 + term2 + term3 + term4;

                    string poin_t = $"v {point.X} {point.Y} {point.Z}\nv {point.X+0.1f} {point.Y+0.01f} {point.Z +0.01f}\n";

                    track += poin_t;
                    n += 2;
                }
                dataList.Add(track);
                exclusion_indeces.Add(n);
            }
            
            for (int r = 1; r <= n - 2; r += 2) {
                if (exclusion_indeces.Contains(r+1)) {
                    continue;
                }
                string faces = $"f {r} {r+1} {r+3} {r+2}\nf {r+2} {r+3} {r+1} {r}";
                dataList.Add(faces);
                //Console.Write(faces);
            }
            File.WriteAllText("C:\\Users\\Owner\\Desktop\\tracks.obj", String.Empty);
            File.WriteAllLines("C:\\Users\\Owner\\Desktop\\tracks.obj", dataList);
        }

        public static List<TrackExtrasData> trackExtrasParse(JObject data) {
            List<TrackExtrasData> dataList = new List<TrackExtrasData>();

            foreach (var igTrackExtra in data["Collections"]["Extras_V1"]) {
                TrackExtrasData currentItem = new TrackExtrasData();

                var children = igTrackExtra.Children().Values<float>().ToList();

                currentItem.pos1 = new Vector3(children[0], children[1], children[2]);
                currentItem.dir1 = new Vector3(children[3], children[4], children[5]);
                currentItem.pos2 = new Vector3(children[6], children[7], children[8]);
                currentItem.dir2 = new Vector3(children[9], children[10], children[11]);

                float distance = Vector3.Distance(currentItem.pos1,currentItem.pos2);
                float scale = distance * 0.25f;

                currentItem.pos3 = new Vector3(children[0] + scale * children[3], children[1] + scale * children[4], children[2] + scale * children[5]);
                currentItem.pos4 = new Vector3(children[6] - scale * children[9], children[7] - scale * children[10], children[8] - scale * children[11]);

                dataList.Add(currentItem);
            }
            return dataList;
        }
    }
}
