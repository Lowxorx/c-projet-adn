using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using NodeNet.Data;
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
            NodeList = new ObservableCollection<DefaultNode>();
        }
        public ICommand MonitoringUcLoaded { get; set; }

        private ObservableCollection<DefaultNode> nodeList;
        public ObservableCollection<DefaultNode> NodeList
        {
            get { return nodeList; }
            set
            {
                nodeList = value;
                RaisePropertyChanged("NodeList");
            }
        }

        public void RefreshNodesInfo(DataInput d)
        {
            Console.WriteLine("refresh node info");
            ObservableCollection<DefaultNode> list = new ObservableCollection<DefaultNode>();
            foreach (DefaultNode n in NodeList)
            {
                if (n.NodeGUID != d.NodeGUID)
                {
                    list.Add(n);
                }
                else
                {
                    n.CpuValue = ((Tuple<float, float>)d.Data).Item1;
                    n.RamValue = ((Tuple<float, float>)d.Data).Item2;
                    list.Add(n);
                }
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
