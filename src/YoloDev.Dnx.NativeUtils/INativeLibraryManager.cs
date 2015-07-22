using System;

namespace YoloDev.Dnx.NativeUtils
{
  public interface INativeLibraryManager
  {
    IntPtr GetNativeFunction (NativeMethods owner, string library, string name);
    void Free (NativeMethods owner);
  }
}
