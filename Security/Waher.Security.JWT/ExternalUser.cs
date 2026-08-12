using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Represents a user in an external system.
	/// </summary>
	public class ExternalUser : IUserWithClaims
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
		/// Full Federated User Name.
		/// </summary>
		public string FederatedUserName
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				string s;

				sb.Append(this.UserName);

				if (!string.IsNullOrEmpty(s = this.Token.Issuer))
				{
					int i = s.IndexOf("://");
					if (i > 0)
						s = s.Substring(i + 3);

					i = s.IndexOf('/');
					if (i > 0)
						s = s.Substring(0, i);

					sb.Append('@');
					sb.Append(s);
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// Friendly name of the user, for display purposes.
		/// </summary>
		public string FriendlyName
		{
			get
			{
				if (this.Token.TryGetClaim(JwtClaims.NickName, out object Value))
					return Value?.ToString() ?? this.UserName;

				if (this.Token.TryGetClaim(JwtClaims.Name, out Value))
					return Value?.ToString() ?? this.UserName;

				if (this.Token.TryGetClaim(JwtClaims.GivenName, out Value))
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(Value?.ToString() ?? string.Empty);

					if (this.Token.TryGetClaim(JwtClaims.MiddleName, out Value))
					{
						sb.Append(' ');
						sb.Append(Value?.ToString() ?? string.Empty);
					}

					if (this.Token.TryGetClaim(JwtClaims.FamilyName, out Value))
					{
						sb.Append(' ');
						sb.Append(Value?.ToString() ?? string.Empty);
					}

					return sb.ToString();
				}

				return this.UserName;
			}
		}

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

		/// <summary>
		/// Creates a set of claims identifying the user.
		/// </summary>
		/// <param name="Encrypted">If communication is encrypted.</param>
		/// <returns>Set of claims.</returns>
		public Task<IEnumerable<KeyValuePair<string, object>>> CreateClaims(bool Encrypted)
		{
			return Task.FromResult(this.Token.Claims);
		}

		/// <summary>
		/// Creates a JWT Token referencing the user object.
		/// </summary>
		/// <param name="Factory">JWT Factory.</param>
		/// <param name="Encrypted">If communication is encrypted.</param>
		/// <param name="AdditionalClaims">Additional claims to include in the token.</param>
		/// <returns>Token, if able to create a token, null otherwise.</returns>
		public Task<string> CreateToken(JwtFactory Factory, bool Encrypted,
			params KeyValuePair<string, object>[] AdditionalClaims)
		{
			return Task.FromResult(this.Token.Token);
		}
	}
}
