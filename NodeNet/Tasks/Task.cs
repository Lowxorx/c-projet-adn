using NodeNet.Network.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Task(int id,NodeState state)
        {
            this.id = id;
            this.state = state;
        }
    }
}
