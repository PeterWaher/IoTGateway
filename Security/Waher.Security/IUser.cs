namespace Waher.Security
{
	/// <summary>
	/// Basic interface for a user.
	/// </summary>
	public interface IUser : IHasPrivileges
	{
		/// <summary>
		/// User Name.
		/// </summary>
		string UserName
		{
			get;
		}

		/// <summary>
		/// Full Federated User Name.
		/// </summary>
		string FederatedUserName
		{
			get;
		}

		/// <summary>
		/// Friendly name of the user, for display purposes.
		/// </summary>
		string FriendlyName
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
	}
}
