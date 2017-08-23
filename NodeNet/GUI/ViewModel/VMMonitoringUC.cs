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

namespace NodeNet.GUI.ViewModel
{
   public class VmMonitoringUc : ViewModelBase
    {
        public ICommand MonitoringUcLoaded { get; set; }

        private ObservableCollection<DefaultNode> nodeList;
        public ObservableCollection<DefaultNode> NodeList
        {
            get => nodeList;
            set
            {
                nodeList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Task> taskList;
        public ObservableCollection<Task> TaskList
        {
            get => taskList;
            set
            {
                taskList = value;
                RaisePropertyChanged();
            }
        }

        [PreferredConstructor]
        public VmMonitoringUc()
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
                if (n.NodeGuid != d.NodeGuid)
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
            Console.WriteLine(@"Launch Cli");
            ObservableCollection<Task> newTaskList = new ObservableCollection<Task>();
            foreach (Task t in TaskList)
            {
                if (t.Id == taskId)
                {
                    t.Progression = 100;
                    t.State = NodeState.Finish;
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
                    n.State = NodeState.Finish;
                }
                newNodeList.Add(n);
            }
            NodeList = null;
            NodeList = newNodeList;
        }

        public void OnLoad()
        {
            Console.WriteLine(@"Launch Cli");
        }

        public void RefreshNodesState(DataInput input)
        {
            ObservableCollection<DefaultNode> list = new ObservableCollection<DefaultNode>();
            foreach (DefaultNode node in NodeList)
            {
                if (node.NodeGuid != input.NodeGuid)
                {
                    list.Add(node);
                }
                else
                {
                    node.State = ((Tuple<NodeState, double>)input.Data).Item1;
                    list.Add(node);
                }
            }
            NodeList = null;
            NodeList = list;
        }

        public void RefreshTaskState(int taskId,double progression)
        {
            ObservableCollection<Task> newList = new ObservableCollection<Task>();
            foreach (Task t in TaskList)
            {
                if(t.Id == taskId)
                {
                    t.Progression = progression;
                }
                newList.Add(t);  
            }
            TaskList = null;
            TaskList = newList;
        }

        public void NodeIsWorkingOnTask(string nodeGuid, int taskId)
        {
            ObservableCollection<DefaultNode> list = new ObservableCollection<DefaultNode>();
            foreach (DefaultNode node in NodeList)
            {
                if (node.NodeGuid == nodeGuid)
                {
                    node.WorkingTask = taskId;
                    node.State = NodeState.Work;
                }
                list.Add(node);
            }
            NodeList = null;
            NodeList = list;

            ObservableCollection<Task> tList = new ObservableCollection<Task>();
            foreach (Task task in TaskList)
            {
                if (task.Id == taskId)
                {
                    task.State = NodeState.InProgress;
                }
                tList.Add(task);
            }
            TaskList = null;
            TaskList = tList;
        }

        public void CreateTask(Task task)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => ViewModelLocator.VmlMonitorUcStatic.TaskList.Add(task));
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
                        t.State = NodeState.Error;
                    }   
                }
                newTaskList.Add(t);
            }
            TaskList = null;
            TaskList = newTaskList;
        }

        public void RefreshNodeState(List<string> nodeGuid, NodeState state,int taskId)
        {
            ObservableCollection<DefaultNode> newNodeList = new ObservableCollection<DefaultNode>();
            foreach(string guid in nodeGuid)
            {
                foreach (DefaultNode n in NodeList)
                {
                    if (n.NodeGuid == guid)
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

        public void NodeIsFailed(string nodeGuid)
        {
            ObservableCollection<DefaultNode> newNodeList = new ObservableCollection<DefaultNode>();
                foreach (DefaultNode n in NodeList)
                {
                    if (n.NodeGuid == nodeGuid)
                    {
                        n.State = NodeState.Error;
                        n.WorkingTask = -1;
                    }
                    newNodeList.Add(n);
                }
                NodeList = null;
                NodeList = newNodeList;
        }
    }

}
