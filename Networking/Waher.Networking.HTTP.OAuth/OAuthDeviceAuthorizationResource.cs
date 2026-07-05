using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH device authorization resource.
	/// </summary>
	public class OAuthDeviceAuthorizationResource : HttpSynchronousResource,
		IHttpGetMethod, IHttpPostMethod
	{
		/// <summary>
		/// Default token resource path: /oauth/refresh_token
		/// </summary>
		public const string DefaultResourcePath = "/oauth/authorize_device";

		/// <summary>
		/// Grant Type for device authorization flow.
		/// </summary>
		public const string GrantType = "urn:ietf:params:oauth:grant-type:device_code";

		private readonly IUserSource userSource;
		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private JwtFactory? jwtFactory;

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		public OAuthDeviceAuthorizationResource()
			: this(null, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthDeviceAuthorizationResource(JwtFactory? JwtFactory)
			: this(null, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(string ResourceName)
			: this(null, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(JwtFactory? JwtFactory, string ResourceName)
			: this(null, JwtFactory, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource)
			: this(UserSource, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource, JwtFactory? JwtFactory)
			: this(UserSource, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource, string ResourceName)
			: this(UserSource, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource, JwtFactory? JwtFactory,
			string ResourceName)
			: base(ResourceName)
		{
			this.jwtFactory = JwtFactory;
			this.userSource = UserSource ?? Security.Users.Users.Source;
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Data source for users, used to authenticate clients.
		/// </summary>
		public IUserSource Users => this.userSource;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
		}

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		/// <returns>Array of authentication schemes (possibly empty) available for
		/// authenticating the user making the request. If no default authentication
		/// is to be performed, null can be returned.</returns>
		public override HttpAuthenticationScheme[]? GetAuthenticationSchemes(HttpRequest Request)
		{
			if (this.jwtFactory is null)
			{
				if (Types.TryGetModuleParameter("JWT", out JwtFactory JwtFactory) &&
					!JwtFactory.Disposed)
				{
					this.jwtFactory = JwtFactory;
					this.authenticationSchemes = null;
				}
			}

			if (Request.Header.Authorization is null)
				return null;

			this.authenticationSchemes ??= OAuthTokenResource.CreateAuthenticationSchemes(
				this.jwtFactory, this.userSource);

			return this.authenticationSchemes;
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
		}

	}
}
