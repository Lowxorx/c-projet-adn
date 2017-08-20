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

        public void RefreshStateFromTaskResult(DataInput input)
        {
            int taskId = input.TaskId;
            Console.WriteLine("Refresh Task state");
            ObservableCollection<Task> newTaskList = new ObservableCollection<Task>();
            foreach (Task t in TaskList)
            {
                if (t.Id == taskId)
                {
                    t.Progression = 100;
                    t.State = NodeState.FINISH;
                }
                newTaskList.Add(t);
            }
            TaskList = null;
            TaskList = newTaskList;

            ObservableCollection<DefaultNode> newNodeList = new ObservableCollection<DefaultNode>();

            foreach (DefaultNode n in NodeList)
            {
                if(n.WorkingTask == input.TaskId)
                {
                    n.State = NodeState.FINISH;
                }
                newNodeList.Add(n);
            }
            NodeList = null;
            NodeList = newNodeList;
        }

        public void OnLoad()
        {
            Console.WriteLine("monitor loaded");
        }

        public void RefreshNodesState(DataInput input)
        {
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

        public void RefreshTaskState(int taskID,double progression)
        {
            ObservableCollection<Task> newList = new ObservableCollection<Task>();
            foreach (Task t in TaskList)
            {
                if(t.Id == taskID)
                {
                    t.Progression = progression;
                }
                newList.Add(t);  
            }
            TaskList = null;
            TaskList = newList;
        }

        public void NodeISWorkingOnTask(string nodeGuid, int taskId)
        {
            ObservableCollection<DefaultNode> list = new ObservableCollection<DefaultNode>();
            foreach (DefaultNode node in NodeList)
            {
                if (node.NodeGUID == nodeGuid)
                {
                    node.WorkingTask = taskId;
                    node.State = NodeState.WORK;
                }
                list.Add(node);
            }
            NodeList = null;
            NodeList = list;

            ObservableCollection<Task> taskList = new ObservableCollection<Task>();
            foreach (Task task in TaskList)
            {
                if (task.Id == taskId)
                {
                    task.State = NodeState.IN_PROGRESS;
                }
                taskList.Add(task);
            }
            TaskList = null;
            TaskList = taskList;
        }

        public void CreateTask(Task task)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => ViewModelLocator.VMLMonitorUcStatic.TaskList.Add(task)));
        }

        public void CancelTask(List<Task> tasks)
        {
            ObservableCollection<Task> newTaskList = new ObservableCollection<Task>();
            foreach (Task t in TaskList)
            {
                foreach (Task task in tasks)
                {
                    if (t.Id == task.Id)
                    {
                        t.Progression = 0;
                        t.State = NodeState.ERROR;
                    }   
                }
                newTaskList.Add(t);
            }
            TaskList = null;
            TaskList = newTaskList;
        }

        public void RefreshNodeState(List<String> nodeGuid, NodeState state,int taskId)
        {
            ObservableCollection<DefaultNode> newNodeList = new ObservableCollection<DefaultNode>();
            foreach(String guid in nodeGuid)
            {
                foreach (DefaultNode n in NodeList)
                {
                    if (n.NodeGUID == guid)
                    {
                        n.State = state;
                        n.WorkingTask = taskId;
                    }
                    newNodeList.Add(n);
                }
                NodeList = null;
                NodeList = newNodeList;
            }
            
        }

        public void NodeIsFailed(string nodeGUID)
        {
            ObservableCollection<DefaultNode> newNodeList = new ObservableCollection<DefaultNode>();
                foreach (DefaultNode n in NodeList)
                {
                    if (n.NodeGUID == nodeGUID)
                    {
                        n.State = NodeState.ERROR;
                        n.WorkingTask = -1;
                    }
                    newNodeList.Add(n);
                }
                NodeList = null;
                NodeList = newNodeList;
        }
    }

}
