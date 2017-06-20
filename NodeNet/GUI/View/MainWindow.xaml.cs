using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NodeNet.Network;

namespace NodeNet.View
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region properties
        private enum Mode { client, serveur };
        private Mode value = new Mode();

        private ConnectionManager Manager { get; set; }
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            this.Manager = new ConnectionManager();
            this.DataContext = Manager;
        }

        #region private methods
        private async Task change_ViewAsync()
        {
            srvBtn.Visibility = Visibility.Hidden;
            cliBtn.Visibility = Visibility.Hidden;
            this.CliTitle.Visibility = Visibility.Visible;
            this.CliMessages.Visibility = Visibility.Visible;
            if (this.value == Mode.client)
            {
                this.Title = "Client";
                CliMessages.Text += "Client connecting to server ...\n";
                //this.Manager.StartClient(getIp(), Int32.Parse(ConfigurationManager.AppSettings["port"]));
            }
            else if (this.value == Mode.serveur)
            {
                this.Title = "Serveur";
                this.Send.Visibility = Visibility.Visible;
                this.SendBtn.Visibility = Visibility.Visible;
                //await this.Manager.StartServerAsync(getIp(), Int32.Parse(8001));
            }
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


        #endregion

        #region listeners
        private void Server_Button_Click(object sender, RoutedEventArgs e)
        {
            this.value = Mode.serveur;
            change_ViewAsync();
        }

        private void Client_Button_Click(object sender, RoutedEventArgs e)
        {
            this.value = Mode.client;
            change_ViewAsync();
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            String data = Send.Text;
            this.Manager.Send(data);
            Console.WriteLine("Sending : " + data + " to client ...");
        }
        #endregion





    }
}
