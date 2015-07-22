using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("YoloDev.Dnx.NativeUtils.Generator")]

namespace YoloDev.Dnx.NativeUtils
{
    public abstract class NativeMethods : IDisposable
    {
        public static TNativeApi Get<TNativeApi>()
          where TNativeApi : NativeMethods
        {
            // TODO: Fix - discover generated type

            var type = typeof(TNativeApi);
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.GetCustomAttribute<NativeApiAttribute>() == null)
                goto typeerror;

            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                goto typeerror;

            var instance = ctor.Invoke(new object[0]);
            return (TNativeApi)instance;

            typeerror:
            throw new InvalidOperationException($"Type {type.Name} is not a native API type.");
        }

        protected internal virtual TDelegate GetNativeMethodDelegate<TDelegate> (ref TDelegate field, NativeMethodAttribute attribute, string name)
        {
            throw new NotImplementedException();
        }

        public void Dispose ()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        ~NativeMethods()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
