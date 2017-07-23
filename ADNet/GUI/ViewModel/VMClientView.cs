using ADNet.Network.Impl;
using c_projet_adn.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.GUI.ViewModel;
using NodeNet.Network.Nodes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    public class VMClientView : ViewModelBase
    {
        #region Properties
        public ICommand WindowLoaded { get; set; }
        public ICommand ICommandBtnSend { get; set; }
        private DNAClient client;

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

        public VMMonitoringUC UcVmMonitoring { get; set; }
        public VMClientView()
        {
            WindowLoaded = new RelayCommand(OnLoad);
            UcVmMonitoring = NodeNet.GUI.ViewModel.ViewModelLocator.VMLMonitorUcStatic;
            ICommandBtnSend = new RelayCommand(SendMessage);
        }

        public void OnLoad()
        {
            client = new DNAClient("Client","127.0.0.1",3001);
            client.Connect("127.0.0.1", 3000);
            //StartMonitorNodes();
        }

        public void SendMessage()
        {
            Console.WriteLine("Sending message " + txtMsg + " to all clients");
            client.SendMessage(txtMsg);
        }

        public void SetMessage(string s)
        {
            ClientResponse = s;
        }

        private void StartMonitorNodes()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            bw.DoWork += (o, a) =>
            {
                while (true)
                {
                    ObservableCollection<Node> list = new ObservableCollection<Node>();
                    foreach (Node n in UcVmMonitoring.NodeList)
                    {
                        //n.RefreshNodesInfos();
                        list.Add(n);
                    }
                    UcVmMonitoring.NodeList = null;
                    UcVmMonitoring.NodeList = list;
                    Thread.Sleep(3000);
                }
            };
            bw.RunWorkerAsync();
            // Méthode avec Thread
            //Thread monitor = new Thread(() =>
            //{

            //})
            //{
            //    Name = "MonitoringThread"
            //};
            //monitor.Start();
        }
    }
}
