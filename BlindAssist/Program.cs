using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Hardware;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using System.Text;
using System.IO;

namespace BlindAssist
{
    public partial class Program
    {
        const string NETWORK_ID = "TUDWeb";
        const string NETWORK_PASSKEY = "";
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
            if (results != null && results.Length > 1)
            {
                var netInfo = results[0];
                netInfo.Key = "";
                wifiRS21.NetworkInterface.Join(netInfo);
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

            if (wifiRS21.IsNetworkConnected)
            {
                SocketServer server = new SocketServer(8080);
                server.DataReceived += new DataReceivedEventHandler(server_DataReceived);
                server.Start();
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
