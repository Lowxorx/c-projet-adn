using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;
using ADNet.GUI.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeView = ADNet.GUI.View.NodeView;

namespace ADNet.GUI.ViewModel
{
    public class VmadNetLauncher : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand IcommandBtnClick { get; set; }
        public ICommand IcommandRdbCliClick { get; set; }
        public ICommand IcommandRdbNodeClick { get; set; }
        public ICommand IcommandRdbServClick { get; set; }
        public ICommand CommandBtnClose { get; set; }

        private bool txtClientEnabled;
        public bool TxtClientEnabledProp => txtClientEnabled;

        private bool nodeChecked;
        public bool NodeChecked
        {
            get => nodeChecked;
            set
            {
                nodeChecked = value;
                if (value)
                {
                    BtnEnabled = true;
                    BtnContent = "Lancer le node";
                }
                RaisePropertyChanged();
            }
        }

        private bool clientChecked;
        public bool ClientChecked
        {
            get => clientChecked;
            set
            {
                clientChecked = value;
                if (value)
                {
                    BtnEnabled = true;
                    BtnContent = "Lancer le client";
                }
                RaisePropertyChanged();
            }
        }

        private bool serverChecked;
        public bool ServerChecked
        {
            get => serverChecked;
            set
            {
                serverChecked = value;
                if (value)
                {
                    BtnEnabled = true;
                    BtnContent = "Lancer le serveur";
                }
                RaisePropertyChanged();
            }
        }
        
        private bool btnEnabled;
        public bool BtnEnabled
        {
            get => btnEnabled;
            set
            {
                btnEnabled = value;
                RaisePropertyChanged();
            }
        }
        private string btnContent;
        public string BtnContent
        {
            get => btnContent;
            set
            {
                btnContent = value;
                RaisePropertyChanged();
            }
        }

        private string txtIpProp;
        public string TxtIpProp
        {
            get => txtIpProp;
            set
            {
                txtIpProp = value;
                RaisePropertyChanged();
            }
        }

        public VmadNetLauncher()
        {
            WindowLoaded = new RelayCommand(OnLoad);
            IcommandBtnClick = new RelayCommand(AppuiBtn);
            IcommandRdbCliClick = new RelayCommand(ModeCliClick);
            IcommandRdbNodeClick = new RelayCommand(ModeNodeClick);
            CommandBtnClose = new RelayCommand(CloseWindow);
            IcommandRdbServClick = new RelayCommand(ModeServClick);
            TxtIpProp = GetLocalIpAddress();
            BtnContent = "-----";
            ServerChecked = false;
            ClientChecked = false;
            NodeChecked = false;
            BtnEnabled = false;
        }

        public static string GetLocalIpAddress()
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

        private void AppuiBtn()
        {
            if (ClientChecked)
            {
                Console.WriteLine(@"Launch Cli");
                ClientView clientView = new ClientView();
                VmClientView vm = (VmClientView)clientView.DataContext;
                vm.Connectip = TxtIpProp;
                clientView.Show();
                CloseAction.Invoke();
            }
            else if (NodeChecked)
            {
                Console.WriteLine(@"Launch Cli");
                NodeView nodeView = new NodeView();
                VmNodeView vm = (VmNodeView)nodeView.DataContext;
                vm.TxtIp = TxtIpProp;
                nodeView.Show();
                CloseAction.Invoke();
            }
            else if (ServerChecked)
            {
                Console.WriteLine(@"Launch Cli");
                OrchView orchView = new OrchView();
                VmOrchView vm = (VmOrchView)orchView.DataContext;
                vm.TxtIp = TxtIpProp;
                orchView.Show();
                CloseAction.Invoke();
            }
            else
            {
                Console.WriteLine(@"Launch Cli");
            }

        }



        public void OnLoad()
        {
            txtClientEnabled = false;
            RaisePropertyChanged(()=> TxtClientEnabledProp);
        }

        public void ModeCliClick()
        {
            txtClientEnabled = true;
            RaisePropertyChanged(() => TxtClientEnabledProp);
        }

        public void ModeNodeClick()
        {
            txtClientEnabled = true;
            RaisePropertyChanged(() => TxtClientEnabledProp);
        }

        public void ModeServClick()
        {
            txtClientEnabled = true;
            RaisePropertyChanged(() => TxtClientEnabledProp);
        }

        public Action CloseAction { get; set; }
    }
}
