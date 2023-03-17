using Microsoft.VisualBasic;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IGtoOBJGen
{
    internal class Communicate
    {
        protected AdbServer server;
        protected AdbClient client;
        protected DeviceData oculusDevice;

        public Communicate(string adbPath) 
        {
            server = new AdbServer();
            var result = server.StartServer(adbPath,restartServerIfNewer:false);
            client = new AdbClient();
            List<DeviceData> devices = client.GetDevices();
            foreach(var device in devices)
            {
                if (device.Model == "Quest_2")
                {
                    oculusDevice = device;
                }
            }
        }
        public void UploadFiles(List<string> filePaths)
        {
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), oculusDevice)) 
            {
                Console.WriteLine("test1");
                foreach (var path in filePaths)
                {
                    string objName = path.Split('\\').Last();
                    Console.WriteLine("test2");
                    using (Stream stream = File.OpenRead(path))
                    {
                        Console.WriteLine($"{objName}");
                        service.Push(stream, $"/data/local/tmp/{objName}", 444, DateTime.Now, null, CancellationToken.None);
                        Console.WriteLine("test3");
                    }
                }
            };
            
        }
        public void DownloadFiles(string fileName)
        {
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), oculusDevice))
            using (Stream stream = File.OpenWrite(@"C:\Users\uclav\Desktop\yessir.obj"))
            {
                service.Pull($"/data/local/tmp/{fileName}", stream, null, CancellationToken.None);
            }
        }
    }
}
