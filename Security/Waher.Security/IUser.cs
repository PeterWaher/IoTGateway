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
