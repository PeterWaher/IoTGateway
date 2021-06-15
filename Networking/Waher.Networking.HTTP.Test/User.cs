using System;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	public class User : IUser
	{
		public string UserName
		{
			get { return "User"; }
		}

		public string PasswordHash
		{
			get { return "Password"; }
		}

		public string PasswordHashType
		{
			get { return string.Empty; }
		}

		public bool HasPrivilege(string Privilege)
		{
			return false;
		}
	}
}
