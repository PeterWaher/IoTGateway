using System;
using System.Threading.Tasks;

namespace Waher.Security
{
	/// <summary>
	/// Interface for data sources containing users.
	/// </summary>
	public interface IUserSource
	{
		/// <summary>
		/// Tries to get a user with a given user name.
		/// </summary>
		/// <param name="UserName">User Name.</param>
		/// <returns>User, if found, null otherwise.</returns>
		Task<IUser> TryGetUser(string UserName);
	}
}
