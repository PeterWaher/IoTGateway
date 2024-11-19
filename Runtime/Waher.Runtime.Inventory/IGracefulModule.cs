using System.Threading.Tasks;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Interface for graceful modules.
	/// </summary>
	public interface IGracefulModule : IModule
	{
		/// <summary>
		/// Prepares the module for being stopped.
		/// </summary>
		Task PrepareStop();
	}
}
