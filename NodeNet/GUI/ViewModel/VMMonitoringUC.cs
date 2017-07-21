using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using NodeNet.Network.Nodes;
using System;
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
            NodeList = new ObservableCollection<Node>();
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

        public void RefreshNodesInfo(Tuple<float,float> values)
        {
            ObservableCollection<Node> list = new ObservableCollection<Node>();
            foreach (Node n in NodeList)
            {
                n.CpuValue = values.Item1;
                n.RamValue = values.Item2;
                list.Add(n);
            }
            NodeList = null;
            NodeList = list;
        }

        public void OnLoad()
        {
            Console.WriteLine("monitor loaded");
        }
    }

}
