using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Interface for configurable modules.
	/// </summary>
	public interface IConfigurableModule : IModule
	{
		/// <summary>
		/// Gets an array of configurable pages for the module.
		/// </summary>
		/// <returns></returns>
		Task<IConfigurablePage[]> GetConfigurablePages();
	}
}
