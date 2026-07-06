using System.Threading.Tasks;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH device authorization resource.
	/// </summary>
	public class OAuthDeviceAuthorizationResource : OAuthResource, IHttpGetMethod, 
		IHttpPostMethod
	{
		/// <summary>
		/// Default token resource path: /oauth/refresh_token
		/// </summary>
		public const string DefaultResourcePath = "/oauth/authorize_device";

		/// <summary>
		/// Grant Type for device authorization flow.
		/// </summary>
		public const string GrantType = "urn:ietf:params:oauth:grant-type:device_code";

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
			: base(UserSource, JwtFactory, ResourceName)
		{
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

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
