using c_projet_adn.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;

namespace ADNet.GUI.ViewModel
{
    class VMClientView : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand IcommandBtnClick { get; set; }
        public VMClientView()
        {
            IcommandBtnClick = new RelayCommand(AppuiBTN);
            WindowLoaded = new RelayCommand(OnLoad);
        }

        private void OnLoad()
        {
            // TODO : Implémenter l'attribution automatique des ports 
            DNANode dnaNode = new DNANode("Node 1", "127.0.0.1", 3001);
            dnaNode.Connect(TxtIp, 3000);
            Console.WriteLine("OK client CO");
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
