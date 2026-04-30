using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Security;

namespace Waher.Networking.HTTP.Authentication
{
	/// <summary>
	/// Represents an HTTP authentication scheme that embeds a collection of authentication
	/// schemes, and adds an additional requirement, that the authenticated user has one or
	/// more specified privileges.
	/// </summary>
	public class RequiredPrivileges : HttpAuthenticationScheme
	{
		private readonly HttpAuthenticationScheme[] authenticationSchemes;
		private readonly string[] privileges;
		private readonly int nrAuthenticationSchemes;
		private readonly int nrPrivileges;

		/// <summary>
		/// Represents an HTTP authentication scheme that embeds a collection of authentication
		/// schemes, and adds an additional requirement, that the authenticated user has one or
		/// more specified privileges.
		/// </summary>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		/// <param name="Privileges">Required privileges.</param>
		public RequiredPrivileges(HttpAuthenticationScheme[] AuthenticationSchemes,
			params string[] Privileges)
		{
			this.authenticationSchemes = AuthenticationSchemes;
			this.privileges = Privileges;

			this.nrAuthenticationSchemes = AuthenticationSchemes?.Length ?? 0;
			this.nrPrivileges = Privileges?.Length ?? 0;
		}

		/// <summary>
		/// Gets available challenges for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge strings.</returns>
		public override string[] GetChallenges()
		{
			ChunkedList<string> Result = new ChunkedList<string>();
			string[] Challenges;
			int i;

			for (i = 0; i < this.nrAuthenticationSchemes; i++)
			{
				Challenges = this.authenticationSchemes[i].GetChallenges();
				if (!(Challenges is null))
					Result.AddRange(Challenges);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override async Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			HttpAuthenticationScheme Scheme;
			IUser User;
			int i, j;

			for (i = 0; i < this.nrAuthenticationSchemes; i++)
			{
				Scheme = this.authenticationSchemes[i];
				User = await Scheme.IsAuthenticated(Request);
				if (User is null)
					continue;

				for (j = 0; j < this.nrPrivileges; j++)
				{
					if (!User.HasPrivilege(this.privileges[j]))
					{
						User = null;
						break;
					}
				}

				if (!(User is null))
					return User;
			}

			return null;
		}
	}
}
