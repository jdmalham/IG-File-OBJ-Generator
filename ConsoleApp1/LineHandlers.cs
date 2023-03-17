using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Numerics;
/*
 "Lines are cool 😎" - Pythagoras
 */
namespace IGtoOBJGen
{
    internal class IGTracks
    {
        protected string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public List<string> filePaths { get; set; }
        
        public IGTracks()
        {
            filePaths = new List<string>();
        }
        public List<PhotonData> photonParse(JObject data)
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
        protected string makePhoton(PhotonData data)
        {
            //Calculate path of the photons within the detector

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
            double t;
            
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
            
            string Contents;
            Contents = $"v {x0} {y0} {z0}\nv {x0+0.001} {y0+0.001} {z0 + 0.001}\nv {x0 + px * t} {y0 + py * t} {z0 + pz * t}\nv {x0 + px * t + 0.001} {y0 + py * t + 0.001} {z0 + pz * t + 0.001}";
            //Output a string of obj vectors that define the photon path
            return Contents;
        }
        public void generatePhotonModels(List<PhotonData> dataList,string eventName)
        {
            //Write obj files for the photons
            List<string> dataStrings = new List<string>();
            int counter = 1;

            foreach (var igPhotonData in dataList)
            {
                string objData = makePhoton(igPhotonData);
                //Hey! That's the function from above!
                dataStrings.Add(objData);
                dataStrings.Add($"f {counter} {counter+1} {counter + 3} {counter + 2}");
                counter += 4;
            }

            File.WriteAllText($"{desktopPath}\\{eventName}\\Photons_V1.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventName}\\Photons_V1.obj", dataStrings);
            filePaths.Add($"{desktopPath}\\{eventName}\\Photons_V1.obj");
            
        }
        public void trackCubicBezierCurve(List<TrackExtrasData> data, int numVerts,string eventName) {
            //Calculate the bezier path of the tracks based on the four pos control vectors defined in the TrackExtrasData struct
            List<string> dataList = new List<string>();
            List<int> exclusion_indeces = new List<int>();
            int n = 0;
            foreach (var item in data) {

                for (double i = 0.0; i <= numVerts; i++) {
                    double t = (double)(i) / (double)(numVerts);
                    
                    double tdiff3 = Math.Pow(1.0 - t, 3);
                    double threetimesTtdiff2 = 3 * t * Math.Pow(1.0 - t, 2);
                    double threetimesT2tdiff = 3 * t * t * (1.0 - t);
                    double t3 = Math.Pow(t, 3);

                    // Check out the wikipedia page for bezier curves if you want to understand the math. That's where I learned it!
                    //also we're using double arrays because i dont like Vector3 and floats. I'm the one who has to go through the headaches of working with double arrays
                    //instead of Vector3 so i get to make that call
                    double[] term1 = { tdiff3*item.pos1[0], tdiff3 * item.pos1[1], tdiff3 * item.pos1[2] };
                    double[] term2 = { threetimesTtdiff2 * item.pos3[0], threetimesTtdiff2 * item.pos3[1], threetimesTtdiff2 * item.pos3[2] };
                    double[] term3 = { threetimesT2tdiff * item.pos4[0], threetimesT2tdiff * item.pos4[1], threetimesT2tdiff * item.pos4[2] };
                    double[] term4 = { t3 * item.pos2[0], t3 * item.pos2[1], t3 * item.pos2[2] };
                    double[] point = { term1[0] + term2[0] + term3[0] + term4[0], term1[1] + term2[1] + term3[1] + term4[1], term1[2] + term2[2] + term3[2] + term4[2] };

                    //these vectors are only offset because this is gonna be used later for a untiy project, and it doesn't like obj lines. Instead, we define them as ribbons.                                                             
                    string poin_t = $"v {point[0]} {point[1]} {point[2]}";
                    string point_t2 = $"v {point[0]} {point[1] + 0.001} {point[2]}";

                    dataList.Add(poin_t); dataList.Add(point_t2);
                    n += 2;
                   
                }

                exclusion_indeces.Add(n);
            }
            
            for (int r = 1; r <= n - 2; r += 2) {
                if (exclusion_indeces.Contains(r+1)) {
                    //Make sure the tracks don't loop back on each other
                    continue;
                }
                // Define faces
                string faces1 = $"f {r} {r + 1} {r + 3} {r + 2}";
                string faces2 = $"f {r+2} {r+3} {r+1} {r}";
                dataList.Add(faces1);
                dataList.Add(faces2);
            }
            
            File.WriteAllText($"{desktopPath}\\{eventName}\\tracks.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventName}\\tracks.obj", dataList);
            filePaths.Add($"{desktopPath}\\{eventName}\\tracks.obj");
        }
        public List<TrackExtrasData> trackExtrasParse(JObject data) {
            List<TrackExtrasData> dataList = new List<TrackExtrasData>();
            foreach (var igTrackExtra in data["Collections"]["Extras_V1"]) {

                TrackExtrasData currentItem = new TrackExtrasData();

                var children = igTrackExtra.Children().Values<double>().ToList();
                
                currentItem.pos1 = new double[3] { children[0], children[1], children[2] };
                
                double dir1mag = Math.Sqrt(  //dir1mag and dir2mag are for making sure the direction vectors are normalized
                    (Math.Pow(children[3], 2) +
                    Math.Pow(children[4], 2) +
                    Math.Pow(children[5], 2))
                    );
                currentItem.dir1 = new double[3] { children[3]/dir1mag, children[4]/dir1mag, children[5]/dir1mag };
                
                currentItem.pos2 = new double[3] { children[6], children[7], children[8] };
                
                double dir2mag = Math.Sqrt(
                    (Math.Pow(children[6], 2) +
                    Math.Pow(children[7], 2) +
                    Math.Pow(children[8], 2))
                    );
                currentItem.dir2 = new double[3] { children[9]/dir2mag, children[10]/dir2mag, children[11]/dir2mag };

                double distance = Math.Sqrt(
                    Math.Pow((currentItem.pos1[0] - currentItem.pos2[0]),2) +
                    Math.Pow(currentItem.pos1[1] - currentItem.pos2[1],2) +
                    Math.Pow(currentItem.pos1[2] - currentItem.pos2[2],2)
                     );
                
                double scale = distance * 0.25;

                currentItem.pos3 = new double[3] { children[0] + scale * currentItem.dir1[0], children[1] + scale * currentItem.dir1[1], children[2] + scale * currentItem.dir1[2] };
                currentItem.pos4 = new double[3] { children[6] - scale * currentItem.dir2[0], children[7] - scale * currentItem.dir2[1], children[8] - scale * currentItem.dir2[2] };

                dataList.Add(currentItem);
            }
            return dataList;
        }
    }
}
