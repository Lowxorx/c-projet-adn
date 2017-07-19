using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    public class VMOrchView : ViewModelBase
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

        private string clientResponse;

        public string ClientResponse
        {
            get { return clientResponse; }
            set
            {
                clientResponse = value;
                RaisePropertyChanged("ClientResponse");
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

        public void SetMessage(string s)
        {
            ClientResponse = s;
        }
    }
}
