using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.Network;
using System;
using System.Net;
using System.Net.NetworkInformation;
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
        public ICommand Send { get; private set; }
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

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.Manager = new ConnectionManager(this);
            StartServer = new RelayCommand(StartServerAsync);
            StartClient = new RelayCommand(StartClientAsync);
            Send = new RelayCommand(Sending);
        }

        private void StartClientAsync()
        {
            Manager.StartClient(getIp(), 8001);
        }
        private async void StartServerAsync()
        {
            await this.Manager.StartServerAsync(getIp(), 8001);
        }

        private void Sending()
        {
            DataInput<String, String> input = new DataInput<string, string>(SendMsg);
            this.Manager.Send(input);
        }

        private static IPAddress getIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    Console.WriteLine(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ip.Address;
                        }
                    }
                }
            }
            throw new Exception("IP NOT FOUND");
        }


    }
}