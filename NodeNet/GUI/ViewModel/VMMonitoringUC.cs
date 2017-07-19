using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using NodeNet.Network.Nodes;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NodeNet.GUI.ViewModel
{
   public class VMMonitoringUC : ViewModelBase
    {
        [PreferredConstructor]
        public VMMonitoringUC()
        {
            MonitoringUcLoaded = new RelayCommand(OnLoad);
            nodeList = new ObservableCollection<Node>();
        }
        public ICommand MonitoringUcLoaded { get; set; }

        private ObservableCollection<Node> nodeList;
        public ObservableCollection<Node> NodeList
        {
            get { return nodeList; }
            set
            {
                nodeList = value;
                RaisePropertyChanged("NodeList");
            }
        }

        public void OnLoad()
        {
            System.Console.WriteLine("monitor loaded");
        }
    }

}
