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
            get => id;
            set => id = value;
        }

        private NodeState state;
        public NodeState State
        {
            get => state;
            set => state = value;
        }

        private double progression;

        public double Progression
        {
            get => progression;
            set => progression = value;
        }

        private string taskName;

        public string TaskName
        {
            get => taskName;
            set => taskName = value;
        }

        private DateTime startTime;

        public DateTime StartTime
        {
            get => startTime;
            set => startTime = value;
        }

        private DateTime endTime;

        public DateTime EndTime
        {
            get => endTime;
            set => endTime = value;
        }
        
        private double duration;

        public double Duration
        {
            get => duration;
            set => duration = value;
        }

        public Task(int id,NodeState state)
        {
            this.id = id;
            this.state = state;
        }
        public Task(int id, NodeState state,string taskName)
        {
            this.id = id;
            this.state = state;
            this.taskName = taskName;
        }
    }
}
