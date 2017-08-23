using System;

namespace NodeNet.Data
{
    [Serializable]
    public class DataInput
    {
        public string ClientGuid;
        public string NodeGuid;
        public int TaskId;
        public int NodeTaskId;
        public MessageType MsgType;
        public string Method { get; set; }
        public object Data { get; set; }

        public override string ToString()
        {
            return "Data -> Method : " + Method + " ClientGuid : " + ClientGuid + " NodeGuid : " + NodeGuid + " TaskId  : " + TaskId + " Data : " + Data ; 
        }
    }
}
