using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System;

namespace ADNet.GUI.ViewModel
{
    class VMADNetLauncher : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand IcommandBtnClick { get; set; }
        public ICommand IcommandRdbCliClick { get; set; }
        public ICommand IcommandRdbServClick { get; set; }

        private Boolean TxtIpEnabled = false;

        public Boolean TxtIpEnabledProp
        {
            get
            {
                return TxtIpEnabled;
            }
        }

        public VMADNetLauncher()
        {
            WindowLoaded = new RelayCommand(Test);
            IcommandBtnClick = new RelayCommand(AppuiBTN);
            IcommandRdbCliClick = new RelayCommand(ModeCliClick);
            IcommandRdbServClick = new RelayCommand(ModeServClick);
        }

        private void AppuiBTN()
        {
            System.Windows.Forms.MessageBox.Show("Appui BTN");
        }

        public void Test()
        {
            TxtIpEnabled = false;
            RaisePropertyChanged("TxtIpEnabledProp");
            System.Windows.Forms.MessageBox.Show("On load");
        }

        public void ModeCliClick()
        {
            TxtIpEnabled = true;
            RaisePropertyChanged("TxtIpEnabledProp");
        }

        public void ModeServClick()
        {
            TxtIpEnabled = false;
            RaisePropertyChanged("TxtIpEnabledProp");
        }
    }
}
