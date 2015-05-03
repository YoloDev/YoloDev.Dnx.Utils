using System;
using Microsoft.Framework.Runtime.Roslyn;
using Microsoft.Framework.Runtime;
using YoloDev.Dnx.Utils.Rewriters;

namespace YoloDev.Dnx.Utils
{
    public class ContractCompileModule : ICompileModule
    {
        readonly IAssemblyLoadContextFactory _loadContextFactory;
        readonly IServiceProvider _services;
        
        public ContractCompileModule(IAssemblyLoadContextFactory loadContextFactory, IServiceProvider services)
        {
            _loadContextFactory = loadContextFactory;
            _services = services;
        }
        
        public void AfterCompile(AfterCompileContext context)
        {

        }

        public void BeforeCompile(BeforeCompileContext context)
        {
            //using (var loadContext = _loadContextFactory.Create()) 
            //{
                var compilation = context.Compilation;
                foreach (var st in compilation.SyntaxTrees)
                {
                    var root = st.GetRoot();
                    //if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
                    var newRoot = ContractSyntaxRewriter.Rewrite(root, compilation.GetSemanticModel(st), null, _services);
                    var newTree = st.WithRootAndOptions(newRoot, st.Options);
                    compilation = compilation.ReplaceSyntaxTree(st, newTree);
                }
                
                context.Compilation = compilation;
            //}
        }
    }
}
