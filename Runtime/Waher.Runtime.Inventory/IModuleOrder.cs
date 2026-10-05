using System.Collections.Generic;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Interface for classes that order modules.
	/// </summary>
	public interface IModuleOrder : IComparer<IModule>
	{
		/// <summary>
		/// Sets the modules to order.
		/// </summary>
		/// <param name="Modules">Modules available.</param>
		void SetModules(IEnumerable<IModule> Modules);
	}
}
