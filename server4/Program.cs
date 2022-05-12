

using System;

using System.Net;

using System.Net.Sockets;
using System.Text;
using System.Threading;

using System.IO;
using System.Xml.Linq;
using System.Linq;
using CoreScanner;
using CoreScannerLib;



namespace SimpleSocketCS

{



    class ServerSocket

    {

        private static int pingpong = 0;

        private static string scanMessageX = null;



        public const int SIO_UDP_CONNRESET = -1744830452;





        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        static public void TheradFunkclientsocket(Socket sock)
        {
            while (true)
            {
                Thread.Sleep(250);
                pingpong++;
             if (!string.IsNullOrEmpty(scanMessageX))
                {
                    if (scanMessageX.Length < 6)
                    {
                        Console.WriteLine("Opening Job Card in Jeeves\n\r");

                        //keybd_event(48, 0, 0, 0);
                        //keybd_event(174, 0, 0, 0);

                        byte[] bytes = Encoding.ASCII.GetBytes(scanMessageX);
                        foreach (byte b in bytes)
                        {
                            keybd_event(b, 0, 0, 0);
                        }

                        keybd_event(13, 0, 0, 0);
                    }
                    else
                    {
                        Console.WriteLine("Sending scanner data to client\n\r");
                        sock.Send(Encoding.ASCII.GetBytes("ScanData: " + scanMessageX + " \n\r"));
                    }

                    scanMessageX = null;
                }
             if (pingpong >= 120)
                {
                    sock.Send(Encoding.ASCII.GetBytes("PING...\n\r"));
                    Console.WriteLine("PING...\n\r");
                    pingpong = 0;
                }


            }

        }



        static public void FormatBuffer(byte[] dataBuffer, string message)

        {

            byte[] byteMessage = System.Text.Encoding.ASCII.GetBytes(message);

            int index = 0;




            while (index < dataBuffer.Length)

            {

                for (int j = 0; j < byteMessage.Length; j++)

                {

                    dataBuffer[index] = byteMessage[j];

                    index++;




                    if (index >= dataBuffer.Length)

                    {

                        break;

                    }

                }

            }

        }









        static void Main(string[] args)

        {
            string data = null;

            string textMessage = "Server: ServerResponse";

            int localPort = 55555, sendCount = 10, bufferSize = 4096;

            IPAddress localAddress = IPAddress.Any; 

            SocketType sockType = SocketType.Stream;

            ProtocolType sockProtocol = ProtocolType.Tcp;



            Thread Scanthread = new Thread(() => Scannerthread(true));
            Scanthread.Start();




            Console.WriteLine();

      

            Console.WriteLine();




            for (int i = 0; i < args.Length; i++)

            {

                try

                {

                    if ((args[i][0] == '-') || (args[i][0] == '/'))

                    {

                        switch (Char.ToLower(args[i][1]))

                        {

                            case 'l':     

                                localAddress = IPAddress.Parse(args[++i]);

                                break;

                            case 'm':       

                                textMessage = args[++i];

                                break;

                            case 'n':       

                                sendCount = System.Convert.ToInt32(args[++i]);

                                break;

                            case 'p':     

                                localPort = System.Convert.ToInt32(args[++i]);

                                break;

                            case 't':     

                                i++;

                                if (String.Compare(args[i], "tcp", true) == 0)

                                {

                                    sockType = SocketType.Stream;

                                    sockProtocol = ProtocolType.Tcp;

                                }

                                else if (String.Compare(args[i], "udp", true) == 0)

                                {

                                    sockType = SocketType.Dgram;

                                    sockProtocol = ProtocolType.Udp;

                                }

                                else

                                {

                               

                                    return;

                                }

                                break;

                            case 'x':    

                                bufferSize = System.Convert.ToInt32(args[++i]);

                                break;

                            default:

                             

                                return;

                        }

                    }

                }

                catch

                {


                    return;

                }

            }



            Socket serverSocket = null;



            try

            {

                IPEndPoint localEndPoint = new IPEndPoint(localAddress, localPort), senderAddress = new IPEndPoint(localAddress, 0);

                Console.WriteLine("Server: IPEndPoint is OK...");

                EndPoint castSenderAddress;

                Socket clientSocket;

                byte[] receiveBuffer = new byte[bufferSize], sendBuffer = new byte[bufferSize];

                int rc;



                FormatBuffer(sendBuffer, textMessage);

                

                serverSocket = new Socket(localAddress.AddressFamily, sockType, sockProtocol);



                Console.WriteLine("Server: Socket() is OK...");


                serverSocket.Bind(localEndPoint);



                Console.WriteLine("Server: {0} server socket bound to {1}", sockProtocol.ToString(), localEndPoint.ToString());



                if (sockProtocol == ProtocolType.Tcp)

                {

                
                    serverSocket.Listen(1);

                    Console.WriteLine("Server: Listen() is OK, I'm listening for connection buddy!");

                }

                else

                {

                    byte[] byteTrue = new byte[4];












                    byteTrue[byteTrue.Length - 1] = 1;

                    serverSocket.IOControl(ServerSocket.SIO_UDP_CONNRESET, byteTrue, null);

                    Console.WriteLine("Server: IOControl() is OK...");

                }



     



                while (true)

                {

                    if (sockProtocol == ProtocolType.Tcp)

                    {


                        clientSocket = serverSocket.Accept();
                        Thread myNewThread = new Thread(() => TheradFunkclientsocket(clientSocket));
                        myNewThread.Start();
                        Console.WriteLine("Server: Accept() is OK...");

                        Console.WriteLine("Server: Accepted connection from: {0}", clientSocket.RemoteEndPoint.ToString());



                       

                        Console.WriteLine("Server: Preparing to receive using Receive()...");

                        while (true)

                        {


                            rc = clientSocket.Receive(receiveBuffer);
                            data += Encoding.ASCII.GetString(receiveBuffer, 0, rc);



                            if (data.IndexOf("\n") > -1)
                            {
                               
                                Console.WriteLine("Text received : {0}", data);

                                byte[] msg = Encoding.ASCII.GetBytes(data);
                   
                                data = null;

                            }



                   

                        }

                        

                        Console.WriteLine("Server: Preparing to send using Send()...");

                        for (int i = 0; i < sendCount; i++)

                        {

                            rc = clientSocket.Send(sendBuffer);

                            Console.WriteLine("Server: Sent {0} bytes", rc);

                        }

                        

                        clientSocket.Shutdown(SocketShutdown.Send);

                        Console.WriteLine("Server: Shutdown() is OK...");

                        clientSocket.Close();

                        Console.WriteLine("Server: Close() is OK...");

                    }

                    else

                    {

                        castSenderAddress = (EndPoint)senderAddress;

                        

                        rc = serverSocket.ReceiveFrom(receiveBuffer, ref castSenderAddress);

                        Console.WriteLine("Server: ReceiveFrom() is OK...");

                        senderAddress = (IPEndPoint)castSenderAddress;

                        Console.WriteLine("Server: Received {0} bytes from {1}", rc, senderAddress.ToString());


                        

                        for (int i = 0; i < sendCount; i++)

                        {

                            try

                            {

                                rc = serverSocket.SendTo(sendBuffer, senderAddress);

                                Console.WriteLine("Server: SendTo() is OK...");

                            }

                            catch

                            {

                         
                                continue;

                            }

                            Console.WriteLine("Server: Sent {0} bytes to {1}", rc, senderAddress.ToString());

                        }



                     

                        Console.WriteLine("Server: Preparing to send using SendTo(), on the way do sleeping, Sleep(250)...");

                        for (int i = 0; i < 3; i++)

                        {

                            serverSocket.SendTo(sendBuffer, 0, 0, SocketFlags.None, senderAddress);

                         

                            System.Threading.Thread.Sleep(550);

                        }

                    }

                }

            }

            catch (SocketException err)

            {

                Console.WriteLine("Server: Socket error occurred: {0}", err.Message);

            }

            finally

            {

           

                if (serverSocket != null)

                {

                    Console.WriteLine("Server: Closing using Close()...");
              
                    serverSocket.Close();
               

                }

            }

        }











        private static void CoreScannerObject_BarcodeEvent(short eventType, ref string barcodeData)
        {
        
            var document = XDocument.Load(new StringReader(barcodeData));
            int barcodeDataType = Int32.Parse(document.Descendants(XName.Get("datatype")).First().Value);
            string barcodeDataLabel = document.Descendants(XName.Get("datalabel")).First().Value;
           
            Console.WriteLine("Decode Data Label: " + barcodeDataLabel);
           
            byte[] data = FromHex(barcodeDataLabel);
            string s = Encoding.ASCII.GetString(data);
            Console.WriteLine(s);
            scanMessageX = s;
       //   scan
        }




        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("0x", "");
            hex = hex.Replace(" ", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }


     static   private bool Scannerthread(bool k)
        {
            CoreScanner.CCoreScanner coreScannerObject = new CCoreScanner();

            int appHandle = 0;
            const short NumberOfScannerTypes = 1;
            short[] scannerTypes = new short[NumberOfScannerTypes];
            scannerTypes[0] = (short)ScannerType.All; //  All scanner types
            int status = -1;



            try
            {
                
                coreScannerObject.Open(appHandle, 
                    scannerTypes,                   
                    NumberOfScannerTypes,        
                    out status);                 

                if (status == (int)Status.Success)
                {
                    Console.WriteLine("CoreScanner Open() - Success");

                    int eventIdCount = 1;
                    int[] eventIdList = new int[eventIdCount];
                   
                    eventIdList[0] = (int)EventType.Barcode;

                    string eventIds = String.Join(",", eventIdList);
                    string inXml = "<inArgs>" +
                                  "<cmdArgs>" +
                                  "<arg-int>" + eventIdCount + "</arg-int>" +  
                                   "<arg-int>" + eventIds + "</arg-int>" +       
                                   "</cmdArgs>" +
                                   "</inArgs>";

                    int opCode = (int)Opcode.RegisterForEvents;
                    string outXml = "";
                    status = -1;

                    // Call register for events
                    coreScannerObject.ExecCommand(opCode,      
                                                  ref inXml,   
                                                  out outXml,  
                                                  out status);  

                    if (status == (int)Status.Success)
                    {
                        Console.WriteLine("CoreScanner RegisterForEvents() - Success");
                    }
                    else
                    {
                        Console.WriteLine("CoreScanner RegisterForEvents() - Failed. Error Code : " + status);
                    }

                
                    coreScannerObject.BarcodeEvent += CoreScannerObject_BarcodeEvent;

                    Console.WriteLine("Scan a barcode now, press " +
                                      "any key to exit.");
                    while (!Console.KeyAvailable)
                    {
                        Thread.Sleep(10);
                    }

                    status = -1;
                    opCode = (int)Opcode.UnregisterForEvents;

                   
                    coreScannerObject.ExecCommand(opCode,      
                                                  ref inXml,   
                                                  out outXml,  
                                                  out status);  

                    if (status == (int)Status.Success)
                    {
                        Console.WriteLine("CoreScanner UnregisterForEvents() - Success");
                    }
                    else
                    {
                        Console.WriteLine("CoreScanner UnregisterForEvents() - Failed. Error Code : " + status);
                    }
                }
                else
                {
                    Console.WriteLine("CoreScanner Open() - Failed. Error Code : " + status);
                }

                
                coreScannerObject.Close(appHandle,  
                                        out status); 

                if (status == (int)Status.Success)
                {
                    Console.WriteLine("CoreScanner Close() - Success");
                }
                else
                {
                    Console.WriteLine("CoreScanner Close() - Failed. Error Code : " + status);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception : " + exception.ToString());
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();








            return true;

        }







    }

}