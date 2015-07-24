using System.Runtime.InteropServices;
using Microsoft.Framework.Runtime;
using YoloDev.Dnx.NativeUtils;

namespace NativeSample
{

    abstract class FooBar : NativeMethods
    {
        public class TestAttribute : System.Attribute {}
        readonly IRuntimeEnvironment _env;

        [Test]
        public FooBar(IRuntimeEnvironment env)
        {
            _env = env;
        }

        [NativeMethod("somelib", CallingConvention.Cdecl, BestFitMapping = false)]
        public abstract void Test();

        [NativeMethod("otherlib", CallingConvention.Cdecl)]
        public abstract int OtherTest(bool foo);
    }
}
