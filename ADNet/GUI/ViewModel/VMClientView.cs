using c_projet_adn.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.GUI.ViewModel;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    public class VMClientView : ViewModelBase
    {
        #region Properties
        public ICommand WindowLoaded { get; set; }
        public ICommand ICommandBtnClose { get; set; }
        public ICommand QuantSelectFile { get; set; }
        public ICommand ProbSelectFile { get; set; }
        public ICommand QuantSendData { get; set; }
        public ICommand ProbSendData { get; set; }


        public VMLogBox VmLogBox { get; set; }

        private DNAClient client;

        private String probResultBox;
        public String ProbResultBox
        {
            get
            {
                return probResultBox;
            }
            set
            {
                probResultBox = value;
                RaisePropertyChanged("ProbResultBox");
            }
        }

        private String quantResultBox;
        public String QuantResultBox
        {
            get
            {
                return quantResultBox;
            }
            set
            {
                quantResultBox = value;
                RaisePropertyChanged("QuantResultBox");
            }
        }

        private bool quantBtSendDataEnabled;
        public bool QuantBtSendDataEnabled
        {
            get
            {
                return quantBtSendDataEnabled;
            }
            set
            {
                quantBtSendDataEnabled = value;
                RaisePropertyChanged("QuantBtSendDataEnabled");
            }
        }

        private bool probBtSendDataEnabled;
        public bool ProbBtSendDataEnabled
        {
            get
            {
                return probBtSendDataEnabled;
            }
            set
            {
                probBtSendDataEnabled = value;
                RaisePropertyChanged("ProbBtSendDataEnabled");
            }
        }

        public VMMonitoringUC UcVmMonitoring { get; set; }
        public Action CloseAction { get; set; }


        #endregion

        public VMClientView()
        {
            QuantSelectFile = new RelayCommand(QuantLoadFile);
            ProbSelectFile = new RelayCommand(ProbLoadFile);
            QuantSendData = new RelayCommand(QuantSendFile);
            ProbSendData = new RelayCommand(ProbSendFile);

            WindowLoaded = new RelayCommand(OnLoad);
            ICommandBtnClose = new RelayCommand(CloseWindow);
            VmLogBox = NodeNet.GUI.ViewModel.ViewModelLocator.VMLLogBoxUcStatic;
            UcVmMonitoring = NodeNet.GUI.ViewModel.ViewModelLocator.VMLMonitorUcStatic;
        }

        public void OnLoad()
        {
            ProbBtSendDataEnabled = false;
            QuantBtSendDataEnabled = false;
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Client démarré, en attente..." + Environment.NewLine;
            client = new DNAClient("Client","127.0.0.1",3001);
            client.Connect("127.0.0.1", 3000);
        }

        private void QuantLoadFile()
        {
            OpenFileDialog loadfile = new OpenFileDialog();
            if (loadfile.ShowDialog() == DialogResult.OK)
            {
                QuantBtSendDataEnabled = true;
                VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Fichier sélectionné : " + loadfile.FileName + Environment.NewLine;
            }
        }
        private void QuantSendFile()
        {
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Lancement du traitement DNA_QUANT " + Environment.NewLine;
            client.DNAQuantStat("AAAA\tCCCC\tTTTT\tGGGG\n" +
                "AAA\tCCC\tTTT\tGGG\n" +
                "AA\tCC\tTT\tGG\n" +
                "AAAA\tCCCC\tTTTT\tGGGG\n" +
                "AAA\tCCC\tTTT\tGGG\n" +
                "AA\tCC\tTT\tGG\n" +
                "AAAA\tCCCC\tTTTT\tGGGG\n" +
                "AAA\tCCC\tTTT\tGGG\n" +
                "AA\tCC\tTT\tGG\n" +
                "AAAA\tCCCC\tTTTT\tGGGG\n" +
                "AAA\tCCC\tTTT\tGGG\n" +
                "AA\tCC\tTT\tGG");
        }
        private void ProbLoadFile()
        {
            OpenFileDialog loadfile = new OpenFileDialog();
            if (loadfile.ShowDialog() == DialogResult.OK)
            {
                VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Fichier sélectionné : " + loadfile.FileName + Environment.NewLine;
            }
        }
        private void ProbSendFile()
        {
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - DNA_PROB n'est pas implémenté." + Environment.NewLine;
        }

        public void QuantDisplayResult(string s)
        {
            QuantResultBox = s;
        }

        private void CloseWindow()
        {
            CloseAction.Invoke();
        }

    }
}
