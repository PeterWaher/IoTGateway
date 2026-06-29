using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Security.Authorization;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Provides OAUTH resource meta-data, as defined in RFC 9728.
	/// </summary>
	public class ResourceMetaDataResource : HttpSynchronousResource, IHttpGetMethod
	{
		public ResourceMetaDataResource()
			: base("/.well-known")
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
				!(this.FirstServer?.TryGetResource(ref ResourceName, false, out HttpResource Resource, out string SubPath) ?? false) ||
				!string.IsNullOrEmpty(SubPath))
			{
				await Response.SendResponse(new NotFoundException());
				return;
			}

			StringBuilder sb = new StringBuilder();
			int Port;
			bool DefaultPort;

			sb.Append("http");
			if (Request.Encrypted)
			{
				sb.Append('s');
				Port = GetPort(this.FirstServer?.OpenHttpsPorts, HttpServer.DefaultHttpsPort);
				DefaultPort = Port == HttpServer.DefaultHttpsPort;
			}
			else
			{
				Port = GetPort(this.FirstServer?.OpenHttpsPorts, HttpServer.DefaultHttpPort);
				DefaultPort = Port == HttpServer.DefaultHttpPort;
			}

			ClientCertificates ClientCertificatesConfig;

			if (this.FirstServer is null)
				ClientCertificatesConfig = ClientCertificates.NotUsed;
			else
				this.FirstServer.GetMTlsSettings(Port, out ClientCertificatesConfig, out _);

			sb.Append("://");
			sb.Append(Request.Host);

			if (!DefaultPort)
			{
				sb.Append(':');
				sb.Append(Port.ToString());
			}

			string ServerUrl = sb.ToString();

			sb.Append(ResourceName);

			string ResourceUrl = sb.ToString();

			Dictionary<string, object> MetaData = new Dictionary<string, object>()
			{
				{ "resource", ResourceUrl },
				{ "authorization_servers", new string[] { ServerUrl } },
				{ "bearer_methods_supported", new string[] { "header" } },
				{ "tls_client_certificate_bound_access_tokens", ClientCertificatesConfig != ClientCertificates.NotUsed }
			};

			OAuthMetaDataAttribute[] OAuthMetaDAtaAttributes = Resource.GetType().GetCustomAttributes<OAuthMetaDataAttribute>(true) as OAuthMetaDataAttribute[] ?? Array.Empty<OAuthMetaDataAttribute>();

			foreach (OAuthMetaDataAttribute Attribute in OAuthMetaDAtaAttributes)
				Attribute.AddMetaData(Resource, MetaData);

			await Response.Return(MetaData);
		}

		private static int GetPort(int[]? Ports, int DefaultPort)
		{
			if (Ports is null || Ports.Length == 0)
				return DefaultPort;
			else
				return Ports[0];
		}
	}
}
