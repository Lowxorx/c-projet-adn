using NodeNet.Network.States;
using System;


namespace NodeNet.Tasks
{
    public class Task
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private NodeState state;
        public NodeState State
        {
            get { return state; }
            set { state = value; }
        }

        private double progression;

        public double Progression
        {
            get { return progression; }
            set { progression = value; }
        }

        private String taskName;

        public String TaskName
        {
            get { return taskName; }
            set { taskName = value; }
        }


        public Task(int id,NodeState state)
        {
            this.id = id;
            this.state = state;
        }
        public Task(int id, NodeState state,String taskName)
        {
            this.id = id;
            this.state = state;
            this.taskName = taskName;
        }
    }
}
