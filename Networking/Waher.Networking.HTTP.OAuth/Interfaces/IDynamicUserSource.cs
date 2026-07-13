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

		/// <summary>
		/// Updates an existing user.
		/// </summary>
		/// <param name="UserName">Name of the user to update.</param>
		/// <param name="UpdateRequest">Update request.</param>
		/// <returns>Information about registration, if successful, null if not
		/// able to update client.</returns>
		Task<IRegistration?> UpdateUser(string UserName, IRegistrationRequest UpdateRequest);

		/// <summary>
		/// Deletes an existing user.
		/// </summary>
		/// <param name="UserName">Name of the user to delete.</param>
		/// <returns>True if the user was successfully deleted, false otherwise.</returns>
		Task<bool> DeleteUser(string UserName);
	}
}
