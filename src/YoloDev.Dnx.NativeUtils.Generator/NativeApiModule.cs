using System;
using System.Collections.Generic;
using Microsoft.Framework.Runtime.Roslyn;
using YoloDev.Dnx.NativeUtils.Generator;

namespace YoloDev.Dnx.NativeUtils
{
    public class NativeApiModule : ICompileModule
    {
        public void AfterCompile(AfterCompileContext context)
        {
            // Do nothing.
        }

        public void BeforeCompile(BeforeCompileContext context)
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();

            var nativeApis = new List<NativeModel> ();

            var compilation = context.Compilation;
            foreach (var st in compilation.SyntaxTrees)
            {
                var sm = compilation.GetSemanticModel(st);
                nativeApis.AddRange(DiscoveryWalker.FindNativeApis(st, sm));
            }

            if (nativeApis.Count == 0)
                return;

            foreach(var api in nativeApis)
            {
                var st = NativeGenerator.Generate(api, compilation);
                compilation = compilation.AddSyntaxTrees(st);
            }

            context.Compilation = compilation;
        }
    }
}
