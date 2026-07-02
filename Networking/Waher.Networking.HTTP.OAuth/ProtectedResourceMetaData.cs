using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP.OAuth.MetaData;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Provides OAUTH resource meta-data, as defined in RFC 9728.
	/// </summary>
	public class ProtectedResourceMetaData : HttpSynchronousResource, IHttpGetMethod
	{
		/// <summary>
		/// /.well-known
		/// </summary>
		public const string WellKnowResourcePath = "/.well-known/oauth-protected-resource";

		/// <summary>
		/// Provides OAUTH resource meta-data, as defined in RFC 9728.
		/// </summary>
		public ProtectedResourceMetaData()
			: base(WellKnowResourcePath)
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

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
			string ResourceName = Request.SubPath;

			if (string.IsNullOrEmpty(ResourceName) ||
				!(Request.Server?.TryGetResource(ref ResourceName, false, out HttpResource Resource, out string SubPath) ?? false) ||
				!string.IsNullOrEmpty(SubPath))
			{
				await Response.SendResponse(new NotFoundException());
				return;
			}

			StringBuilder sb = GenerateServerUrl(Request, out int Port);
			string ServerUrl = sb.ToString();

			sb.Append(ResourceName);
			string ResourceUrl = sb.ToString();

			ClientCertificates ClientCertificatesConfig;

			if (Request.Server is null)
				ClientCertificatesConfig = ClientCertificates.NotUsed;
			else
				Request.Server.GetMTlsSettings(Port, out ClientCertificatesConfig, out _);

			Dictionary<string, object> MetaData = new Dictionary<string, object>()
			{
				{ "resource", ResourceUrl },
				{ "authorization_servers", new string[] { ServerUrl } },
				{ "bearer_methods_supported", new string[] { "header", "body" } },
				{ "tls_client_certificate_bound_access_tokens", ClientCertificatesConfig != ClientCertificates.NotUsed }
			};

			OAuthMetaDataAttribute[] OAuthMetaDAtaAttributes = Resource.GetType().GetCustomAttributes<OAuthMetaDataAttribute>(true) as OAuthMetaDataAttribute[] ?? Array.Empty<OAuthMetaDataAttribute>();

			foreach (OAuthMetaDataAttribute Attribute in OAuthMetaDAtaAttributes)
				await Attribute.AddMetaData(Resource, MetaData);

			await Response.Return(MetaData);
		}

		/// <summary>
		/// Generates a server URL, based on the request.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="Port">Port used in server URL.</param>
		/// <returns>Built URL</returns>
		public static StringBuilder GenerateServerUrl(HttpRequest Request, out int Port)
		{
			return GenerateServerUrl(Request.Server, Request.Encrypted, Request.Host, out Port);
		}

		/// <summary>
		/// Generates a server URL, based on the request.
		/// </summary>
		/// <param name="Encrypted">If access is encrypted.</param>
		/// <param name="Host">Host used to access server.</param>
		/// <param name="Port">Port used in server URL.</param>
		/// <returns>Built URL</returns>
		public StringBuilder GenerateServerUrl(bool Encrypted, string? Host, out int Port)
		{
			return GenerateServerUrl(this.FirstServer, Encrypted, Host, out Port);
		}

		/// <summary>
		/// Generates a server URL, based on the request.
		/// </summary>
		/// <param name="Server">HTTP Server hosting the resource.</param>
		/// <param name="Encrypted">If access is encrypted.</param>
		/// <param name="Host">Host used to access server.</param>
		/// <param name="Port">Port used in server URL.</param>
		/// <returns>Built URL</returns>
		public static StringBuilder GenerateServerUrl(HttpServer Server, bool Encrypted, 
			string? Host, out int Port)
		{ 
			StringBuilder sb = new StringBuilder();
			bool DefaultPort;

			sb.Append("http");
			if (Encrypted)
			{
				int[]? Ports = Server?.OpenHttpsPorts;

				if ((Ports?.Length ?? 0) > 0)
				{
					sb.Append('s');
					Port = GetPort(Server?.OpenHttpsPorts, HttpServer.DefaultHttpsPort);
					DefaultPort = Port == HttpServer.DefaultHttpsPort;
				}
				else
				{
					Port = GetPort(Server?.OpenHttpPorts, HttpServer.DefaultHttpPort);
					DefaultPort = Port == HttpServer.DefaultHttpPort;
				}
			}
			else
			{
				Port = GetPort(Server?.OpenHttpPorts, HttpServer.DefaultHttpPort);
				DefaultPort = Port == HttpServer.DefaultHttpPort;
			}

			sb.Append("://");
			sb.Append(Host ?? "localhost");	// TODO: Public IP, if available, instead of localhost.

			if (!DefaultPort)
			{
				sb.Append(':');
				sb.Append(Port.ToString());
			}

			return sb;
		}

		private static int GetPort(int[]? Ports, int DefaultPort)
		{
			if (Ports is null || Ports.Length == 0)
				return DefaultPort;
			else
				return Ports[0];
		}

		/// <summary>
		/// Gets the resource meta-data URI for a given resource.
		/// </summary>
		/// <param name="Encrypted">If access is encrypted.</param>
		/// <param name="Domain">Domain used to access the resource.</param>
		/// <param name="ResourceName">Name of the resource.</param>
		/// <returns>Resource meta-data URI.</returns>
		public string GetResourceMetaDataUri(bool Encrypted, string? Domain, string ResourceName)
		{
			StringBuilder sb = this.GenerateServerUrl(Encrypted, Domain, out _);
			sb.Append(this.ResourceName);
			sb.Append(ResourceName);
			return sb.ToString();
		}
	}
}
