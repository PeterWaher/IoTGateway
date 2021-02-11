using System;
using System.Reflection;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	public class ParamsArguments : IDisposable
	{
        private static ParamsArguments instance;
        private readonly Assembly appAssembly;
        private readonly Assembly[] additionalAssemblies;
        private readonly SealedClass[] domains;

        private ParamsArguments(Assembly appAssembly, Assembly[] additionalAssemblies, params SealedClass[] domains)
        {
            this.appAssembly = appAssembly;
            this.additionalAssemblies = additionalAssemblies;
            this.domains = domains;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            instance = null;
        }
    }
}
