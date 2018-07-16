using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
namespace LandmarkServer
{
    class PositionClient
    {
        public PositionClient(Socket client,Action<int> onSocketClose,LocationDetector _detector)
        {
            detector = _detector;
            clientID = clientIDCount++;
            onClientClose = onSocketClose;
            socketClient = client;
            sendArgs = new SocketAsyncEventArgs();
            //sendArgs.SetBuffer(new byte[1024], 0, 1024);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>((obj,args)=>
            {
                onSend(args);
            });
            recvArgs = new SocketAsyncEventArgs();
            recvArgs.SetBuffer(new byte[1024],0,1024);
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>((obj, args) =>
            {
                OnReadBytes(args);

            });
        }

        private LocationDetector detector;
        Action<int> onClientClose;
        public int GetID()
        {
            return clientID;
        }
        public void StartReceive()
        {
            socketClient.ReceiveAsync(recvArgs);
        }
        static int clientIDCount = 0;
        int clientID = 0;
        List<byte[]> bufferData = new List<byte[]>();
        bool bSending = false;
        Socket socketClient;
        SocketAsyncEventArgs sendArgs;
        SocketAsyncEventArgs recvArgs;
        int currentSend = 0;

        void OnCmd(int cmdID)
        {
            if (cmdID == 27)
            {
                Close();
                return;
            }
            if (cmdID == 1)
            {
                byte[] data = detector.GetLocationData();
                Send(data);
                return;
            }
            if (cmdID == 2)
            {
                byte[] data = detector.GetGlobalLocationData();
                Send(data);
                return;
            }
            System.Console.WriteLine("client {0} receive command {1} !",clientID, cmdID);
            String formatText = String.Format("{0} ({1},{2})", cmdID, cmdID * .1f, cmdID * 0.2f);
            Send(UTF8Encoding.UTF8.GetBytes(formatText));
        } 
        void Close()
        {
            socketClient.Shutdown(SocketShutdown.Both);
            //socketClient.Close();
        }
         void OnReadBytes(SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                for (int i = 0; i < args.BytesTransferred; ++i)
                {
                    OnCmd((int)args.Buffer[i]);
                }
          //      args.SetBuffer(0, BufferLength);
                
                bool will = socketClient.ReceiveAsync(args);
                if (!will)
                {
                    OnReadBytes( args);
                }
            }
            else
            {
                System.Console.WriteLine("client closed");
                if (onClientClose != null)
                {
                    onClientClose(clientID);
                }
            }
        }
        public void Send(byte[] data)
        {
            
            if (!bSending)
            {
                sendArgs.SetBuffer(data, 0, data.Length);
                sendAsyncArgs(sendArgs);
            }
            else
            {
                bufferData.Add(data);
            }
        }
        void onSend(SocketAsyncEventArgs args)
        {
            int bufflen = args.Buffer.Length;
            currentSend += bufflen;
            
            if (currentSend == bufflen)
            {
                // send complete!
                // get next buffer
                currentSend = 0;
                if (bufferData.Count > 0)
                {
                    byte[] data = bufferData[0];
                    bufferData.RemoveAt(0);
                    args.SetBuffer(data, 0, data.Length);
                }
                else
                {
                    bSending = false;
                }
            }
            else if (currentSend < bufflen)
            {
                args.SetBuffer(currentSend, bufflen - currentSend);
            }
            if (bSending)
            {
                bool bAsync = socketClient.SendAsync(args);
                if (!bAsync)
                {
                    onSend(args);
                }
            }

        }
        void sendAsyncArgs(SocketAsyncEventArgs args)
        {
            if (args == null)
            {
                args = new SocketAsyncEventArgs();
            }
            bool basync = socketClient.SendAsync(args);
            if (!basync)
            {
                onSend(args);
            }

        }
    }
    class PositionServer
    {

        Socket listenSocket;
        SocketAsyncEventArgs acceptEvents;
        Dictionary<int, PositionClient> dictId2Client = new Dictionary<int, PositionClient>();
        private LocationDetector locationDetector = new LocationDetector();
        public PositionServer()
        {
            acceptEvents = new SocketAsyncEventArgs();
            acceptEvents.Completed += new EventHandler<SocketAsyncEventArgs>((obj, args) =>
            {
                OnNewAccept(args);
            });
        }
        void OnNewAccept(SocketAsyncEventArgs args)
        {
            if (args.AcceptSocket != null && args.SocketError == SocketError.Success)
            {
                // receive a new socket
                OnNewClient(args.AcceptSocket);
            }
            args.AcceptSocket = null;
            listenSocket.AcceptAsync(args);
        }
        void OnCmd(int cmdID)
        {
            System.Console.WriteLine("You receve command {0} !", cmdID);
       
            

            
        }
        int BufferLength = 1024;
        SocketAsyncEventArgs newReadWriteArgs()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            byte[] buffer = new byte[BufferLength];
            args.SetBuffer(buffer, 0, buffer.Length);
            return args;
        }
        void OnReadBytes(Socket newSocket, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                for (int i = 0; i < args.BytesTransferred; ++i)
                {
                    OnCmd((int)args.Buffer[i]);
                }
          //      args.SetBuffer(0, BufferLength);
                bool will = newSocket.ReceiveAsync(args);
                if (!will)
                {
                    OnReadBytes(newSocket, args);
                }
            }
            else
            {
                System.Console.WriteLine("client closed");
            }
        }
        void OnCloseClient(int clientid)
        {
            dictId2Client.Remove(clientid);
            System.Console.WriteLine("manager: client {0} closed, remain {1}", clientid,dictId2Client.Count);
        }
        void OnNewClient(Socket newSocket)
        {
            /* SocketAsyncEventArgs receiveArg = newReadWriteArgs();
             receiveArg.Completed += new EventHandler<SocketAsyncEventArgs>((obj, args) =>
             {
                 OnReadBytes(newSocket, args);

             });
             bool will = newSocket.ReceiveAsync(receiveArg);
             if(!will)
             {
                 OnReadBytes(newSocket, receiveArg);
             }*/
            PositionClient client = new PositionClient(newSocket,OnCloseClient,locationDetector);
            client.StartReceive();
            dictId2Client[client.GetID()] = client;
            System.Console.WriteLine("client {0} connected now clients {1}", client.GetID(), dictId2Client.Count);

        }
        void Write(Socket socket, byte[] buffer)
        {
            SocketAsyncEventArgs writeArg = newReadWriteArgs();
            writeArg.Completed += new EventHandler<SocketAsyncEventArgs>((obj, args) =>
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {

                }
            });
        }
        void Listen()
        {

        }
        public void StartServer(int port)
        {
            listenSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint epLocal = new IPEndPoint(IPAddress.Any, port);
            listenSocket.Bind(epLocal);
            listenSocket.Listen(10);
            bool bWill = listenSocket.AcceptAsync(acceptEvents);
            if (!bWill)
            {
                OnNewAccept(acceptEvents);
            }
        }
    }
}
