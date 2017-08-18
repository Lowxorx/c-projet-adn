using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
namespace NodeNet.GUI.ViewModel
{
    public class VMLogBox : ViewModelBase
    {
        public ICommand LogboxLoaded { get; set; }

        [PreferredConstructor]
        public VMLogBox()
        {
            LogboxLoaded = new RelayCommand(OnLoad);

        }

        public void OnLoad()
        {
            Console.WriteLine("logbox loaded");
        }

        private string logBox;
        public string LogBox
        {
            get { return logBox; }
            set
            {
                logBox = value;
                RaisePropertyChanged("LogBox");
            }
        }

    }
}
