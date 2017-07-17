using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NodeNet.GUI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public ICommand StartServer { get; private set; }
        public ICommand StartClient { get; private set; }
        public ICommand SendMessage { get; private set; }
        public ICommand AskStatus { get; private set; }
        private ConnectionManager Manager { get; set; }


        private string sendmsg;

        public string SendMsg
        {
            get { return sendmsg; }
            set
            {
                sendmsg = value;
                RaisePropertyChanged(() => SendMsg);
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                Console.WriteLine("Message Raised");
                message = value;
                RaisePropertyChanged(() => Message);
            }
        }

        List<Tuple<string, string, string>> sockets;      
        public List<Tuple<string, string, string>> Sockets
        {
            get { return sockets; }
            set
            {
                sockets = value;
                RaisePropertyChanged(() => Sockets);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.Manager = new ConnectionManager(this);
            StartServer = new RelayCommand(StartServerAsync);
            StartClient = new RelayCommand(StartClientAsync);
            SendMessage = new RelayCommand(Sending);
            AskStatus = new RelayCommand(AskingStatus);
            this.Sockets = new List<Tuple<string, string, string>>();
        }

        private void StartClientAsync()
        {
            Manager.mode = ConnectionManager.Mode.Node;
            Manager.StartClient(getIp(), 8002);
        }
        private void StartServerAsync()
        {
            Manager.mode = ConnectionManager.Mode.Orchestrator;
            this.Manager.StartServerAsync(getIp(), 8002);
        }

        private void Sending()
        {
            DataInput input = new DataInput(DataInput.request.msg);
            input.msg = SendMsg;
            this.Manager.SendBroadcast(input);
        }

        private void AskingStatus()
        {
            DataInput input = new DataInput(DataInput.request.status);
            this.Manager.SendBroadcast(input);
        }

        private static IPAddress getIp()
        {
            IPAddress ipv4Address = Array.Find(
            Dns.GetHostEntry(string.Empty).AddressList,
            a => a.AddressFamily == AddressFamily.InterNetwork);
            return ipv4Address;
        }

    }
}