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
   public class VMMonitoringUC : ViewModelBase
    {
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

        private ObservableCollection<Task> taskList;
        public ObservableCollection<Task> TaskList
        {
            get { return taskList; }
            set
            {
                taskList = value;
                RaisePropertyChanged("TaskList");
            }
        }

        [PreferredConstructor]
        public VMMonitoringUC()
        {
            MonitoringUcLoaded = new RelayCommand(OnLoad);
            NodeList = new ObservableCollection<DefaultNode>();
            TaskList = new ObservableCollection<Task>();
        }


        public void RefreshNodesInfo(DataInput d)
        {
            ObservableCollection<DefaultNode> list = new ObservableCollection<DefaultNode>();
            foreach (DefaultNode n in NodeList)
            {
                if (n.NodeGUID != d.NodeGUID)
                {
                    list.Add(n);
                }
                else
                {
                    n.CpuValue = ((Tuple<float, double>)d.Data).Item1;
                    n.RamValue = ((Tuple<float, double>)d.Data).Item2;
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

        public void RefreshNodesState(DataInput input)
        {
            Console.WriteLine("Refresh Node state");
            ObservableCollection<DefaultNode> list = new ObservableCollection<DefaultNode>();
            foreach (DefaultNode node in NodeList)
            {
                if (node.NodeGUID != input.NodeGUID)
                {
                    list.Add(node);
                }
                else
                {
                    node.State = ((Tuple<NodeState, Double>)input.Data).Item1;
                    list.Add(node);
                }
            }
            NodeList = null;
            NodeList = list;
        }

        public void RefreshTaskState(DataInput input)
        {
            Console.WriteLine("Refresh Task state");
            ObservableCollection<DefaultNode> list = new ObservableCollection<DefaultNode>();
            foreach (DefaultNode node in NodeList)
            {
                if (node.NodeGUID != input.NodeGUID)
                {
                    list.Add(node);
                }
                else
                {
                    node.State = ((Tuple<NodeState, double>)input.Data).Item1;
                    node.Progression = ((Tuple<NodeState, Double>)input.Data).Item2;
                    node.WorkingTask = input.TaskId;
                    list.Add(node);
                }
            }
            NodeList = null;
            NodeList = list;
        }

        public void CancelTask(DataInput input)
        {
            throw new NotImplementedException();
        }
    }

}
