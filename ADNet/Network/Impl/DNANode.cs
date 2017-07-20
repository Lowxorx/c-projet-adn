using ADNet.Worker.Impl;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Worker;
using NodeNet.Worker.Impl;
using System;
using System.Net.Sockets;

namespace ADNet.Network.Impl
{
    public class DNANode : Node
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker<String>(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker<String>());
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
                    String res = WorkerFactory.GetWorker(input.Method).DoWork(GenericWorker<Object,Object>.PrepareData(input.Data));
                    
                    if (res != null)
                    {
                        //res.MsgType = MessageType.RESPONSE;
                        //res.Method = DISPLAY_MESSAGE_METHOD;
                        //SendData(node, res);
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
