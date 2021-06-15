using System;
using Waher.Security;

namespace Waher.Networking.CoAP.Test
{
	internal class User : IUser
	{
		private readonly string[] privileges;
		private readonly string userName;
		private readonly string passwordHash;
		private readonly string passwordHashType;

		public User(string UserName, string PasswordHash, string PasswordHashType,
			params string[] Privileges)
		{
			this.userName = UserName;
			this.passwordHash = PasswordHash;
			this.passwordHashType = PasswordHashType;
			this.privileges = Privileges;
		}

		public string UserName => this.userName;
		public string PasswordHash => this.passwordHash;
		public string PasswordHashType => this.passwordHashType;

		public bool HasPrivilege(string Privilege)
		{
			return Array.IndexOf<string>(this.privileges, Privilege) >= 0;
		}
	}
}
