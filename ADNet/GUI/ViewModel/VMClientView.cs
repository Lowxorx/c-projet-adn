using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.GUI.ViewModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using ADNet.Network.Impl;
using NodeNet.Data;

namespace ADNet.GUI.ViewModel
{
    public class VmClientView : ViewModelBase
    {
        #region Properties
        public ICommand WindowLoaded { get; set; }
        public ICommand CommandBtnClose { get; set; }
        public ICommand QuantSelectFile { get; set; }
        public ICommand ProbSelectFile { get; set; }
        public ICommand QuantSendData { get; set; }
        public ICommand ProbSendData { get; set; }
        public ICommand ProbCleanResultBox { get; set; }
        public ICommand QuantCleanResultBox { get; set; }

        public VmLogBox VmLogBox { get; set; }

        private DnaClient client;

        private OpenFileDialog loadfile;

        private string probResultBox;
        public string ProbResultBox
        {
            get => probResultBox;
            set
            {
                probResultBox = value;
                RaisePropertyChanged();
            }
        }

        private string quantResultBox;
        public string QuantResultBox
        {
            get => quantResultBox;
            set
            {
                quantResultBox = value;
                RaisePropertyChanged();
            }
        }

        private string probSelectedFile;
        public string ProbSelectedFile
        {
            get => probSelectedFile;
            set
            {
                probSelectedFile = value;
                RaisePropertyChanged();
            }
        }

        private string connectIp;
        public string Connectip
        {
            get => connectIp;
            set
            {
                connectIp = value;
                RaisePropertyChanged();
            }
        }
        private string quantSelectedFile;
        public string QuantSelectedFile
        {
            get => quantSelectedFile;
            set
            {
                quantSelectedFile = value;
                RaisePropertyChanged();
            }
        }
        

        private bool quantBtSendDataEnabled;
        public bool QuantBtSendDataEnabled
        {
            get => quantBtSendDataEnabled;
            set
            {
                quantBtSendDataEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool probBtSendDataEnabled;
        public bool ProbBtSendDataEnabled
        {
            get => probBtSendDataEnabled;
            set
            {
                probBtSendDataEnabled = value;
                RaisePropertyChanged();
            }
        }

        public VmMonitoringUc UcVmMonitoring { get; set; }
        public Action CloseAction { get; set; }


        #endregion

        public VmClientView()
        {
            QuantSelectFile = new RelayCommand(QuantLoadFile);
            ProbSelectFile = new RelayCommand(ProbLoadFile);
            QuantSendData = new RelayCommand(QuantSendFile);
            ProbSendData = new RelayCommand(ProbSendFile);
            ProbCleanResultBox = new RelayCommand(CleanProbBox);
            QuantCleanResultBox = new RelayCommand(CleanQuantBox);

            WindowLoaded = new RelayCommand(OnLoad);
            CommandBtnClose = new RelayCommand(CloseWindow);
            VmLogBox = NodeNet.GUI.ViewModel.ViewModelLocator.VmlLogBoxUcStatic;
            UcVmMonitoring = NodeNet.GUI.ViewModel.ViewModelLocator.VmlMonitorUcStatic;
        }

        private void OnLoad()
        {
            ProbBtSendDataEnabled = false;
            QuantBtSendDataEnabled = false;
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Client démarré, en attente..." + Environment.NewLine;
            client = new DnaClient("Client", Connectip, 3001);
            client.Connect(Connectip, 3000);
        }

        private void QuantLoadFile()
        {
            loadfile = new OpenFileDialog();
            if (loadfile.ShowDialog() == DialogResult.OK)
            {
                QuantBtSendDataEnabled = true;
                QuantSelectedFile = "Fichier sélectionné : " + loadfile.FileName;
                VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Fichier sélectionné : " + loadfile.FileName + Environment.NewLine;
            }
        }
        private void QuantSendFile()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (o, a) =>
            {
                client.DnaQuantStat(client.DnaParseData(loadfile.FileName));
            };
            bw.RunWorkerAsync();
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Lancement du traitement DNA_QUANT " + Environment.NewLine;
        }
        private void ProbLoadFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() != DialogResult.OK) return;
            ProbBtSendDataEnabled = true;
            ProbSelectedFile = "Fichier sélectionné : " + openFile.FileName;
            VmLogBox.LogBox += DateTime.Now.ToLongTimeString() + " - Fichier sélectionné : " + openFile.FileName + Environment.NewLine;
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
