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
		/// Checks if the password is correct.
		/// </summary>
		/// <param name="Password">Password to check.</param>
		/// <param name="PasswordType">Type of password provided. If the empty string, the password is provided in clear text.
		/// If not the empty string, the password is hashed according to the authentication mechanism that is being used.</param>
		/// <returns>If the password is correct.</returns>
		bool CheckPassword(string Password, string PasswordType);

	}
}
