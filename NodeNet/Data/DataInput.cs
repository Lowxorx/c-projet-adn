using System;

namespace NodeNet.Data
{
    [Serializable]
    public class DataInput
    {
        public String ClientGUID;
        public String NodeGUID;
        public int TaskId;
        public int SubTaskId;
        public MessageType MsgType;
        public String Method { get; set; }
        public Object Data { get; set; }

        public override string ToString()
        {
            return "Data -> Method : " + Method + " ClientGuid : " + ClientGUID + " NodeGuid : " + NodeGUID + " TaskId  : " + TaskId + " Data : " + Data ; 
        }
    }
}
