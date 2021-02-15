using System;

namespace Waher.Runtime.Inventory.Test.Definitions
{
    [Singleton]
    public class ParamsExample : IParamsExample
    {
        public ParamsExample(params SealedClass[] _)
        {
        }
    }
}