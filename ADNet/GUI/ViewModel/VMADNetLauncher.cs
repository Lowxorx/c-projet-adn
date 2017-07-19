using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System;
using ADNet.GUI.View;
using c_projet_adn.GUI.View;

namespace ADNet.GUI.ViewModel
{
    class VMADNetLauncher : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand IcommandBtnClick { get; set; }
        public ICommand IcommandRdbCliClick { get; set; }
        public ICommand IcommandRdbServClick { get; set; }

        private Boolean txtIpEnabled = false;

        public Boolean TxtIpEnabledProp
        {
            get
            {
                return txtIpEnabled;
            }
        }

        private String txtIp;
        public String TxtIpProp
        {
            get
            {
                return txtIp;
            }
            set
            {
                txtIp = value;
                RaisePropertyChanged("TxtIpProp");
            }
        }

        public VMADNetLauncher()
        {
            WindowLoaded = new RelayCommand(OnLoad);
            IcommandBtnClick = new RelayCommand(AppuiBTN);
            IcommandRdbCliClick = new RelayCommand(ModeCliClick);
            IcommandRdbServClick = new RelayCommand(ModeServClick);
        }

        private void AppuiBTN()
        {
            if (txtIpEnabled)
            {
                ClientView cliView = new ClientView(txtIp);
                cliView.Show();
            }
            else
            {
                OrchView orchView = new OrchView();
                orchView.Show();
            }
        }

        public void OnLoad()
        {
            txtIpEnabled = false;
            RaisePropertyChanged("TxtIpEnabledProp");
        }

        public void ModeCliClick()
        {
            txtIpEnabled = true;
            RaisePropertyChanged("TxtIpEnabledProp");
        }

        public void ModeServClick()
        {
            txtIpEnabled = false;
            RaisePropertyChanged("TxtIpEnabledProp");
        }
    }
}
