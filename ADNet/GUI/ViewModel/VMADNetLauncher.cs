using ADNet.GUI.View;
using ADNet.Network.Impl;
using c_projet_adn.GUI.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
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

        private Boolean txtNodeEnabled = false;

        public Boolean TxtNodeEnabledProp
        {
            get
            {
                return txtNodeEnabled;
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
            TxtIpProp = "127.0.0.1";
        }

        private void CloseWindow()
        {
            CloseAction.Invoke();
        }

        private void AppuiBTN()
        {
            if (txtClientEnabled)
            {
                Console.WriteLine("Launch Cli");
                ClientView clientView = new ClientView();
                clientView.Show();
                CloseAction.Invoke();
            }
            else if (txtNodeEnabled)
            {
                Console.WriteLine("Launch Node");
                NodeView nodeView = new NodeView();
                VMNodeView vm = (VMNodeView)nodeView.DataContext;
                vm.TxtIp = TxtIpProp;
                nodeView.Show();
                CloseAction.Invoke();
            }
            else
            {
                Console.WriteLine("Launch Orch");
                OrchView orchView = new OrchView();
                VMOrchView vm = (VMOrchView)orchView.DataContext;
                vm.TxtIp = TxtIpProp;
                orchView.Show();
                CloseAction.Invoke();
            }
        }



        public void OnLoad()
        {
            txtClientEnabled = false;
            txtNodeEnabled = false;
            RaisePropertyChanged("TxtClientEnabledProp");
            RaisePropertyChanged("TxtNodeEnabledProp");
        }

        public void ModeCliClick()
        {
            txtClientEnabled = true;
            txtNodeEnabled = false;
            RaisePropertyChanged("TxtClientEnabledProp");
            RaisePropertyChanged("TxtNodeEnabledProp");
        }

        public void ModeNodeClick()
        {
            txtClientEnabled = false;
            txtNodeEnabled = true;
            RaisePropertyChanged("TxtClientEnabledProp");
            RaisePropertyChanged("TxtNodeEnabledProp");
        }

        public void ModeServClick()
        {
            txtClientEnabled = false;
            txtNodeEnabled = false;
            RaisePropertyChanged("TxtClientEnabledProp");
            RaisePropertyChanged("TxtNodeEnabledProp");
        }

        public Action CloseAction { get; set; }
    }
}
