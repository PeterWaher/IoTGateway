using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Security.JWS;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Provides OAUTH authorization server meta-data, as defined in RFC 8414.
	/// </summary>
	public class AuthorizationServerMetaData : HttpSynchronousResource, IHttpGetMethod
	{
		private OAuthAuthorizeResource authorizeResource;

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
			StringBuilder sb = ProtectedResourceMetaData.GenerateServerUrl(Request, out int Port);
			string ServerUrl = sb.ToString();

			sb.Append(this.authorizeResource.ResourceName);
			string AuthorizeUri = sb.ToString();
			string TokenUri = ServerUrl + "/oauth/token"; // TODO

			Dictionary<string, object> MetaData = new Dictionary<string, object>()
			{
				{ "issuer", ServerUrl },
				{ "authorization_endpoint", AuthorizeUri },
				{ "token_endpoint", TokenUri },
				{ "scopes_supported", OAuthScopesSupportedAttribute.RegisteredScopes },
				{ "token_endpoint_auth_methods_supported", new string[]	// TODO: Dynamic
					{
						"client_secret_basic"	// TODO
					} 
				},
				{ "token_endpoint_auth_signing_alg_values_supported", JwsAlgorithm.GetAlgorithmNames() },
				{ "response_types_supported", new string[] { "code" } },
				{ "grant_types_supported", new string[] { "client_credentials", "password" } }
			};

			await Response.Return(MetaData);
		}
	}
}
