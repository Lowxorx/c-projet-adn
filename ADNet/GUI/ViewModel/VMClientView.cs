using c_projet_adn.Network.Impl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            DNANode dnaNode = new DNANode("Node 1", "127.0.0.1", 3001);
            // TODO récupérer le txt sur la fenêtre d'avant
            dnaNode.Connect("127.0.0.1",3000);
            Console.WriteLine("OK client CO");
        }

        private void AppuiBTN()
        {
           // TODO STOP This Node
        }
    }
}
