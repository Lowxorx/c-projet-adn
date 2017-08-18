
using ADNet.GUI.ViewModel;
using NodeNet.Network;
using NodeNet.Network.Nodes;
using System;
using System.Net.Sockets;
using NodeNet.Data;
using NodeNet.Tasks;
using System.Collections.Generic;

namespace c_projet_adn.Network.Impl
{
    public class DNAClient : DefaultClient
    {
        private const String DNA_QUANT_METHOD = "DNA_QUANT";
        public DNAClient(String name, String adress, int port) : base(name,adress,port)
        {
            WorkerFactory.AddWorker(DNA_QUANT_METHOD, new TaskExecutor(this, DNAQuantStatDisplay, null, null));
        }

        public DNAClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}


        public Object ProcessDisplayMessageFunction(DataInput input)
        {
            Console.WriteLine("Client Process Display Response From Orchestrator Msg : " + input.Data);
            ViewModelLocator.VMLCliStatic.DisplayResult((String)input.Data);
            return null;
        }

        public void DNAQuantStat(String genomicString)
        {
            DataInput data = new DataInput()
            {
                Data = genomicString,
                ClientGUID = NodeGUID,
                MsgType = MessageType.CALL,
                Method = DNA_QUANT_METHOD
            };
            SendData(Orch, data);
        }

        public Object DNAQuantStatDisplay(DataInput input)
        {
            Console.WriteLine("DNAQuantStatDisplay");
            String display = "";
            foreach(Tuple<char,int> result in (List<Tuple<char,int>>)input.Data)
            {
                display += result.Item1 + " : " + result.Item2.ToString() + " "; 
            }
            ViewModelLocator.VMLCliStatic.DisplayResult(display);
            return null;
        }
    }
}