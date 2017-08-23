using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.GUI.ViewModel;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    public class VmOrchView : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand CommandBtnClose { get; set; }
        public VmLogBox VmLogBox { get; set; }
        public VmOrchView()
        {
            CommandBtnClose = new RelayCommand(CloseWindow);
            WindowLoaded = new RelayCommand(OnLoad);
            VmLogBox = NodeNet.GUI.ViewModel.ViewModelLocator.VmlLogBoxUcStatic;
        }

        private void OnLoad()
        {
            DnaOrchestra orch = new DnaOrchestra("Orchestrator", TxtIp, 3000);
            orch.Listen();
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Serveur démarré, en écoute..." + Environment.NewLine;
        }

        private string txtIp;

        public string TxtIp
        {
            get => txtIp;
            set
            {
                txtIp = value;
                RaisePropertyChanged();
            }
        }

        private void CloseWindow()
        {
            CloseAction.Invoke();
        }

        public Action CloseAction { get; set; }

    }
}
