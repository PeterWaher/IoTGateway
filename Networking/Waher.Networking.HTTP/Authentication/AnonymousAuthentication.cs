using System;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.HTTP.Authentication
{
	/// <summary>
	/// Anonymous access. Can be limited to certain HTTP methods.
	/// </summary>
	public class AnonymousAuthentication : HttpAuthenticationScheme
	{
		private readonly string[] methods;

		/// <summary>
		/// Anonymous access. Can be limited to certain HTTP methods.
		/// </summary>
		/// <param name="Methods">Methods for which to use anonymous authentication.</param>
		public AnonymousAuthentication(params string[] Methods)
		{
			this.methods = Methods;
		}

		/// <summary>
		/// Methods for which to use anonymous authentication.
		/// </summary>
		public string[] Methods => this.methods;

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			return null;
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			if (Array.IndexOf(this.methods, Request.Header.Method) < 0)
				return Task.FromResult<IUser>(null);
			else
				return Task.FromResult<IUser>(new AnonymousUser());
		}

		private class AnonymousUser : IUser
		{
			public AnonymousUser()
			{
			}

			public string UserName => " Anonymous ";
			public string PasswordHash => string.Empty;
			public string PasswordHashType => string.Empty;
			public bool HasPrivilege(string Privilege) => false;
		}
	}
}
