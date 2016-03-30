using System;
using static System.Console;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace FlashMyPi.Service
{
    public class SocketServer : IDisposable
    {
        private Thread listenerThread;

        public void Dispose()
        {
            // stop the socket server.
            listenerThread.Abort();
        }

        public void Start()
        {
            listenerThread = new Thread(AsynchronousSocketListener.StartListening);
            listenerThread.Start();
        }
    }

    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent connectionComplete = new ManualResetEvent(false);

        public static void StartListening()
        {
            try
            {
                var listener = ConnectSocket(11000);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    connectionComplete.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    connectionComplete.WaitOne();
                }

            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
            }

            WriteLine("\nPress ENTER to continue...");
            Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            connectionComplete.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            WriteLine($"Client connected. {handler.RemoteEndPoint.ToString()}");
            //openSockets.Add(handler);
            MessageBus.Subscribe(pattern => SendOnPattern(handler, pattern));

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            try {
                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read 
                    // more data.
                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // All the data has been read from the 
                        // client. Display it on the console.
                        WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    }
                    else
                    {
                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch(SocketException exception)
            {
                WriteLine($"read socket was closed by remote client: {exception.Message}");
            }
        }

        private static Random random = new Random();
        private static ConcurrentBag<Socket> openSockets = new ConcurrentBag<Socket>();
//        private static Timer timer = new Timer(x => 
//        {
//            foreach(var socket in openSockets)
//            {
//                SendOnTimer(socket);
//            }
//        }, null, 1000, 1000);

        private static void SendOnTimer(Socket handler)
        {
            if(!handler.Connected)
            {
                return;
            }
            try
            {
                var bytes = new byte[64];
                for(var i = 0; i < 64; i++)
                {
                    bytes[i] = (byte)random.Next(0,2);
                }

                handler.BeginSend(bytes, 0, bytes.Length, 0, x => 
                {
                    WriteLine("Sent, on timer message.");
                }, handler);
            }
            catch(Exception e)
            {
                WriteLine($"handler threw, so closing: {e.Message}");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }

        private static void SendOnPattern(Socket socket, Pattern pattern)
        {
            if(!socket.Connected)
            {
                return;
            }
            try
            {
                var x = 0;
                var bytes = new byte[64];
                for(var i = 0; i < 64; i++)
                {
                    bytes[i] = (byte)pattern.Pixels[x];
                    x++;
                    if(x >= pattern.Pixels.Length)
                    {
                        x = 0;
                    }
                }

                socket.BeginSend(bytes, 0, bytes.Length, 0, _ => 
                {
                    WriteLine("Sent on pattern subscription handler.");
                }, socket);
            }
            catch(Exception e)
            {
                WriteLine($"handler threw, so closing: {e.Message}");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            
        }

        private static void Send(Socket handler, string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
            }
        }

        public static Socket ConnectSocket(int port)
        {
            // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            // an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            var ipe = new IPEndPoint(IPAddress.Any, port);
            var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(ipe);
            socket.Listen(100);

            if (socket.IsBound)
            {
                WriteLine($"Waiting on {port}");
                return socket;
            }
            throw new ApplicationException($"Could not bind to {port}.");
        }
    }
}
