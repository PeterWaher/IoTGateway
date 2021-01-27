using System;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all HTTP authentication schemes, as defined in RFC-7235:
	/// https://datatracker.ietf.org/doc/rfc7235/
	/// </summary>
	public abstract class HttpAuthenticationScheme
	{
		private readonly bool requireEncryption;
#if !WINDOWS_UWP
		private readonly int minStrength;
#endif
		/// <summary>
		/// Base class for all HTTP authentication schemes, as defined in RFC-7235:
		/// https://datatracker.ietf.org/doc/rfc7235/
		/// </summary>
		public HttpAuthenticationScheme()
#if WINDOWS_UWP
			: this(false)
#else
			: this(false, 0)
#endif
		{
		}

#if WINDOWS_UWP
		/// <summary>
		/// Base class for all HTTP authentication schemes, as defined in RFC-7235:
		/// https://datatracker.ietf.org/doc/rfc7235/
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		public HttpAuthenticationScheme(bool RequireEncryption)
		{
			this.requireEncryption = RequireEncryption;
		}
#else
		/// <summary>
		/// Base class for all HTTP authentication schemes, as defined in RFC-7235:
		/// https://datatracker.ietf.org/doc/rfc7235/
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="MinStrength">Minimum security strength of algorithms used.</param>
		public HttpAuthenticationScheme(bool RequireEncryption, int MinStrength)
		{
			this.requireEncryption = RequireEncryption;
			this.minStrength = MinStrength;
		}
#endif
		/// <summary>
		/// If scheme requires encryption.
		/// </summary>
		public bool RequireEncryption => this.requireEncryption;

#if !WINDOWS_UWP
		/// <summary>
		/// Minimum security strength of algorithms used.
		/// </summary>
		public int MinStrength => this.minStrength;
#endif
		/// <summary>
		/// If the authentication scheme uses user sessions.
		/// </summary>
		public virtual bool UserSessions => false;

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public abstract string GetChallenge();

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public abstract Task<IUser> IsAuthenticated(HttpRequest Request);
	}
}
