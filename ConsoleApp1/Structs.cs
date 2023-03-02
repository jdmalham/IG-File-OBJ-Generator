using System.Numerics;
struct MuonChamberData {
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
struct CalorimetryData {
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
struct METData {
    public double phi;
    public double pt;
    public double px;
    public double py;
    public double pz;
}
struct MuonData {
    public string name;
    public double pt;
    public int charge;
    public Vector3 position;
    public double phi;
    public double eta;
    public double caloEnergy;
}
struct PhotonData {
    public string name;
    public double energy;
    public double et;
    public double eta;
    public double phi;
    public Vector3 position;
}
struct TrackExtrasData {
    public Vector3 pos1;
    public Vector3 dir1;
    public Vector3 pos2;
    public Vector3 dir2;
    public Vector3 pos3;
    public Vector3 pos4;
}