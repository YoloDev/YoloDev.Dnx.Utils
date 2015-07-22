using System.Runtime.InteropServices;
using YoloDev.Dnx.NativeUtils;

namespace NativeSample
{
    abstract class FooBar : NativeMethods
    {
        [NativeMethod("somelib", CallingConvention.Cdecl, BestFitMapping = false)]
        public abstract void Test();

        [NativeMethod("otherlib", CallingConvention.Cdecl)]
        public abstract int OtherTest(bool foo);
    }
}
