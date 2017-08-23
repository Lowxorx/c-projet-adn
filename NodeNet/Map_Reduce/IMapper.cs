using System;

namespace NodeNet.Map_Reduce
{
    public interface IMapper : ICloneable
    {
        object Map(object input,int nbMap);

        bool MapIsEnd();

        IMapper Reset();

        new object Clone();
    }
}
