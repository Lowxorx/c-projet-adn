using ADNet.Worker.Impl;
using NodeNet.Network.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Network.Data;
using System.Net.Sockets;
using NodeNet.Worker;

namespace c_projet_adn.Network.Impl
{
    public class DNANode : Node
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker());
            Name = name;
            Address = address;
            Port = port;
        }
        public override void ReceiveCallback(IAsyncResult ar)
        {
            base.ReceiveCallback(ar);
            Tuple<Node, byte[]> state = (Tuple<Node, byte[]>)ar.AsyncState;
            byte[] buffer = state.Item2;
            Node node = state.Item1;
            Socket client = node.NodeSocket;
            try
            {
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                Console.WriteLine("Number of bytes received : " + bytesRead);
                if (bytesRead > 0)
                {
                    DataInput input = DataFormater.Deserialize<DataInput>(buffer);
                    IWorker worker = WorkerFactory.GetWorker(input.Method);
                     DataInput res = worker.DoWork(input);
                        if (res != null)
                        {
                            res.msgType = MessageType.RESPONSE;
                            SendData(node, res);
                        }
                }
                else
                {
                    receiveDone.Set();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());

            }

        }
    }
}
