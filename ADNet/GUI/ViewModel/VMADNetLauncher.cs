using ADNet.GUI.View;
using ADNet.Network.Impl;
using c_projet_adn.GUI.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    public class VMADNetLauncher : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand IcommandBtnClick { get; set; }
        public ICommand IcommandRdbCliClick { get; set; }
        public ICommand IcommandRdbNodeClick { get; set; }
        public ICommand IcommandRdbServClick { get; set; }
        public ICommand ICommandBtnClose { get; private set; }

        private Boolean txtClientEnabled = false;
        public Boolean TxtClientEnabledProp
        {
            get
            {
                return txtClientEnabled;
            }
        }

        private Boolean nodeChecked = false;
        public Boolean NodeChecked
        {
            get
            {
                return nodeChecked;
            }
            set
            {
                nodeChecked = value;
                if (value)
                {
                    BtnEnabled = true;
                    BtnContent = "Lancer le node";
                }
                RaisePropertyChanged("NodeChecked");
            }
        }

        private Boolean clientChecked = false;
        public Boolean ClientChecked
        {
            get
            {
                return clientChecked;
            }
            set
            {
                clientChecked = value;
                if (value)
                {
                    BtnEnabled = true;
                    BtnContent = "Lancer le client";
                }
                RaisePropertyChanged("ClientChecked");
            }
        }

        private Boolean serverChecked = false;
        public Boolean ServerChecked
        {
            get
            {
                return serverChecked;
            }
            set
            {
                serverChecked = value;
                if (value)
                {
                    BtnEnabled = true;
                    BtnContent = "Lancer le serveur";
                }
                RaisePropertyChanged("ServerChecked");
            }
        }
        
        private Boolean btnEnabled = false;
        public Boolean BtnEnabled
        {
            get
            {
                return btnEnabled;
            }
            set
            {
                btnEnabled = value;
                RaisePropertyChanged("BtnEnabled");
            }
        }
        private String btnContent;
        public String BtnContent
        {
            get
            {
                return btnContent;
            }
            set
            {
                btnContent = value;
                RaisePropertyChanged("BtnContent");
            }
        }

        private String txtIpProp;
        public String TxtIpProp
        {
            get
            {
                return txtIpProp;
            }
            set
            {
                txtIpProp = value;
                RaisePropertyChanged("TxtIpProp");
            }
        }

        public VMADNetLauncher()
        {
            WindowLoaded = new RelayCommand(OnLoad);
            IcommandBtnClick = new RelayCommand(AppuiBTN);
            IcommandRdbCliClick = new RelayCommand(ModeCliClick);
            IcommandRdbNodeClick = new RelayCommand(ModeNodeClick);
            ICommandBtnClose = new RelayCommand(CloseWindow);
            IcommandRdbServClick = new RelayCommand(ModeServClick);
            TxtIpProp = GetLocalIPAddress();
            BtnContent = "-----";
            ServerChecked = false;
            ClientChecked = false;
            NodeChecked = false;
            BtnEnabled = false;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void CloseWindow()
        {
            CloseAction.Invoke();
        }

        private void AppuiBTN()
        {
            if (ClientChecked)
            {
                Console.WriteLine("Launch Cli");
                ClientView clientView = new ClientView();
                VMClientView vm = (VMClientView)clientView.DataContext;
                vm.Connectip = TxtIpProp;
                clientView.Show();
                CloseAction.Invoke();
            }
            else if (NodeChecked)
            {
                Console.WriteLine("Launch Node");
                NodeView nodeView = new NodeView();
                VMNodeView vm = (VMNodeView)nodeView.DataContext;
                vm.TxtIp = TxtIpProp;
                nodeView.Show();
                CloseAction.Invoke();
            }
            else if (ServerChecked)
            {
                Console.WriteLine("Launch Orch");
                OrchView orchView = new OrchView();
                VMOrchView vm = (VMOrchView)orchView.DataContext;
                vm.TxtIp = TxtIpProp;
                orchView.Show();
                CloseAction.Invoke();
            }
            else
            {
                Console.WriteLine("Aucun mode sélectionné");
            }

        }



        public void OnLoad()
        {
            txtClientEnabled = false;
            RaisePropertyChanged("TxtClientEnabledProp");
        }

        public void ModeCliClick()
        {
            txtClientEnabled = true;
            RaisePropertyChanged("TxtClientEnabledProp");
        }

        public void ModeNodeClick()
        {
            txtClientEnabled = true;
            RaisePropertyChanged("TxtClientEnabledProp");
        }

        public void ModeServClick()
        {
            txtClientEnabled = true;
            RaisePropertyChanged("TxtClientEnabledProp");
        }

        public Action CloseAction { get; set; }
    }
}
