using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Security;
using Waher.Security.Authorization;

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
		private readonly IAuthorization<HttpRequest> authorization;
		private readonly int nrAuthenticationSchemes;

		/// <summary>
		/// Represents an HTTP authentication scheme that embeds a collection of authentication
		/// schemes, and adds an additional requirement, that the authenticated user has one or
		/// more specified privileges.
		/// </summary>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		/// <param name="Privileges">Required privileges.</param>
		public RequiredPrivileges(HttpAuthenticationScheme[] AuthenticationSchemes,
			params string[] Privileges)
			: this(AuthenticationSchemes, GetAuthorization(Privileges))
		{
		}

		/// <summary>
		/// Represents an HTTP authentication scheme that embeds a collection of authentication
		/// schemes, and adds an additional requirement, that the authenticated user has one or
		/// more specified privileges.
		/// </summary>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		/// <param name="Authorization">Resource authorization</param>
		public RequiredPrivileges(HttpAuthenticationScheme[] AuthenticationSchemes,
			IAuthorization<HttpRequest> Authorization)
		{
			this.authenticationSchemes = AuthenticationSchemes;
			this.authorization = Authorization;
			this.nrAuthenticationSchemes = AuthenticationSchemes?.Length ?? 0;
		}

		private static IAuthorization<HttpRequest> GetAuthorization(string[] Privileges)
		{
			if (Privileges.Length == 1)
				return new SinglePrivilege<HttpRequest>(Privileges[0]);
			else
				return new MultiplePrivileges<HttpRequest>(Privileges);
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
			int i;

			for (i = 0; i < this.nrAuthenticationSchemes; i++)
			{
				Scheme = this.authenticationSchemes[i];
				User = await Scheme.IsAuthenticated(Request);
				if (User is null)
					continue;

				if (this.authorization.IsAuthorized(Request, User))
					return User;
			}

			return null;
		}
	}
}
