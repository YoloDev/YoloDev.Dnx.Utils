using System;
using System.ComponentModel;

namespace YoloDev.Dnx.NativeUtils
{
  [AttributeUsage(AttributeTargets.Assembly)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public sealed class NativeApiAttribute : Attribute
  {
      readonly Type _target;
      readonly Type _implementation;

      public NativeApiAttribute(Type target, Type implementation)
      {
          _target = target;
          _implementation = implementation;
      }
  }
}
