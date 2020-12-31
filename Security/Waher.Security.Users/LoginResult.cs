using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.Users
{
	/// <summary>
	/// Result of login attempt
	/// </summary>
	public enum LoginResultType
	{
		/// <summary>
		/// Remote endpoint has been permanently blocked from the system.
		/// </summary>
		PermanentlyBlocked,

		/// <summary>
		/// Remote endpoint has been temporarily blocked from the system. New attempts can be made after <see cref="LoginResult.Next"/>.
		/// </summary>
		TemporarilyBlocked,

		/// <summary>
		/// User name or password incorrect
		/// </summary>
		InvalidCredentials,

		/// <summary>
		/// No password, or an ampty password provided.
		/// </summary>
		NoPassword,

		/// <summary>
		/// Login successful. User object can be accessed using <see cref="LoginResult.User"/>
		/// </summary>
		Success
	}

	/// <summary>
	/// Contains information about a login attempt.
	/// </summary>
	public class LoginResult
	{
		private readonly User user;
		private readonly DateTime? next;
		private readonly LoginResultType type;

		/// <summary>
		/// Remote endpoint has been blocked.
		/// </summary>
		/// <param name="Next">Time when a new login can be attempted.</param>
		public LoginResult(DateTime Next)
		{
			this.user = null;
			this.next = Next;
			this.type = Next == DateTime.MaxValue ? LoginResultType.PermanentlyBlocked : LoginResultType.TemporarilyBlocked;
		}

		/// <summary>
		/// Login attempt has been made.
		/// </summary>
		/// <param name="User">User object found and authenticated.</param>
		public LoginResult(User User)
		{
			this.user = User;
			this.next = null;
			this.type = User is null ? LoginResultType.InvalidCredentials : LoginResultType.Success;
		}

		/// <summary>
		/// Empty password provided
		/// </summary>
		public LoginResult()
		{
			this.user = null;
			this.next = null;
			this.type = LoginResultType.NoPassword;
		}

		/// <summary>
		/// User object corresponding to the successfully logged in user.
		/// </summary>
		public User User => this.user;

		/// <summary>
		/// Time when a new login can be attempted.
		/// </summary>
		public DateTime? Next => this.next;

		/// <summary>
		/// Type of login result.
		/// </summary>
		public LoginResultType Type => this.type;
	}
}
