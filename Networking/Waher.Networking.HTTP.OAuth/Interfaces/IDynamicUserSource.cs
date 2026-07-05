using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.HTTP.OAuth.Interfaces
{
	/// <summary>
	/// A dynamic user source, supporting registering new users.
	/// </summary>
	public interface IDynamicUserSource : IUserSource
	{
		/// <summary>
		/// Registers a new user.
		/// </summary>
		/// <param name="RegistrationRequest">Registration request.</param>
		/// <returns>Information about registration, if successful, null if not
		/// able to register client.</returns>
		Task<IRegistration?> RegisterUser(IRegistrationRequest RegistrationRequest);
	}
}
