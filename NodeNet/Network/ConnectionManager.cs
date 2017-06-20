using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NodeNet.Network
{

    public class ConnectionManager : INotifyPropertyChanged
    {

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        private Protocol protocol = new Protocol();

        private List<Socket> sockets = new List<Socket>();
        private Socket socket { get; set; }
        private string msg;
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private void RaisePropertyChanged(String property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public String message
        {
            get { return msg; }
            set { msg = value; RaisePropertyChanged("message"); Console.WriteLine("PropertyChange Raised"); }
        }

        #region Ctor
        public ConnectionManager()
        {
        }
        #endregion

        #region Méthodes client/serveur

        public async Task StartServerAsync(IPAddress ip, int port)
        {
            TcpListener listener = new TcpListener(ip, port);
            listener.Start();

            while (true)
            {
                this.sockets.Add(await listener.AcceptSocketAsync());
                Console.WriteLine(String.Format("Client Connection accepted from {0}", sockets.Last().RemoteEndPoint.ToString()));
                Console.WriteLine(sockets.Count().ToString());
            }
        }

        public async void StartClient(IPAddress ip, int port)
        {
            IPEndPoint remoteEP = new IPEndPoint(ip, port);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), socket);

                connectDone.WaitOne();

                Receive(socket);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion

        private void TestCo()
        {

        }

        #region Méthodes envoi/réception
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = this.protocol.encapsulate(Encoding.ASCII.GetBytes(data), Protocol.code.sendData);

            foreach (Socket socket in sockets)
            {
                try
                {
                    socket.BeginSend(byteData, 0, byteData.Length, 0,
                        new AsyncCallback(SendCallback), socket);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        #endregion

        #region Méthodes Callback
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {

                    NetworkStream stream = new NetworkStream(client);

                    var data = new List<byte>();
                    while(stream.DataAvailable)
                        data.Add((byte)stream.ReadByte());

                    byte[] array = data
                                    .Select(a => a)
                                    .ToArray();

                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(array));


                    //// There might be more data, so store the data received so far.
                    //state.sb.Append(System.Text.Encoding.Default.GetString(state.buffer, 0, bytesRead));

                    //// Get the rest of the data.
                    //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    //    new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    receiveDone.Set();
                }
                this.message = state.sb.ToString();
                Console.WriteLine(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine(String.Format("Connection accepted to {0} ", socket.RemoteEndPoint.ToString()));

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion



    }
}
