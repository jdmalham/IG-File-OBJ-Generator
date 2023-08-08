using MathNet.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Numerics;
namespace IGtoOBJGen
{
    internal class IGTracks
    {
        //Properties
        protected string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        protected string eventTitle;
        protected JObject data { get; set; }
        private List<TrackExtrasData> trackExtrasData {  get; set; }
        private List<TrackExtrasData> subTrackExtras { get; set; }//Extras corresponding to "Tracks_V3" data points
        private List<TrackExtrasData> standaloneMuonExtras { get; set; }
        private List<TrackExtrasData> globalMuonExtras { get; set; }
        private List<TrackExtrasData> trackerMuonExtras { get; set; }
        private List<TrackExtrasData> electronExtras { get; set; }
        private List<StandaloneMuonData> standaloneMuonDatas { get; set; }
        private List<GlobalMuonData> globalMuonDatas { get; set; }
        private List<TrackerMuonData> trackerMuonDatas { get; set; }
        private List<GsfElectron> electronData { get; set; }
        public List<string> filePaths { get; set; }

        //Constructor
        public IGTracks(JObject inputData, string name)
        {
            data = inputData;

            eventTitle = name;

            if (!Directory.Exists($"{desktopPath}\\{eventTitle}")) { 
                Directory.CreateDirectory($"{desktopPath}\\{eventTitle}");
                Directory.CreateDirectory($"{desktopPath}\\{eventTitle}\\3_standaloneMuons");
                Directory.CreateDirectory($"{desktopPath}\\{eventTitle}\\2_globalMuons");
                Directory.CreateDirectory($"{desktopPath}\\{eventTitle}\\1_trackerMuons");
                Directory.CreateDirectory($"{desktopPath}\\{eventTitle}\\4_gsfElectrons");
            }

            Execute();
            Serialize();
            SerializeMET();
        }

        //Main Class Method
        public void Execute()
        {
            var photonlist = photonParse();
            generatePhotonModels(photonlist);

            trackExtrasData = trackExtrasParse();
            globalMuonDatas = globalMuonParse();
            makeGlobalMuons();

            trackerMuonDatas = trackerMuonParse();   
            makeTrackerMuons();
            
            standaloneMuonDatas = standaloneMuonParse();
            makeStandaloneMuons();

            var tracklist = tracksParse();
            makeTracks();

            electronData = electronParse();
            makeElectrons();
        }

        //Methods
        public List<PhotonData> photonParse()
        {
            List<PhotonData> dataList = new List<PhotonData>();
            int idNumber = 0;

            foreach (var igPhotonData in data["Collections"]["Photons_V1"])
            {
                PhotonData currentPhotonItem = new PhotonData();

                var children = igPhotonData.Children().Values<double>().ToArray();

                currentPhotonItem.id = idNumber;
                currentPhotonItem.energy = children[0];
                currentPhotonItem.et = children[1];
                currentPhotonItem.eta = children[2];
                currentPhotonItem.phi = children[3];
                currentPhotonItem.position = new Vector3 ( (float)children[4], (float)children[5], (float)children[6] );

                idNumber++;
                dataList.Add(currentPhotonItem);
            }
            return dataList;
        }
        private string makePhoton(PhotonData inputData)
        {
            double lEB = 3.0; //half-length of ECAL barrel in meters
            double rEB = 1.24; //radius of ECAL barrel in meters
            double eta = inputData.eta;
            double phi = inputData.phi;
            double px = Math.Cos(phi);
            double py = Math.Sin(phi);
            double pz = Math.Sinh(eta);
            double x0 = inputData.position.X;
            double y0 = inputData.position.Y;
            double z0 = inputData.position.Z;
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
        public void generatePhotonModels(List<PhotonData> dataList)
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

            File.WriteAllText($"{desktopPath}\\{eventTitle}\\Photons_V1.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventTitle}\\Photons_V1.obj", dataStrings);
        }
        public List<string> trackCubicBezierCurve(List<TrackExtrasData> inputData,string objectName) {
            //Calculate the bezier path of the tracks based on the four pos control vectors defined in the TrackExtrasData struct
            List<string> dataList = new List<string>();
            List<string> testList = new List<string>();
            List<int> exclusion_indeces = new List<int>();
            int numVerts = 32;
            int n = 0;
            int num = 1;
            foreach (var item in inputData) 
            {
                testList.Clear();
                dataList.Add("o OBJECT");
                for (double i = 0.0; i <= numVerts; i++) {
                    
                    double t = (double)(i) / (double)(numVerts);
                    
                    double t1 = Math.Pow(1.0 - t, 3);
                    double t2 = 3 * t * Math.Pow(1.0 - t, 2);
                    double t3 = 3 * t * t * (1.0 - t);
                    double t4 = Math.Pow(t, 3);

                    // Check out the wikipedia page for bezier curves if you want to understand the math. That's where I learned it!
                    // also we're using double arrays because i dont like Vector3 and floats. I'm the one who has to go through the headaches of working with double arrays
                    // instead of Vector3 so i get to make that call. i also wrote this before i realized i couldn't avoid using MathNET and i can't be bothered to 
                    // change it such that it uses MathNET vectors

                    double[] term1 = { t1*item.pos1[0], t1 * item.pos1[1], t1 * item.pos1[2] };
                    double[] term2 = { t2 * item.pos3[0], t2 * item.pos3[1], t2 * item.pos3[2] };
                    double[] term3 = { t3 * item.pos4[0], t3 * item.pos4[1], t3 * item.pos4[2] };
                    double[] term4 = { t4 * item.pos2[0], t4 * item.pos2[1], t4 * item.pos2[2] };
                    double[] point = { term1[0] + term2[0] + term3[0] + term4[0], term1[1] + term2[1] + term3[1] + term4[1], term1[2] + term2[2] + term3[2] + term4[2] };
                                                           
                    string poin_t = $"v {point[0]} {point[1]} {point[2]}";
                    string point_t2 = $"v {point[0]} {point[1] + 0.001} {point[2]}";

                    dataList.Add(poin_t); dataList.Add(point_t2);
                    testList.Add(poin_t); testList.Add(point_t2);
                    n += 2;
                }
               for (int r=1; r<n-2; r++)
                {
                    string faces1 = $"f {r} {r + 1} {r + 3} {r + 2}";
                    string faces2 = $"f {r + 2} {r + 3} {r + 1} {r}";
                    testList.Add(faces1);testList.Add(faces2);
                }
                File.WriteAllLines(desktopPath + $"/{eventTitle}" + $"/{objectName}_{num}.obj", testList);
                num++;
                exclusion_indeces.Add(n);
            }
            
            for (int r = 1; r <= n - 2; r += 2) {
                if (exclusion_indeces.Contains(r+1)) 
                {
                    //Make sure the tracks don't loop back on each other
                    continue;
                }
                // Define faces
                string faces1 = $"f {r} {r + 1} {r + 3} {r + 2}";
                string faces2 = $"f {r+2} {r+3} {r+1} {r}";
                dataList.Add(faces1);
                dataList.Add(faces2);
            }
            
            return dataList;
            //filePaths.Add($"{desktopPath}\\{eventName}\\tracks.obj");
        }
        public List<string> singleFiletrackCubicBezierCurve(List<TrackExtrasData> inputData)
        {
            //Same as the above function, except it outputs all the tracks into their own singular file. This allows me to keep Tracks_V3 as its 
            //own single file since it doesn't need to be matched to data.
            List<string> dataList = new List<string>();
            List<string> testList = new List<string>();
            List<int> exclusion_indeces = new List<int>();
            int numVerts = 32;
            int n = 0;
            int num = 1;
            foreach (var item in inputData)
            {
                testList.Clear();
                dataList.Add("o OBJECT");
                for (double i = 0.0; i <= numVerts; i++)
                {

                    double t = (double)(i) / (double)(numVerts);

                    double t1 = Math.Pow(1.0 - t, 3);
                    double t2 = 3 * t * Math.Pow(1.0 - t, 2);
                    double t3 = 3 * t * t * (1.0 - t);
                    double t4 = Math.Pow(t, 3);

                    // Check out the wikipedia page for bezier curves if you want to understand the math. That's where I learned it!
                    // also we're using double arrays because i dont like Vector3 and floats. I'm the one who has to go through the headaches of working with double arrays
                    // instead of Vector3 so i get to make that call. i also wrote this before i realized i couldn't avoid using MathNET and i can't be bothered to 
                    // change it such that it uses MathNET vectors

                    double[] term1 = { t1 * item.pos1[0], t1 * item.pos1[1], t1 * item.pos1[2] };
                    double[] term2 = { t2 * item.pos3[0], t2 * item.pos3[1], t2 * item.pos3[2] };
                    double[] term3 = { t3 * item.pos4[0], t3 * item.pos4[1], t3 * item.pos4[2] };
                    double[] term4 = { t4 * item.pos2[0], t4 * item.pos2[1], t4 * item.pos2[2] };
                    double[] point = { term1[0] + term2[0] + term3[0] + term4[0], term1[1] + term2[1] + term3[1] + term4[1], term1[2] + term2[2] + term3[2] + term4[2] };

                    string poin_t = $"v {point[0]} {point[1]} {point[2]}";
                    string point_t2 = $"v {point[0]} {point[1] + 0.001} {point[2]}";

                    dataList.Add(poin_t); dataList.Add(point_t2);
                    testList.Add(poin_t); testList.Add(point_t2);
                    n += 2;
                }
                exclusion_indeces.Add(n);
            }

            for (int r = 1; r <= n - 2; r += 2)
            {
                if (exclusion_indeces.Contains(r + 1))
                {
                    //Make sure the tracks don't loop back on each other
                    continue;
                }
                // Define faces
                string faces1 = $"f {r} {r + 1} {r + 3} {r + 2}";
                string faces2 = $"f {r + 2} {r + 3} {r + 1} {r}";
                dataList.Add(faces1);
                dataList.Add(faces2);
            }

            return dataList;
            //filePaths.Add($"{desktopPath}\\{eventName}\\tracks.obj");
        }
        public List<TrackExtrasData> trackExtrasParse() {
            List<TrackExtrasData> dataList = new List<TrackExtrasData>();

            foreach (var igTrackExtra in data["Collections"]["Extras_V1"]) 
            {
                TrackExtrasData currentItem = new TrackExtrasData();

                var children = igTrackExtra.Children().Values<double>().ToArray();
                
                currentItem.pos1 = new double[3] { children[0], children[1], children[2] };
                
                double dir1mag = Math.Sqrt(  //dir1mag and dir2mag are for making sure the direction vectors are normalized
                    Math.Pow(children[3], 2) +
                    Math.Pow(children[4], 2) +
                    Math.Pow(children[5], 2)
                );
                currentItem.dir1 = new double[3] { children[3]/dir1mag, children[4]/dir1mag, children[5]/dir1mag };
                
                currentItem.pos2 = new double[3] { children[6], children[7], children[8] };
                
                double dir2mag = Math.Sqrt(
                    Math.Pow(children[9], 2) +
                    Math.Pow(children[10], 2) +
                    Math.Pow(children[11], 2)
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
        public List<GlobalMuonData> globalMuonParse()
        {
            List<GlobalMuonData> dataList = new List<GlobalMuonData> ();
            int idNumber = 0;

            var assocs = data["Associations"]["MuonGlobalPoints_V1"];

            if (assocs == null||assocs.HasValues == false)
            {
                Console.WriteLine("No Global Muons!");
                return dataList;
            }

            foreach (var item in data["Collections"]["GlobalMuons_V1"])
            {
                GlobalMuonData muonData = new GlobalMuonData();
                var children = item.Children().Values<double>().ToArray();

                muonData.id = idNumber;
                muonData.pt = children[0];
                muonData.charge = (int)children[1];
                muonData.position = new double[] { children[2], children[3], children[4] };
                muonData.phi = children[5];
                muonData.eta = children[6];
                muonData.caloEnergy = children[7];

                idNumber++;
                dataList.Add(muonData);
            }
            int firstassoc = assocs[0][1][1].Value<int>();
            globalMuonExtras = trackExtrasData.GetRange(firstassoc, assocs.Last()[1][1].Value<int>() - firstassoc + 1);

            return dataList;
        }
        public void makeGlobalMuons() 
        {
            DirectoryInfo dir = new DirectoryInfo($"{desktopPath}\\{eventTitle}\\2_globalMuons");
            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }
            if (globalMuonExtras == null) { return; }
            List<string> dataList = trackCubicBezierCurve(globalMuonExtras,"2_globalMuons\\2_globalMuons");
            File.WriteAllText($"{desktopPath}\\{eventTitle}\\2_globalMuons.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventTitle}\\2_globalMuons.obj", dataList);
        }
        public List<TrackerMuonData> trackerMuonParse()
        {
            List<TrackerMuonData> dataList = new List<TrackerMuonData>();
            int idNumber = 0;

            var assocs = data["Associations"]["MuonTrackerExtras_V1"];

            if (assocs == null || assocs.HasValues == false)
            {
                Console.WriteLine("No Tracker Muons!");
                return dataList;
            }

            foreach (var item in data["Collections"]["TrackerMuons_V1"])
            {
                TrackerMuonData muonData = new TrackerMuonData();
                var children = item.Children().Values<double>().ToArray();

                muonData.id = idNumber;
                muonData.pt = children[0];
                muonData.charge = (int)children[1];
                muonData.position = new double[] { children[2], children[3], children[4] };
                muonData.phi = children[5];
                muonData.eta = children[6];
                
                idNumber++;
                dataList.Add(muonData);
            }
            int firstassoc = assocs[0][1][1].Value<int>();
            trackerMuonExtras = trackExtrasData.GetRange(firstassoc, assocs.Last()[1][1].Value<int>() - firstassoc + 1);

            return dataList;
        }
        public void makeTrackerMuons() 
        {
            DirectoryInfo dir = new DirectoryInfo($"{desktopPath}\\{eventTitle}\\1_trackerMuons");
            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }
            if (trackerMuonExtras == null) { return; }
            List<string> dataList = trackCubicBezierCurve(trackerMuonExtras,"1_trackerMuons\\1_trackerMuons");
            File.WriteAllText($"{desktopPath}\\{eventTitle}\\1_trackerMuons.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventTitle}\\1_trackerMuons.obj", dataList);
        }
        public List<StandaloneMuonData> standaloneMuonParse()
        {
            List<StandaloneMuonData> dataList = new List<StandaloneMuonData>();
            int idNumber = 0;
            var assocs = data["Associations"]["MuonTrackExtras_V1"];
            if (assocs == null || assocs.HasValues == false)
            {
                Console.WriteLine("No Standalone Muons!");
                return dataList;
            }
            foreach (var item in data["Collections"]["Tracks_V3"])
            {
                StandaloneMuonData muon = new StandaloneMuonData();
                var children = item.Children().Values<double>().ToArray();
                muon.id = idNumber;
                muon.pt = children[0];
                muon.charge = (int)children[1];
                muon.position = new double[] { children[2], children[3], children[4] };
                muon.phi = children[5];
                muon.eta = children[6];
                muon.caloEnergy = children[7];

                idNumber++;
                dataList.Add(muon);
                
            }
            int firstassoc = assocs[0][1][1].Value<int>();
            standaloneMuonExtras = trackExtrasData.GetRange(firstassoc, assocs.Last()[1][1].Value<int>() - firstassoc + 1);

            return dataList;
        }
        public void makeStandaloneMuons() 
        {
            DirectoryInfo dir = new DirectoryInfo($"{desktopPath}\\{eventTitle}\\3_standaloneMuons");
            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }
            if (standaloneMuonExtras == null) { return; }
            List<string> dataList = trackCubicBezierCurve(standaloneMuonExtras,"3_standaloneMuons\\3_standaloneMuons");
            File.WriteAllText($"{desktopPath}\\{eventTitle}\\3_standaloneMuons.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventTitle}\\3_standaloneMuons.obj", dataList);
        }
        public List<Track> tracksParse()
        {
            List<Track> dataList = new List<Track>();
            var assocs = data["Associations"]["TrackExtras_V1"];

            if (assocs == null || assocs.HasValues == false)
            {
                return dataList;
            }
            foreach (var item in data["Collections"]["Tracks_V3"])
            {
                Track track = new Track();
                var children = item.Children().Values<double>().ToArray();

                track.pos = new double[] { children[0], children[1], children[2] };
                track.dir = new double[] { children[3], children[4], children[5] };
                track.pt = children[6];
                track.phi = children[7];
                track.eta = children[8];
                track.charge = (int)children[9];
                track.chi2 = children[10];
                track.ndof = children[11];
                dataList.Add(track);
            }
            int firstassoc = assocs[0][1][1].Value<int>();
            subTrackExtras = trackExtrasData.GetRange(firstassoc, assocs.Last()[1][1].Value<int>() - firstassoc + 1);

            return dataList;
        }
        public void makeTracks() 
        {
            if (subTrackExtras == null) { return; }
            List<string> dataList = singleFiletrackCubicBezierCurve(subTrackExtras);
            File.WriteAllText($"{desktopPath}\\{eventTitle}\\9_tracks.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventTitle}\\9_tracks.obj", dataList);
        }
        public List<GsfElectron> electronParse()
        {
            List<GsfElectron> dataList = new List<GsfElectron>();
            int idNumber = 0;

            var assocs = data["Associations"]["GsfElectronExtras_V1"];

            if (assocs == null || assocs.HasValues == false)
            {
                return dataList;
            }

            foreach (var item in data["Collections"]["GsfElectrons_V2"])
            {
                GsfElectron electron = new GsfElectron();
                var children = item.Children().Values<double>().ToArray();

                electron.id = idNumber;
                electron.pt = children[0];
                electron.eta = children[1];
                electron.phi = children[2];
                electron.charge = (int)children[3];
                electron.pos = new double[] { children[4], children[5], children[6] };
                electron.dir = new double[] { children[7], children[8], children[9] };
                
                idNumber++;
                dataList.Add(electron);
            }
            int firstassoc = assocs[0][1][1].Value<int>();
            electronExtras = trackExtrasData.GetRange(firstassoc, assocs.Last()[1][1].Value<int>()- firstassoc + 1);
                        
            return dataList;
        }
        public void makeElectrons() 
        {
            DirectoryInfo dir = new DirectoryInfo($"{desktopPath}\\{eventTitle}\\4_gsfElectrons");
            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }
            if (electronExtras == null) { return; }
            List<string> dataList = trackCubicBezierCurve(electronExtras, "4_gsfElectrons\\4_gsfElectrons");
            File.WriteAllText($"{desktopPath}\\{eventTitle}\\4_gsfElectrons.obj", String.Empty);
            File.WriteAllLines($"{desktopPath}\\{eventTitle}\\4_gsfElectrons.obj", dataList);
        }
        public METData METParse()
        {
            METData met = new METData();
            var metdata = data["Collections"]["PFMETs_V1"][0];
            var children = metdata.Values<double>().ToList();
            met.phi = children[0];
            met.pt = children[1];
            met.px = children[2];
            met.py = children[3];
            met.pz = children[4];

            return met;
        }
        public void SerializeMET()
        {
            string metdata = JsonConvert.SerializeObject(METParse(), Formatting.Indented);
            File.WriteAllText($@"{desktopPath}/{eventTitle}/METData.json", metdata);
        }
        public void Serialize()
        {
            string totaljson = JsonConvert.SerializeObject(new { globalMuonDatas, trackerMuonDatas, standaloneMuonDatas, electronData },Formatting.Indented);
            File.WriteAllText($@"{desktopPath}/{eventTitle}/totalData.json", totaljson);
        }
    }
}