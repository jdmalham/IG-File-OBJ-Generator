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
    public double[] pos1;
    public double[] dir1;
    public double[] pos2;
    public double[] dir2;
    public double[] pos3;
    public double[] pos4;
}