using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security
{
	/// <summary>
	/// Basic interface for a user.
	/// </summary>
	public interface IUser
	{
		/// <summary>
		/// User Name.
		/// </summary>
		string UserName
		{
			get;
		}

		/// <summary>
		/// Password Hash
		/// </summary>
		string PasswordHash
		{
			get;
		}

		/// <summary>
		/// Type of password hash. The empty stream means a clear-text password.
		/// </summary>
		string PasswordHashType
		{
			get;
		}

		/// <summary>
		/// If the user has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the user has the corresponding privilege.</returns>
		bool HasPrivilege(string Privilege);
	}
}
