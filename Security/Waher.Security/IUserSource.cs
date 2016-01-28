using System;
using System.Collections.Generic;
using System.Text;

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
		/// <param name="User">User, if found, null otherwise.</param>
		/// <returns>If a user with the given user name was found.</returns>
		bool TryGetUser(string UserName, out IUser User);
	}
}
