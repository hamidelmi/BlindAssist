using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Networking;
using Microsoft.SPOT;
using System;
using GTM = Gadgeteer.Modules;

namespace BlindAssist
{
    public partial class Program
    {
        const int DEFAULT_SERVER_PORT = 8080;
        const int DEFAULT_CLIENT_PORT = DEFAULT_SERVER_PORT + 1;
        const int rfidLength = 10;
        const string NETWORK_ID = "Ehsan :-)";
        const string NETWORK_PASSKEY = "EhsanAmir66!@";
        string[] requestedItems;

        SocketClient client;
        SocketServer server;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
                timer.Start(); 4D00556F06
            *******************************************************************************************/

            try
            {
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
                    showNetworkInformation();

                    wifiRS21.NetworkUp+=wifiRS21_NetworkUp;
                }
                else
                {
                    Debug.Print("Unable to find the network");
                }

                rfidReader.IdReceived += rfidReader_IdReceived;
            }
            catch (System.Exception e)
            {
                Debug.Print(e.Message);

            }
        }

        /// <summary>
        /// Show Network Information on the start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="state"></param>
        void wifiRS21_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            showNetworkInformation();
            StartServer();
        }

        /// <summary>
        /// Show Network details
        /// </summary>
        private void showNetworkInformation()
        {
            Debug.Print("net status:" + wifiRS21.IsNetworkConnected);
            Debug.Print("net status:" + wifiRS21.NetworkInterface.IPAddress);
        }

        void rfidReader_IdReceived(RFIDReader sender, string e)
        {
            Debug.Print("rfid reads:" + e);
            if (wifiRS21.IsNetworkConnected && wifiRS21.NetworkInterface.IPAddress != "0.0.0.0")
            {
                try
                {
                    if (requestedItems == null)
                        return;

                    string readRfid = e;
                    for (int i = 0; i < requestedItems.Length; i++)
                    {
                        if (requestedItems[i] == readRfid)
                        {
                            client.Send(readRfid);
                            Debug.Print("Final Result:" + readRfid);
                        }
                    }
                }
                catch
                {
                    Debug.Print("rfidReader_IdReceived method error \n" + e);
                }

            }
        }
        
        private void StartServer()
        {
            if (server != null)
                return;

            server = new SocketServer(DEFAULT_SERVER_PORT);
            server.Start(wifiRS21.NetworkInterface.IPAddress);
            server.DataReceived += new DataReceivedEventHandler(server_DataReceived);
            server.RemoteIPChanged += server_RemoteIPChanged;
            Debug.Print("Start server...");
        }

        void server_RemoteIPChanged(object sender, EventArgs e)
        {
            client = new SocketClient(server.RemoteIP, DEFAULT_CLIENT_PORT);
        }

        private void server_DataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                string receivedMessage = BytesToString(e.Data);
                //Check commands: 
                //start: rfid;rfid;rfid;rfid
                //find: rfid
                //find: rfid
                //finish
                //recieved rfid=>nothing

                setOrderedItems(receivedMessage);
                Debug.Print("Recieved message from the Client:" + receivedMessage);
                Debug.Print("The number of the ; in input string:" + requestedItems.Length);
                string response = "Response from server for the request '" + receivedMessage + "'";
                e.ResponseData = System.Text.Encoding.UTF8.GetBytes(response);

                if (receivedMessage == "close")
                    e.Close = true;
            }
            catch (System.Exception ee)
            {
                Debug.Print(ee.Message);
                throw;
            }
        }

        private void setOrderedItems(string receivedMessage)
        {
            int rfidCount = orderedRfidCount(receivedMessage);
            requestedItems = new string[rfidCount];
            int x = 0;
            for (int i = 0; i < rfidCount; i++)
            {
                requestedItems[i] = receivedMessage.Substring(x, rfidLength);
                x += rfidLength + 1;
            }

            for (int i = 0; i < requestedItems.Length; i++)
            {
                Debug.Print(requestedItems[i]);
            }

        }
        private int orderedRfidCount(string recieiveString)
        {

            int count = 0;
            //count = recieiveString.Split(';').Length;
            for (int i = 0; i < recieiveString.Length; i++)
            {
                char k = recieiveString[i];
                if (k == ';')
                {
                    count++;
                }
            }
            return count;
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
