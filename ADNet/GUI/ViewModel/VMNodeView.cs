using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.GUI.ViewModel;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    class VmNodeView : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand CommandBtnClose { get; set; }
        public VmLogBox VmLogBox { get; set; }

        public VmNodeView()
        {
            CommandBtnClose = new RelayCommand(CloseWindow);
            WindowLoaded = new RelayCommand(OnLoad);
            VmLogBox = NodeNet.GUI.ViewModel.ViewModelLocator.VmlLogBoxUcStatic;
        }

        private void OnLoad()
        {
            // TODO : Implémenter l'attribution automatique des ports 
            DnaNode dnaNode = new DnaNode("Node", "127.0.0.1", 3002);
            dnaNode.Connect(TxtIp, 3000);
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Node démarré, en attente..." + Environment.NewLine;
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
