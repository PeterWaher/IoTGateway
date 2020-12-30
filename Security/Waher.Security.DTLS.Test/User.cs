using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Security.DTLS.Test
{
	public class User : IUser
	{
		private readonly string userName;
		private readonly string passwordHash;
		private readonly string passwordHashType;

		public User(string UserName, string PasswordHash, string PasswordHashType)
		{
			this.userName = UserName;
			this.passwordHash = PasswordHash;
			this.passwordHashType = PasswordHashType;
		}

		public string UserName => this.userName;
		public string PasswordHash => this.passwordHash;
		public string PasswordHashType => this.passwordHashType;

		public bool HasPrivilege(string Privilege)
		{
			return false;
		}
	}
}
