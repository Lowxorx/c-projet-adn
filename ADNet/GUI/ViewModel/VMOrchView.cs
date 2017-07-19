using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    class VMOrchView : ViewModelBase
    {
        #region Properties
        public ICommand WindowLoaded { get; set; }
        public ICommand ICommandBtnSend { get; set; }
        private DNAOrchestra orch;

        private String txtMsg;
        public String TxtMsgProp
        {
            get
            {
                return txtMsg;
            }
            set
            {
                txtMsg = value;
                RaisePropertyChanged("TxtMsgProp");
            }
        }
        #endregion


        public VMOrchView()
        {
            WindowLoaded = new RelayCommand(OnLoad);
            ICommandBtnSend = new RelayCommand(SendMessage);
        }

        public void OnLoad()
        {
            orch = new DNAOrchestra("Orchestrator","127.0.0.1",3000);
            orch.Listen();
        }

        public void SendMessage()
        {
            Console.WriteLine("Sending message " + txtMsg + " to all clients");
            orch.SendMessage(txtMsg);
        }
    }
}
