using NodeNet.Network.States;
using System;


namespace NodeNet.Tasks
{
    [Serializable]
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

        private DateTime startTime;

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        private DateTime endTime;

        public DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }


        private double duration;

        public double Duration
        {
            get { return duration; }
            set { duration = value; }
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
