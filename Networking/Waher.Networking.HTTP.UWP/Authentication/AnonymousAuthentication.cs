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
#if WINDOWS_UWP
			: this(false, Methods)
#else
			: this(false, 0, Methods)
#endif
		{
		}

#if WINDOWS_UWP
		/// <summary>
		/// Anonymous access. Can be limited to certain HTTP methods.
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="Methods">Methods for which to use anonymous authentication.</param>
		public AnonymousAuthentication(bool RequireEncryption, params string[] Methods)
			: base(RequireEncryption)
#else
		/// <summary>
		/// Anonymous access. Can be limited to certain HTTP methods.
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="MinStrength">Minimum security strength of algorithms used.</param>
		/// <param name="Methods">Methods for which to use anonymous authentication.</param>
		public AnonymousAuthentication(bool RequireEncryption, int MinStrength, params string[] Methods)
			: base(RequireEncryption, MinStrength)
#endif
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
