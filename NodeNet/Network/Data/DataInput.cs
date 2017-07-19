using System;

namespace NodeNet.Network.Data
{
    [Serializable]
    public class DataInput
    {
        public MessageType MsgType;
        public String Method { get; set; }
        public byte[] Data { get; set; }
    }
}
