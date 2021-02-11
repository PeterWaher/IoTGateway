using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Inventory.Test.Definitions
{
    public sealed class SealedClass
    {
        public SealedClass(string domainName, string key, string secret)
        {
            this.Name = domainName;
            this.Key = key;
            this.Secret = secret;
        }

        public string Name { get; }
        public string Key { get; }
        public string Secret { get; }
    }
}
