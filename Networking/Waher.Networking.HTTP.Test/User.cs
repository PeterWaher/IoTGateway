using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Test
{
	public class User : IUserWithClaims
	{
		public string UserName => "User";
		public string PasswordHash => "Password";
		public string PasswordHashType => string.Empty;

		public Task<IEnumerable<KeyValuePair<string, object>>> CreateClaims(bool Encrypted)
		{
			int IssuedAt = (int)Math.Round(DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds);
			int Expires = IssuedAt + 3600;

			List<KeyValuePair<string, object>> Claims = new List<KeyValuePair<string, object>>()
			{
				new(JwtClaims.JwtId, Guid.NewGuid().ToString()),
				new(JwtClaims.Subject, this.UserName),
				new(JwtClaims.IssueTime, IssuedAt),
				new(JwtClaims.ExpirationTime, Expires),
				new(JwtClaims.Issuer, "Unit test")
			};

			return Task.FromResult<IEnumerable<KeyValuePair<string, object>>>(Claims);
		}

		public async Task<string> CreateToken(JwtFactory Factory, bool Encrypted)
		{
			IEnumerable<KeyValuePair<string, object>> Claims = await this.CreateClaims(Encrypted);
			if (Claims is null)
				return null;

			return Factory.Create(Claims);
		}

		public bool HasPrivilege(string Privilege) => false;
	}
}
