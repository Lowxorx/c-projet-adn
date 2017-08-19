using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

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

        public void RefreshTaskState(Task task)
        {
            Console.WriteLine("Refresh Task state");
            ObservableCollection<Task> newList = new ObservableCollection<Task>();
            bool taskIsAbsent = true;
            foreach (Task t in TaskList)
            {
                if(t.Id == task.Id)
                {
                    taskIsAbsent = false;
                    t.Progression = task.Progression;
                }
                newList.Add(t);  
            }
            TaskList = null;
            TaskList = newList;
            if (taskIsAbsent)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => ViewModelLocator.VMLMonitorUcStatic.TaskList.Add(task)));
            }
        }

        public void CancelTask(Task task)
        {
            Console.WriteLine("Refresh Task state");
            ObservableCollection<Task> newTaskList = new ObservableCollection<Task>();
            bool taskIsAbsent = true;
            foreach (Task t in TaskList)
            {
                if (t.Id == task.Id)
                {
                    taskIsAbsent = false;
                    t.Progression = 0;
                }
                newTaskList.Add(t);
            }
            TaskList = null;
            TaskList = newTaskList;
            if (taskIsAbsent)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => ViewModelLocator.VMLMonitorUcStatic.TaskList.Add(task)));
            }
        }

        internal void RefreshNodeState(string nodeGUID, NodeState state,int taskId)
        {
            ObservableCollection<DefaultNode> newNodeList = new ObservableCollection<DefaultNode>();

            foreach (DefaultNode n in NodeList)
            {
                if (n.NodeGUID == nodeGUID)
                {
                    n.State = state;
                    n.WorkingTask = taskId;
                }
                newNodeList.Add(n);
            }
            NodeList = null;
            NodeList = newNodeList;
        }

        internal void NodeIsFailed(string nodeGUID)
        {
           
        }
    }

}
