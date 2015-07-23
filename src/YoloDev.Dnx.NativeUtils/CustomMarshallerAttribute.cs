using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoloDev.Dnx.NativeUtils
{
    public interface ICustomMarshaller<TFrom, TTo>
    {
        TTo Marshal(TFrom value);
        void Free(TTo native);
    }
}
