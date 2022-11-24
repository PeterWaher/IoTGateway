namespace Waher.Security.JWT
{
	/// <summary>
	/// Represents a user in an external system.
	/// </summary>
	public class ExternalUser : IUser
	{
		/// <summary>
		/// Represents a user in an external system.
		/// </summary>
		/// <param name="UserName">User name</param>
		/// <param name="Token">JWT Token</param>
		public ExternalUser(string UserName, JwtToken Token)
		{
			this.UserName = UserName;
			this.Token = Token;
		}

		/// <summary>
		/// User Name.
		/// </summary>
		public string UserName { get; }

		/// <summary>
		/// JWT Token
		/// </summary>
		public JwtToken Token { get; }

		/// <summary>
		/// Password Hash
		/// </summary>
		public string PasswordHash => "N/A";

		/// <summary>
		/// Type of password hash. The empty stream means a clear-text password.
		/// </summary>
		public string PasswordHashType => "N/A";

		/// <summary>
		/// If the user has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the user has the corresponding privilege.</returns>
		public bool HasPrivilege(string Privilege) => false;
	}
}
