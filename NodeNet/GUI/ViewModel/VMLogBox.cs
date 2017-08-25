using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Windows.Input;

namespace NodeNet.GUI.ViewModel
{
    public class VmLogBox : ViewModelBase
    {
        public ICommand LogboxLoaded { get; set; }

        [PreferredConstructor]
        public VmLogBox()
        {
            LogboxLoaded = new RelayCommand(OnLoad);
        }

        public void OnLoad()
        {
            //Console.WriteLine(@"Launch Cli");
        }

        private string logBox;
        public string LogBox
        {
            get => logBox;
            set
            {
                logBox = value;
                RaisePropertyChanged();
            }
        }
    }
}
