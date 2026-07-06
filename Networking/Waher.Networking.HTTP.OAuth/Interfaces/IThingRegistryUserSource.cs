using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.HTTP.OAuth.Interfaces
{
	/// <summary>
	/// A Thing Registry user source, supporting management of devices, with information
	/// about ownership.
	/// </summary>
	public interface IThingRegistryUserSource : IUserSource
	{
		/// <summary>
		/// Checks if a user is a device, and if so, if it has an owner in the
		/// Thing Registry, and if so returns the owner of the device.
		/// </summary>
		/// <param name="Device">The device user.</param>
		/// <returns>The owner of the device, if any.</returns>
		Task<IUser?> TryGetOwner(IUser Device);
	}
}
