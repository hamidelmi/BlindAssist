using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Networking;
using Microsoft.SPOT;
using GTM = Gadgeteer.Modules;

namespace BlindAssist
{
    public partial class Program
    {
        const string NETWORK_ID = "Maryam";
        const string NETWORK_PASSKEY = "smileplz";
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            wifiRS21.NetworkInterface.Open();
            wifiRS21.UseDHCP();

            var results = wifiRS21.NetworkInterface.Scan(NETWORK_ID);
            if (results != null && results.Length > 0)
            {
                var netInfo = results[0];
                netInfo.Key = NETWORK_PASSKEY;
                wifiRS21.DebugPrintEnabled = true;
                //wifiRS21.NetworkSettings.EnableDhcp();
                wifiRS21.NetworkInterface.Join(netInfo);
                //wifiRS21.UseDHCP();
                //wifiRS21.NetworkSettings.RenewDhcpLease();
                wifiRS21.UseStaticIP("192.168.0.110", "255.255.255.0", "192.168.0.1");
            }
            else
            {
                Debug.Print("Unable to find the network");
            }

            rfidReader.IdReceived += rfidReader_IdReceived;
        }

        void wifiRS21_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            showNetworkInformation();
        }

        private void showNetworkInformation()
        {
            Debug.Print("net status:" + wifiRS21.IsNetworkConnected);
            Debug.Print("net status:" + wifiRS21.NetworkInterface.IPAddress);
        }

        void rfidReader_IdReceived(RFIDReader sender, string e)
        {
            showNetworkInformation();

            if (wifiRS21.IsNetworkConnected && wifiRS21.NetworkInterface.IPAddress != "0.0.0.0")
            {
                try
                {
                    SocketServer server = new SocketServer(8080);
                    server.DataReceived += new DataReceivedEventHandler(server_DataReceived);
                    server.Start(wifiRS21.NetworkInterface.IPAddress);

                    //Gadgeteer.Networking.GETContent.
                }
                catch
                {

                }
                //Gadgeteer.
            }

            Debug.Print("rfid reads:" + e);
        }

        private void server_DataReceived(object sender, DataReceivedEventArgs e)
        {
            string receivedMessage = BytesToString(e.Data);
            Debug.Print(receivedMessage);

            string response = "Response from server for the request '" + receivedMessage + "'";
            e.ResponseData = System.Text.Encoding.UTF8.GetBytes(response);

            if (receivedMessage == "close")
                e.Close = true;
        }

        private string BytesToString(byte[] bytes)
        {
            string str = string.Empty;
            for (int i = 0; i < bytes.Length; ++i)
                str += (char)bytes[i];

            return str;
        }

       

    }
}
