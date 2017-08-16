using NodeNet.Network.States;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Tasks
{
    public class Task
    {
        private int id;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        private int parentTaskID;

        public int ParentTaskID
        {
            get { return parentTaskID; }
            set { parentTaskID = value; }
        }


        private double progression;
        public double Progression
        {
            get { return progression; }
            set { progression = value; }
        }

        private Object result;
        public object Result
        {
            get { return result; }
            set { result = value; }
        }

        private bool isMapped;

        public bool IsMapped
        {
            get { return isMapped; }
            set { isMapped = value; }
        }

        private NodeState state;

        public NodeState State
        {
            get { return state; }
            set { state = value; }
        }

        public Task(int taskID)
        {
            this.id = taskID;
        }

        public Task(int taskID,NodeState state)
        {
            this.id = taskID;
            this.state = state;
        }

        public Task(int taskID, int parentTaskID, NodeState state) : this(taskID)
        {
            this.id = taskID;
            this.parentTaskID = parentTaskID;
            this.state = state;
        }
    }
}
