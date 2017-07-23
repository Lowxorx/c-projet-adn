﻿using ADNet.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    class VMOrchView : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand IcommandBtnClick { get; set; }
        public VMOrchView()
        {
            IcommandBtnClick = new RelayCommand(AppuiBTN);
            WindowLoaded = new RelayCommand(OnLoad);
        }

        private void OnLoad()
        {
            DNAOrchestra orch = new DNAOrchestra("Orchestrator", TxtIp, 3000);
            orch.Listen();
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


        private void AppuiBTN()
        {
           // TODO STOP This Node
        }
    }
}
