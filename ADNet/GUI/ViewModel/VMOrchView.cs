using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.GUI.ViewModel;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    public class VMOrchView : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand ICommandBtnClose { get; set; }
        public VMLogBox VmLogBox { get; set; }
        public VMOrchView()
        {
            ICommandBtnClose = new RelayCommand(CloseWindow);
            WindowLoaded = new RelayCommand(OnLoad);
            VmLogBox = NodeNet.GUI.ViewModel.ViewModelLocator.VMLLogBoxUcStatic;
        }

        private void OnLoad()
        {
            DNAOrchestra orch = new DNAOrchestra("Orchestrator", TxtIp, 3000);
            orch.Listen();
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Serveur démarré, en écoute..." + Environment.NewLine;
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
