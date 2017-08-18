using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    public class VMOrchView : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand ICommandBtnClose { get; set; }
        public VMOrchView()
        {
            ICommandBtnClose = new RelayCommand(CloseWindow);
            WindowLoaded = new RelayCommand(OnLoad);
        }

        private void OnLoad()
        {
            DNAOrchestra orch = new DNAOrchestra("Orchestrator", TxtIp, 3000);
            orch.Listen();
            LogBox += DateTime.Now.ToLongTimeString() + " - Serveur démarré, en écoute...";
        }

        private string logBox;
        public string LogBox
        {
            get { return logBox; }
            set
            {
                logBox = value;
                RaisePropertyChanged("LogBox");
            }
        }

        private string txtIp;

        public string TxtIp
        {
            get { return txtIp; }
            set
            {
                txtIp = value;
                RaisePropertyChanged("TxtIp");
            }
        }

        private void CloseWindow()
        {
            CloseAction.Invoke();
        }

        public Action CloseAction { get; set; }

    }
}
