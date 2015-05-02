﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Runtime;
using YoloDev.Dnx.Utils;


namespace ContractSample.compiler.preprocess
{
    public class AddContracts : ContractCompileModule
    {
        public AddContracts(IAssemblyLoadContextFactory loadContextFactory, IServiceProvider services)
            : base(loadContextFactory, services)
        {
        }
    }
}
