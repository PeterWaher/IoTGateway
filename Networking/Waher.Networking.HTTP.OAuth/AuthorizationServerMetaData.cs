using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Runtime.Collections;
using Waher.Security.JWS;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Provides OAUTH authorization server meta-data, as defined in RFC 8414.
	/// </summary>
	public class AuthorizationServerMetaData : HttpSynchronousResource, IHttpGetMethod
	{
		private readonly OAuthAuthorizeResource authorizeResource;

		/// <summary>
		/// /.well-known
		/// </summary>
		public const string WellKnowResourcePath = "/.well-known/oauth-authorization-server";

		/// <summary>
		/// Provides OAUTH authorization server meta-data, as defined in RFC 8414.
		/// </summary>
		public AuthorizationServerMetaData(OAuthAuthorizeResource AuthorizeResource)
			: base(WellKnowResourcePath)
		{
			this.authorizeResource = AuthorizeResource;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			StringBuilder sb = ProtectedResourceMetaData.GenerateServerUrl(Request, out _);
			string ServerUrl = sb.ToString();

			sb.Append(this.authorizeResource.ResourceName);
			string AuthorizeUri = sb.ToString();
			string TokenUri = ServerUrl + OAuthTokenResource.DefaultResourcePath;

			ChunkedList<string> GrantTypesSupported = new ChunkedList<string>(6)
			{
				"authorization_code",
				"implicit",
				"client_credentials",
				"password",
				"refresh_token"
			};

			if (!(this.authorizeResource.OAuthDeviceAuthorizationResource is null))
				GrantTypesSupported.Add(OAuthDeviceAuthorizationResource.GrantType);

			Dictionary<string, object> MetaData = new Dictionary<string, object>()
			{
				{ "issuer", ServerUrl },
				{ "authorization_endpoint", AuthorizeUri },
				{ "token_endpoint", TokenUri },
				{ "scopes_supported", OAuthScopesSupportedAttribute.RegisteredScopes },
				{ "token_endpoint_auth_methods_supported", new string[]
					{
						"client_secret_basic",
						"client_secret_post"
					} 
				},
				{ "token_endpoint_auth_signing_alg_values_supported", JwsAlgorithm.GetAlgorithmNames() },
				{ "response_types_supported", new string[] { "code", "token" } },
				{ "code_challenge_methods_supported", new string[] { "plain", "S256" } },
				{ "authorization_response_iss_parameter_supported", this.authorizeResource.JwtFactory?.HasIssuer ?? false },
				{ "grant_types_supported", GrantTypesSupported.ToArray() }
			};

			if (!(this.authorizeResource.OAuthRegistrationResource is null))
				MetaData["registration_endpoint"] = ServerUrl + this.authorizeResource.OAuthRegistrationResource.ResourceName;

			if (!(this.authorizeResource.OAuthDeviceAuthorizationResource is null))
				MetaData["device_authorization_endpoint"] = ServerUrl + this.authorizeResource.OAuthDeviceAuthorizationResource.ResourceName;

			await Response.Return(MetaData);
		}
	}
}
