namespace Waher.Security.DTLS.Test
{
	public class User(string UserName, string PasswordHash, string PasswordHashType) : IUser
	{
		private readonly string userName = UserName;
		private readonly string passwordHash = PasswordHash;
		private readonly string passwordHashType = PasswordHashType;

		public string UserName => this.userName;
		public string FederatedUserName => this.userName;
		public string FriendlyName => this.userName;
		public string PasswordHash => this.passwordHash;
		public string PasswordHashType => this.passwordHashType;

		public bool HasPrivilege(string Privilege)
		{
			return false;
		}
	}
}
