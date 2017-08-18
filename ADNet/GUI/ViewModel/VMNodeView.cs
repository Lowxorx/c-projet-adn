using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.GUI.ViewModel;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    class VMNodeView : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand ICommandBtnClose { get; set; }
        public VMLogBox VmLogBox { get; set; }

        public VMNodeView()
        {
            ICommandBtnClose = new RelayCommand(CloseWindow);
            WindowLoaded = new RelayCommand(OnLoad);
            VmLogBox = NodeNet.GUI.ViewModel.ViewModelLocator.VMLLogBoxUcStatic;
        }

        private void OnLoad()
        {
            // TODO : Implémenter l'attribution automatique des ports 
            DNANode dnaNode = new DNANode("Node", "127.0.0.1", 3002);
            dnaNode.Connect(TxtIp, 3000);
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Node démarré, en attente..." + Environment.NewLine;
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
