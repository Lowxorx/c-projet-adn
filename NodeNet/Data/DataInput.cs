using System;

namespace NodeNet.Data
{
    [Serializable]
    public class DataInput
    {
        public String ClientGUID;
        public String NodeGUID;
        public MessageType MsgType;
        public String Method { get; set; }
        public Object Data { get; set; }
    }
}
