using ADNet.GUI.View;
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
        public ICommand IcommandRdbServClick { get; set; }

        private Boolean txtIpEnabled = false;

        public Boolean TxtIpEnabledProp
        {
            get
            {
                return txtIpEnabled;
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
            IcommandRdbServClick = new RelayCommand(ModeServClick);
            TxtIpProp = "127.0.0.1";
        }

        private void AppuiBTN()
        {
            if (txtIpEnabled)
            {
                ClientView cliView = new ClientView();
                VMClientView vm = (VMClientView)cliView.DataContext;
                vm.TxtIp = TxtIpProp;
                cliView.Show();
                CloseAction.Invoke();

            }
            else
            {
                OrchView orchView = new OrchView();
                orchView.Show();
                CloseAction.Invoke();
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

        public Action CloseAction { get; set; }
    }
}
