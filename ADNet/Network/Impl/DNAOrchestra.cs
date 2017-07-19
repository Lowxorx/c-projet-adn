using ADNet.Worker.Impl;
using NodeNet.Network.Data;
using NodeNet.Network.Nodes;
using NodeNet.Network.Orch;
using NodeNet.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ADNet.Network.Impl
{
    class DNAOrchestra : Orchestrator
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";

        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker());
        }

        public void SendMessage(String msg)
        {
            DataInput input = new DataInput()
            {
                Method = DISPLAY_MESSAGE_METHOD,
                Data = DataFormater.Serialize(msg),
                MsgType = MessageType.CALL
            };
            SendDataToAllNodes(input);
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
                this.bytearrayList = new List<byte[]>();


                if (bytesRead == 4096)
                {
                    byte[] data = buffer;
                    this.bytearrayList.Add(data);

                }
                else
                {
                    DataInput input;
                    if (bytearrayList.Count > 0)
                    {
                        byte[] data = bytearrayList
                                     .SelectMany(a => a)
                                     .ToArray();
                         input = DataFormater.Deserialize<DataInput>(data);
                    }
                    else
                    {
                        input = DataFormater.Deserialize<DataInput>(buffer);
                    }

                   
                    IWorker worker = WorkerFactory.GetWorker(input.Method);
                    worker.ProcessResponse(input, (d) => ProcessDisplayMessageFunction(input));
                    // Dans le cas d'un noeud client
                    Console.WriteLine("Get res from client : " + DataFormater.Deserialize<String>(input.Data));
                    receiveDone.Set();
                }

                Receive(node);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());

            }
        }

        public new void Stop()
        {
            throw new NotImplementedException();
        }

        public void ProcessDisplayMessageFunction(DataInput input)
        {
            Console.WriteLine("In process Display from DNAOrchestra");
        }
    }
}
