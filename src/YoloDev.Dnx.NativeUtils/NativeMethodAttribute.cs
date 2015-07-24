using System;
using System.Runtime.InteropServices;

namespace YoloDev.Dnx.NativeUtils
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public sealed class NativeMethodAttribute : Attribute
  {
    readonly string _library;
    readonly CallingConvention _callingConvention;

    public bool BestFitMapping;
    public CharSet CharSet;
    public bool SetLastError;
    public bool ThrowOnUnmappableChar;

    public NativeMethodAttribute(string library, CallingConvention callingConvention)
    {
      _library = library;
      _callingConvention = callingConvention;
    }

    public CallingConvention CallingConvention => _callingConvention;
    public string Library => _library;
  }
}
