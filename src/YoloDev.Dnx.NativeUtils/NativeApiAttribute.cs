using System;
using System.ComponentModel;

namespace YoloDev.Dnx.NativeUtils
{
  [AttributeUsage(AttributeTargets.Class)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public sealed class NativeApiAttribute : Attribute {}
}
