using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Networking;
using Microsoft.SPOT;
using System;
using GTM = Gadgeteer.Modules;

namespace BlindAssist
{
    /// <summary>
    /// This is the main class of the aplication
    /// The purpose of the aplication is to help the Blind people to buy stuffs from grocery stores.
    /// The appliction has tow main parts, One of them is the Handheld Mobile phone, by which user sends the requested items to the server 
    /// and the next one is the gadgeteer part which is responsible to searh(Listen) the RFDID tags requested by the user.
    /// Both parties are connected to the same router (Domain).
    /// The application uses the static Ip assignment.
    /// IP:"192.168.0.110", SM:"255.255.255.0", DG:"192.168.0.1" the default setting
    /// Please set the above properties based on your local network.
    /// Once the read RFID is equal to what user has requested an appropriate feedback is sent to the user Moblie phone.
    /// It also includes one server and client socket class in order to receive and send data from/to the user.
    /// </summary>
    public partial class Program
    {
        #region The variables definition
        /// <summary>
        /// The server IP to be connected from the aother app
        /// </summary>
        const string DEFAULT_SERVER_IP = "192.168.0.120";
        
        /// <summary>
        /// The default subnet mask of the server subnet
        /// </summary>
        const string DEFAULT_SUBNET_MASK = "255.255.255.0";

        /// <summary>
        /// The default gateway of the server subnet
        /// </summary>
        const string DEFAULT_GATEWAY= "192.168.0.1";

        /// <summary>
        /// The port NO. of the Gadgeteer-Side server to be used by the ServerSocket
        /// </summary>
        const int DEFAULT_SERVER_PORT = 8080;
        /// <summary>
        /// The port NO. of the Gadgeteer-Side server to be used by the ServerSocket
        /// </summary>
        const int DEFAULT_CLIENT_PORT = DEFAULT_SERVER_PORT + 1;
        /// <summary>
        /// The standard length of the RFID tag
        /// </summary>
        const int rfidLength = 10;
        /// <summary>
        /// Router SSID to be connected to
        /// </summary>
        const string NETWORK_ID = "";
        /// <summary>
        /// Router Passkey to be connected to
        /// </summary>
        const string NETWORK_PASSKEY = "";
        /// <summary>
        /// This holds the received ordered items form the client
        /// Once a RFID tag is read by the reader, it is checked by the content of this array and if they are same the appropriate feedback is sent to the client
        /// </summary>
        string[] requestedItems;
        /// <summary>
        /// ClientSocket
        /// </summary>
        SocketClient client;
        /// <summary>
        /// ServerSocket
        /// </summary>
        SocketServer server; 
        #endregion

        #region Main Method
        // This method is run when the mainboard is powered up or reset.(Main Method)   
        void ProgramStarted()
        {
            try
            {
                if (NETWORK_ID!= "")
                {
                    Debug.Print("Program Started...");
                    wifiRS21.NetworkInterface.Open();
                    wifiRS21.UseDHCP();
                    var results = wifiRS21.NetworkInterface.Scan(NETWORK_ID);
                    if (results != null && results.Length > 0)
                    {
                        var netInfo = results[0];
                        netInfo.Key = NETWORK_PASSKEY;
                        wifiRS21.DebugPrintEnabled = true;
                        wifiRS21.NetworkInterface.Join(netInfo);
                        wifiRS21.UseStaticIP(DEFAULT_SERVER_IP, DEFAULT_SUBNET_MASK, DEFAULT_GATEWAY);
                        showNetworkInformation();
                        wifiRS21.NetworkUp += wifiRS21_NetworkUp;
                    }
                    else
                    {
                        Debug.Print("Unable to find the network");
                    }

                    rfidReader.IdReceived += rfidReader_IdReceived; 
                }
                else
                {
                    Debug.Print("Please specify the Network SSID and Password in the given fields in Program.cs class!");
                }
            }
            catch (System.Exception e)
            {
                Debug.Print(e.Message);

            }
        } 
        #endregion

        #region Event Methods 
        #region wifiRS21_NetworkUp() implementation
        /// <summary>
        /// Show Network Information on the application startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="state"></param>
        void wifiRS21_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            showNetworkInformation();
            StartServer();
        }
        #endregion

        #region showNetworkInformation() implementation
        /// <summary>
        /// Show Network details
        /// </summary>
        private void showNetworkInformation()
        {
            Debug.Print("net status:" + wifiRS21.IsNetworkConnected);
            Debug.Print("net status:" + wifiRS21.NetworkInterface.IPAddress);
        }
        #endregion

        #region rfidReader_IdReceived() Implementation
        /// <summary>
        /// This method is called whenver a new RFID Tag is Read
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Received RFID tag</param>
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
                            Debug.Print("Final Sent Result:" + readRfid);
                        }
                    }
                }
                catch
                {
                    Debug.Print("rfidReader_IdReceived method error \n" + e);
                }

            }
        }
        #endregion

        #region server_RemoteIPChanged() Implementation
        /// <summary>
        /// Renew the Socket Client Based on the new connected devices specifications
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void server_RemoteIPChanged(object sender, EventArgs e)
        {
            //Initialize the client object right after being connected to a smartphone.
            //(The opened port by the Client as a receiving server) 8081
            client = new SocketClient(server.RemoteIP, DEFAULT_CLIENT_PORT);
        } 
        #endregion

        #region server_DataReceived() implementation
        /// <summary>
        /// Once the data is received in the server side, This method is invoked.
        /// The received data needs to be reformed and then be set into a String array
        /// in order to be checked by the RFID reader function
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Received event</param>
        private void server_DataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                string receivedMessage = BytesToString(e.Data);
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
        #endregion
        #endregion

        #region Processing and controlling methods

        /// <summary>
        /// Starts the server to be ready to be connected by other parties
        /// </summary>
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

        /// <summary>
        /// This function is responsible to set the ordered Items into a Public string[] variable in order to be
        /// checked with whenever a new RFID tag entry is read by the RFID reade
        /// </summary>
        /// <param name="receivedMessage">New sequence of requested item from the client 
        /// The format shoul be like RFID;RFID;RFID;...
        /// Each RFID tag has a length of 10 Digits</param>
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

            Debug.Print("Requested Items after being processed in the server side are:");
            for (int i = 0; i < requestedItems.Length; i++)
            {
                Debug.Print("\n" + (i + 1).ToString() + requestedItems[i]);
            }

        }

        /// <summary>
        /// Counts the number of the given RFIDs from the client
        /// </summary>
        /// <param name="receiveString">The received string of ordered products from the client</param>
        /// <returns>Number of the ordered items</returns>
        private int orderedRfidCount(string receiveString)
        {
            int count = 0;
            for (int i = 0; i < receiveString.Length; i++)
            {
                char k = receiveString[i];
                if (k == ';')
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Reforms a byte array to string 
        /// </summary>
        /// <param name="bytes">Bytes array to be reformed</param>
        /// <returns>String equivalent to the received bytes array</returns>
        private string BytesToString(byte[] bytes)
        {
            string str = string.Empty;
            for (int i = 0; i < bytes.Length; ++i)
                str += (char)bytes[i];

            return str;
        } 
        #endregion

    }
}
