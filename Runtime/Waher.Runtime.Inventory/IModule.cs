using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Interface for late-bound modules loaded at runtime.
	/// </summary>
	public interface IModule
	{
		/// <summary>
		/// Starts the module.
		/// </summary>
		Task Start();

		/// <summary>
		/// Stops the module.
		/// </summary>
		Task Stop();
	}
}
