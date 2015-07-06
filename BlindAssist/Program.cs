using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Networking;
using Microsoft.SPOT;
using GTM = Gadgeteer.Modules;

namespace BlindAssist
{
    public partial class Program
    {
        private bool isDataFromServerReceived;
        const int rfidLength = 10;
        const string NETWORK_ID = "Ehsan :-)";
        const string NETWORK_PASSKEY = "EhsanAmir66!@";
        //SocketServer server;
        string[] requestedItems;
        string[] requestedItemsSearch;
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
                timer.Start(); 4D00556F06
            *******************************************************************************************/

            try
            {
                //server = new SocketServer(8080);
                // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
                Debug.Print("Program Started");
                wifiRS21.NetworkInterface.Open();
                wifiRS21.UseDHCP();
                Debug.Print("Program Started2");
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
            //showNetworkInformation();

            if (wifiRS21.IsNetworkConnected && wifiRS21.NetworkInterface.IPAddress != "0.0.0.0")
            {
                try
                {
                    StartServer();
                    if (server == null)
                        return;
                    
                    requestedItemsSearch = new string[requestedItems.Length];
                    //??
                    //It should not be created in here
                    //It should already have a list of RFID that user is looking for.
                    string readRfid = e;
                    for (int i = 0; i < requestedItems.Length; i++)
                    {
                        if (requestedItems[i] == readRfid)
                        {
                            server.SendBack(System.Text.Encoding.UTF8.GetBytes(readRfid));
                            Debug.Print("Final Result:" + requestedItems[i]);
                        }
                    }
                }
                catch
                {
                    Debug.Print("rfidReader_IdReceived method error \n" + e);
                }

                Debug.Print("rfid reads:" + e);
            }
        }

        SocketServer server;
        private void StartServer()
        {
            if (server != null)
                return;
            server = new SocketServer(8080);
            server.Start(wifiRS21.NetworkInterface.IPAddress);
            server.DataReceived += new DataReceivedEventHandler(server_DataReceived);
        }

        private void server_DataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                string receivedMessage = BytesToString(e.Data);
                setOrderedItems(receivedMessage);
                Debug.Print("Recieved message from the Client:" + receivedMessage);
                Debug.Print("The number of the ; in input string:" + requestedItems.Length);
                string response = "Response from server for the request '" + receivedMessage + "'";
                isDataFromServerReceived = true;
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

            int count=0;
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
