using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Test
{
	public class User(string UserName, string Password, string Owner,
		string[] Privileges) : IUserWithClaims
	{
		public User(string UserName, string Password, string[] Privileges)
			: this(UserName, Password, string.Empty, Privileges)
		{
		}

		public string UserName { get; } = UserName;
		public string PasswordHash { get; } = Password;
		public string PasswordHashType { get; } = string.Empty;
		public string Owner { get; } = Owner;
		public string[] Privileges { get; } = Privileges;

		public Task<IEnumerable<KeyValuePair<string, object>>> CreateClaims(bool Encrypted)
		{
			int IssuedAt = (int)Math.Round(DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds);
			int Expires = IssuedAt + 3600;

			List<KeyValuePair<string, object>> Claims =
			[
				new(JwtClaims.JwtId, Guid.NewGuid().ToString()),
				new(JwtClaims.Subject, this.UserName),
				new(JwtClaims.IssueTime, IssuedAt),
				new(JwtClaims.ExpirationTime, Expires),
				new(JwtClaims.Issuer, "Unit test")
			];

			return Task.FromResult<IEnumerable<KeyValuePair<string, object>>>(Claims);
		}

		public async Task<string> CreateToken(JwtFactory Factory, bool Encrypted,
			params KeyValuePair<string, object>[] AdditionalClaims)
		{
			IEnumerable<KeyValuePair<string, object>> Claims = await this.CreateClaims(Encrypted);
			if (Claims is null)
				return null;

			return Factory.Create(JwtFactory.JoinClaims(Claims, AdditionalClaims));
		}

		public bool HasPrivilege(string Privilege)
		{
			return Array.IndexOf(this.Privileges, Privilege) >= 0;
		}
	}
}
