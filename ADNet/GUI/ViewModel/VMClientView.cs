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
        public ICommand ProbCleanResultBox { get; private set; }
        public ICommand QuantCleanResultBox { get; private set; }

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

        private String probSelectedFile;
        public String ProbSelectedFile
        {
            get
            {
                return probSelectedFile;
            }
            set
            {
                probSelectedFile = value;
                RaisePropertyChanged("ProbSelectedFile");
            }
        }

        private String connectIp;
        public String Connectip
        {
            get
            {
                return connectIp;
            }
            set
            {
                connectIp = value;
                RaisePropertyChanged("Connectip");
            }
        }
        private String quantSelectedFile;
        public String QuantSelectedFile
        {
            get
            {
                return quantSelectedFile;
            }
            set
            {
                quantSelectedFile = value;
                RaisePropertyChanged("QuantSelectedFile");
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
            ProbCleanResultBox = new RelayCommand(CleanProbBox);
            QuantCleanResultBox = new RelayCommand(CleanQuantBox);

            WindowLoaded = new RelayCommand(OnLoad);
            ICommandBtnClose = new RelayCommand(CloseWindow);
            VmLogBox = NodeNet.GUI.ViewModel.ViewModelLocator.VMLLogBoxUcStatic;
            UcVmMonitoring = NodeNet.GUI.ViewModel.ViewModelLocator.VMLMonitorUcStatic;
        }

        private void OnLoad()
        {
            ProbBtSendDataEnabled = false;
            QuantBtSendDataEnabled = false;
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Client démarré, en attente..." + Environment.NewLine;
            client = new DNAClient("Client", Connectip, 3001);
            client.Connect(Connectip, 3000);
        }

        private void QuantLoadFile()
        {
            OpenFileDialog loadfile = new OpenFileDialog();
            if (loadfile.ShowDialog() == DialogResult.OK)
            {
                QuantBtSendDataEnabled = true;
                QuantSelectedFile = "Fichier sélectionné : " + loadfile.FileName;
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
                "AA\tCC\tTT\tGG"+
                "AAAA\tCCCC\tTTTT\tGGGG\n" +
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
                "AA\tCC\tTT\tGG" +
                "AAAA\tCCCC\tTTTT\tGGGG\n" +
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
                "AA\tCC\tTT\tGG" +
                "AAAA\tCCCC\tTTTT\tGGGG\n" +
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
                ProbBtSendDataEnabled = true;
                ProbSelectedFile = "Fichier sélectionné : " + loadfile.FileName;
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
        private void CleanQuantBox()
        {
            QuantResultBox = string.Empty;
        }
        private void CleanProbBox()
        {
            ProbResultBox = string.Empty;
        }

    }
}
