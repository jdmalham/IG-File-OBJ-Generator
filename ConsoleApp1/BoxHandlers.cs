using Newtonsoft.Json.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace IGtoOBJGen
{
    internal class IGBoxes
    {
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private JObject data;
        private double EESCALE = 0.01;
        private double EBSCALE = 0.1;
        private double ESSCALE = 100;
        private double HBSCALE;
        private double HESCALE;
        private double HFSCALE;
        private double HOSCALE;
        public IGBoxes(JObject dataFile)
        {
            HBSCALE = 1.0;
            HESCALE = 1.0;
            HFSCALE = 1.0;
            HOSCALE = 1.0;
            data = dataFile;
            setScales();
        }
        public List<MuonChamberData> muonChamberParse()
        {
            var dataList = new List<MuonChamberData>();
            if (data["Collections"]["MuonChambers_V1"] != null)
            {
                foreach (var igChamberData in data["Collections"]["MuonChambers_V1"])
                {
                    MuonChamberData muonChamberData = new MuonChamberData();
                    var children = igChamberData.Children().Values<double>().ToArray();

                    muonChamberData.name = "MuonChambers_V1";
                    muonChamberData.detid = (int)children[0];
                    muonChamberData.front_1 = new double[] { children[1], children[2], children[3] };
                    muonChamberData.front_2 = new double[] { children[4], children[5], children[6] };
                    muonChamberData.front_3 = new double[] { children[7], children[8], children[9] };
                    muonChamberData.front_4 = new double[] { children[10], children[11], children[12] };
                    muonChamberData.back_1 = new double[] { children[13], children[14], children[15] };
                    muonChamberData.back_2 = new double[] { children[16], children[17], children[18] };
                    muonChamberData.back_3 = new double[] { children[19], children[20], children[21] };
                    muonChamberData.back_4 = new double[] { children[22], children[23], children[24] };

                    dataList.Add(muonChamberData);
                }
            }
            return dataList;
        }
        public void generateMuonChamberModels(List<MuonChamberData> data)
        {
            if (data.Count() == 0) { return; }

            List<string> dataStrings = new List<string>();
            int counter = 1;
            string name = data[0].name;

            foreach (var chamber in data)
            {
                dataStrings.Add($"o {name}");

                dataStrings.Add($"v {String.Join(' ', chamber.front_1)}");
                dataStrings.Add($"v {String.Join(' ', chamber.front_2)}");
                dataStrings.Add($"v {String.Join(' ', chamber.front_3)}");
                dataStrings.Add($"v {String.Join(' ', chamber.front_4)}");
                dataStrings.Add($"v {String.Join(' ', chamber.back_1)}");
                dataStrings.Add($"v {String.Join(' ', chamber.back_2)}");
                dataStrings.Add($"v {String.Join(' ', chamber.back_3)}");
                dataStrings.Add($"v {String.Join(' ', chamber.back_4)}");

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
        public List<List<CalorimetryData>> calorimetryParse( string name, List<List<CalorimetryData>> dataList)
        {
            List<CalorimetryData> mediatingList = new List<CalorimetryData>();
            foreach (var item in data["Collections"][name])
            {
                CalorimetryData ebHitsData = new CalorimetryData();
                var children = item.Children().Values<double>().ToArray();
                
                switch (name) 
                {
                    case "EERecHits_V2":
                        ebHitsData.scale = children[0] * EESCALE;
                        break;
                    case "ESRecHits_V2":
                        ebHitsData.scale = children[0] * ESSCALE;
                        break;
                    case "EBRecHits_V2":
                        ebHitsData.scale = children[0] * EBSCALE;
                        break;
                    case "HBRecHits_V2":
                        ebHitsData.scale = children[0] / HBSCALE;
                        break;
                    case "HERecHits_V2":
                        ebHitsData.scale = children[0] / HESCALE;
                        break;
                    case "HFRecHits_V2":
                        ebHitsData.scale = children[0] / HFSCALE;
                        break;
                    case "HORecHits_V2":
                        ebHitsData.scale = children[0] / HOSCALE;
                        break;
                    default:
                        ebHitsData.scale = 1.0;
                        break;
                }
                
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
        public List<JetData> jetParse()
        {
            List<JetData> datalist = new List<JetData>();
            foreach (var item in data["Collections"]["PFJets_V1"])
            {
                JetData currentJet = new JetData();
                var children = item.Children().Values<double>().ToArray();

                currentJet.et = children[0];
                currentJet.eta = children[1];
                currentJet.theta = children[2];
                currentJet.phi = children[3];

                datalist.Add(currentJet);
            }
            return datalist;
        }
        public void generateJetModels(List<JetData> data)
        {
            List<string> objData = new List<string>();
            int[] exclusionList = {};
            double maxZ = 2.25;
            double maxR = 1.10;
            double radius = 0.3 * (1.0 / (1 + 0.001));
            int numSections = 32;
            int iterNumber = 0;

            foreach (var item in data)
            {
                iterNumber++;
                double ct = Math.Cos(item.theta);
                double st = Math.Sin(item.theta);
                double cp = Math.Cos(item.phi);
                double sp = Math.Sin(item.phi);

                double length1 = (ct != 0.0) ? maxZ / Math.Abs(ct) : maxZ;
                double length2 = (st != 0.0) ? maxR / Math.Abs(st) : maxR;
                double length = length1 < length2 ? length1 : length2;

                var geometryData = jetGeometry(item,radius,length,numSections);
                objData.AddRange(geometryData);
                exclusionList.Append(2*numSections * iterNumber);
            }
            
            for (int i = 1; i <= 2*numSections*data.Count-numSections-1; i++) 
            {
                if (exclusionList.Contains(i-1))
                {
                    string thisface = $"f {i} {2 * i} {i + 1} {i - numSections + 1}";
                    objData.Add(thisface);
                    i += numSections;
                    continue;
                }

                string face = $"f {i} {i + numSections} {i+1 +numSections} {i + 1}";
                objData.Add(face);
            }

            File.WriteAllText($"{path}\\test_obj\\jets.obj", String.Empty);
            File.WriteAllLines($"{path}\\test_obj\\jets.obj", objData);
        }
        public List<string> jetGeometry(JetData item, double radius, double length, int sections)
        {
            List<string> bottomsection = new List<string>();
            List<string> topsection = new List<string>();

            var M = Matrix<double>.Build;

            double[,] xRot = 
                { { 1, 0, 0 }, 
                { 0, Math.Cos(item.theta), -1.0 * Math.Sin(item.theta) }, 
                { 0, Math.Sin(item.theta), Math.Cos(item.theta) } };
            
            double[,] zRot = 
                { { Math.Cos(item.phi+Math.PI/2.0), -1.0 * Math.Sin(item.phi+Math.PI/2.0), 0 }, 
                { Math.Sin(item.phi+Math.PI/2.0), Math.Cos(item.phi+Math.PI/2.0), 0 }, 
                { 0, 0, 1 } };
                       
            var rx = M.DenseOfArray(xRot); //Rotation matrices
            var rz = M.DenseOfArray(zRot);

            for (double i = 1.0; i <= sections; i++)
            {
                
                double radian = (2.0 * i * Math.PI) / (double)sections;

                string bottompoint = "v 0 0 0";
                bottomsection.Add(bottompoint);

                double[] feederArray = {radius*Math.Cos(radian), radius*Math.Sin(radian),length};
                Vector<double> temptop = Vector<double>.Build.DenseOfArray(feederArray);
                
                var rotation = rz*rx;
                var top = rotation * temptop;
                
                string toppoint = $"v {top[0]} {top[1]} {top[2]}";
                topsection.Add(toppoint);
            }

            bottomsection.AddRange(topsection);

            return bottomsection;
        }
        public List<string> generateCalorimetryBoxes(List<CalorimetryData> inputData)
        {
            List<string> geometryData = new List<string>();
            int counter = 1;

            var V = Vector<double>.Build;

            foreach (CalorimetryData box in inputData)
            {
                double scale = box.scale;
                
                var v0 = V.DenseOfArray(box.front_1);
                var v1 = V.DenseOfArray(box.front_2);
                var v2 = V.DenseOfArray(box.front_3);
                var v3 = V.DenseOfArray(box.front_4);
                var v4 = V.DenseOfArray(box.back_1);
                var v5 = V.DenseOfArray(box.back_2);
                var v6 = V.DenseOfArray(box.back_3);
                var v7 = V.DenseOfArray(box.back_4);

                var center = v0 + v1;
                center += v2;
                center += v3;
                center += v4;
                center += v5;
                center += v6;
                center += v7;
                center /= 8.0;
                
                v0 -= center;
                v0 *= scale;
                v0 += center;

                v1 -= center;
                v1*= scale;
                v1 += center;
                
                v2 -= center;
                v2 *= scale;
                v2 += center;

                v3 -= center;
                v3 *= scale;
                v3 += center;

                v4 -= center;
                v4 *= scale;
                v4 += center;

                v5 -= center;
                v5 *= scale;
                v5 += center;

                v6 -= center;
                v6 *= scale;
                v6 += center;

                v7 -= center;
                v7 *= scale;
                v7 += center;
                
                //geometryData.Add($"o {box.name}");
                geometryData.Add($"v {String.Join(' ', v0)}");
                geometryData.Add($"v {String.Join(' ', v1)}");
                geometryData.Add($"v {String.Join(' ', v2)}");
                geometryData.Add($"v {String.Join(' ', v3)}");
                geometryData.Add($"v {String.Join(' ', v4)}");
                geometryData.Add($"v {String.Join(' ', v5)}");
                geometryData.Add($"v {String.Join(' ', v6)}");
                geometryData.Add($"v {String.Join(' ', v7)}");

                geometryData.Add($"f {counter} {counter + 1} {counter + 2} {counter + 3}");
                geometryData.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                geometryData.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                geometryData.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
                geometryData.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
                geometryData.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                geometryData.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                geometryData.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
                geometryData.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
                geometryData.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
                geometryData.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
                geometryData.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");

                counter += 8;
            }
            return geometryData;
        }
        public List<string> generateCalorimetryTowers(List<CalorimetryData> inputData)
        {
            List<string> geometryData = new List<string>();
            int counter = 1;

            var V = Vector<double>.Build;

            foreach (CalorimetryData box in inputData)
            {
                var v0 = V.DenseOfArray(box.front_1);
                var v1 = V.DenseOfArray(box.front_2);
                var v2 = V.DenseOfArray(box.front_3);
                var v3 = V.DenseOfArray(box.front_4);
                var v4 = V.DenseOfArray(box.back_1);
                var v5 = V.DenseOfArray(box.back_2);
                var v6 = V.DenseOfArray(box.back_3);
                var v7 = V.DenseOfArray(box.back_4);

                v4 -= v0;
                v5 -= v1;
                v6 -= v2;
                v7 -= v3;

                double v4mag = v4.L2Norm();
                double v5mag = v5.L2Norm();
                double v6mag = v6.L2Norm();
                double v7mag = v7.L2Norm();

                v4 /= v4mag;
                v5 /= v5mag;
                v6 /= v6mag;
                v7 /= v7mag;

                v4 *= box.scale;
                v5 *= box.scale;
                v6 *= box.scale;
                v7 *= box.scale;

                v4 += v0;
                v5 += v1;
                v6 += v2;
                v7 += v3;

                geometryData.Add($"o {box.name}");
                geometryData.Add($"v {String.Join(' ', v0)}");
                geometryData.Add($"v {String.Join(' ', v1)}");
                geometryData.Add($"v {String.Join(' ', v2)}");
                geometryData.Add($"v {String.Join(' ', v3)}");
                geometryData.Add($"v {String.Join(' ', v4)}");
                geometryData.Add($"v {String.Join(' ', v5)}");
                geometryData.Add($"v {String.Join(' ', v6)}");
                geometryData.Add($"v {String.Join(' ', v7)}");

                geometryData.Add($"f {counter} {counter + 1} {counter + 2} {counter + 3}");
                geometryData.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                geometryData.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                geometryData.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
                geometryData.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
                geometryData.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                geometryData.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                geometryData.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
                geometryData.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
                geometryData.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
                geometryData.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
                geometryData.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");

                counter += 8;
            }
            return geometryData;
        } 
        public void setScales()
        {
            List<string> HCALSETS = new List<string>() { "HERecHits_V2","HBRecHits_V2","HFRecHits_V2","HORecHits_V2" };
            foreach (string HCALSET in HCALSETS)
            {
                var collection = data["Collections"][HCALSET];
                
                if ( collection.HasValues == false)
                {
                    continue;
                }
                
                List<double> energies_ = new List<double>();

                foreach (var item in collection)
                {
                    //Console.WriteLine((double)item[0].Value<double>());
                    energies_.Add((double)item[0].Value<double>());
                }

                double [] energies = energies_.ToArray();
                double scaleEnergy = energies.Max();
                
                switch(HCALSET)
                {
                    case "HERecHits_V2":
                        HESCALE = scaleEnergy; 
                        Console.WriteLine($"HESCALE: {HESCALE}");
                        break;
                    case "HBRecHits_V2":
                        HBSCALE = scaleEnergy;
                        Console.WriteLine($"HBSCALE: {HBSCALE}");
                        break;
                    case "HFRecHits_V2":
                        HFSCALE = scaleEnergy;
                        Console.WriteLine($"HFSCALE: {HFSCALE}");
                        break;
                    case "HORecHits_V2":
                        HOSCALE = scaleEnergy;
                        Console.WriteLine($"HOSCALE: {HOSCALE}");
                        break;
                }
            }
        }
    }
}