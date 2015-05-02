﻿using YoloDev.Dnx.Utils;

namespace ContractSample
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Class1
    {
        public Class1([NotNull] string foo)
        {
            Foo(foo);
        }
        
        public void Foo([NotNull] string bar)
        {
            System.Console.WriteLine(bar);
        }
    }
}
