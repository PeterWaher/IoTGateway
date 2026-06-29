using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Authentication;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Runtime.Timing;
using Waher.Security;
using Waher.Security.Authorization;
using Waher.Security.JWT;
using Waher.Security.Users;
using Waher.Things.Metering;

namespace Waher.Things.Http
{
	/// <summary>
	/// HTTP module
	/// </summary>
	[Singleton]
	[ModuleDependency("Waher.Service.IoTBroker.XmppServerModule")]  // For JWT factory, if available.
	public class HttpModule : IModule, ITlsCertificateEndpoint, IAuthorization<HttpRequest>
	{
		internal const string PostPrivileges = "Admin.SensorData.Post";

		private static HttpServer webServer;
		private static SensorDataReceptorResource api;
		private static LocalWebServerNode localWebServerNode;
		private static X509Certificate certificate;
		private static Scheduler scheduler;
		private static string rootFolder;

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			try
			{
				if (!Types.TryGetModuleParameter("HTTP", out object Obj) ||
					!(Obj is HttpServer WebServer))
				{
					Log.Error("Local Web Server not found.");
					return Task.CompletedTask;
				}

				webServer = WebServer;

				if (!Types.TryGetModuleParameter("Root", out Obj) ||
					!(Obj is string RootFolder))
				{
					Log.Warning("Root folder not defined.");
					rootFolder = null;
				}
				else
					rootFolder = RootFolder;

				if (!Types.TryGetModuleParameter("Scheduler", out Obj) ||
					!(Obj is Scheduler Scheduler))
				{
					Log.Warning("Scheduler not available.");
					scheduler = null;
				}
				else
					scheduler = Scheduler;

				HttpAuthenticationScheme[] Schemes = GetAuthenticationSchemes(this);

				api = new SensorDataReceptorResource("/ReportSensorData", Schemes);
				webServer.Register(api);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Checks if an user or object is authorized to perform an action.
		/// </summary>
		/// <param name="Request">Resource to authorize access to.</param>
		/// <param name="User">User or object with access privileges.</param>
		/// <returns>If authorized access.</returns>
		public bool IsAuthorized(HttpRequest Request, IHasPrivileges User)
		{
			string Privilege;

			if (string.IsNullOrEmpty(Request.SubPath))
				Privilege = PostPrivileges;
			else
				Privilege = PostPrivileges + Request.SubPath.Replace('/', '.');

			return User.HasPrivilege(Privilege);
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes()
		{
			return GetAuthenticationSchemes(Array.Empty<string>());
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <param name="RequiredPrivilege">Required privilege.</param>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes(string RequiredPrivilege)
		{
			return GetAuthenticationSchemes(new SinglePrivilege<HttpRequest>(RequiredPrivilege));
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <param name="RequiredPrivileges">Required privileges.</param>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes(params string[] RequiredPrivileges)
		{
			if ((RequiredPrivileges?.Length ?? 0) == 0)
				return GetAuthenticationSchemes((IAuthorization<HttpRequest>)null);
			else
				return GetAuthenticationSchemes(Networking.HTTP.Authentication.RequiredPrivileges.GetAuthorization(RequiredPrivileges));
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <param name="Authorization">Resource authorization</param>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes(IAuthorization<HttpRequest> Authorization)
		{
			return GetAuthenticationSchemes(null, Authorization);
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes(Uri ResourceMetaData)
		{
			return GetAuthenticationSchemes(ResourceMetaData, Array.Empty<string>());
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		/// <param name="RequiredPrivilege">Required privilege.</param>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes(
			Uri ResourceMetaData, string RequiredPrivilege)
		{
			return GetAuthenticationSchemes(ResourceMetaData, 
				new SinglePrivilege<HttpRequest>(RequiredPrivilege));
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		/// <param name="RequiredPrivileges">Required privileges.</param>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes(
			Uri ResourceMetaData, params string[] RequiredPrivileges)
		{
			if ((RequiredPrivileges?.Length ?? 0) == 0)
				return GetAuthenticationSchemes(ResourceMetaData, (IAuthorization<HttpRequest>)null);
			else
			{
				return GetAuthenticationSchemes(ResourceMetaData,
					Networking.HTTP.Authentication.RequiredPrivileges.GetAuthorization(RequiredPrivileges));
			}
		}

		/// <summary>
		/// Gets an array of authentication schemes available to authorize access to a
		/// web resource.
		/// </summary>
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		/// <param name="Authorization">Resource authorization</param>
		/// <returns>Array of authentication schemes.</returns>
		public static HttpAuthenticationScheme[] GetAuthenticationSchemes(
			Uri ResourceMetaData, IAuthorization<HttpRequest> Authorization)
		{
			List<HttpAuthenticationScheme> Schemes = new List<HttpAuthenticationScheme>();
			string Domain;
			int MinStrength;
			bool Encrypted;

			if (!Types.TryGetModuleParameter("X509", out object Obj) ||
				!(Obj is X509Certificate Certificate))
			{
				if (Types.TryGetModuleParameter("Realm", out Obj) &&
					Obj is string Realm)
				{
					Domain = Realm;
				}
				else
					Domain = null;

				Encrypted = false;
				MinStrength = 0;
			}
			else
			{
				certificate = Certificate;
				Encrypted = true;
				Domain = BinaryTcpClient.GetDomainFromSubject(Certificate.Subject);
				MinStrength = 128;
			}

			if (Types.TryGetModuleParameter("JWT", out JwtFactory JwtFactory) &&
				!JwtFactory.Disposed)
			{
				Schemes.Add(new JwtAuthentication(Encrypted, MinStrength, Domain, null, 
					JwtFactory, ResourceMetaData));   

				// Any JWT token generated by the server will suffice. Does not have to point to a
				// registered user.
			}

			webServer ??= Types.TryGetModuleParameter<HttpServer>("HTTP");

			if (!(webServer is null) && webServer.ClientCertificates != ClientCertificates.NotUsed)
				Schemes.Add(new MutualTlsAuthentication(Users.Source));

			Schemes.Add(new BasicAuthentication(Encrypted, MinStrength, Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.MD5, Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.SHA256, Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.SHA3_256, Domain, Users.Source));

			if (!(webServer is null))
				Schemes.Add(new SessionAuthentication(webServer));

			if (Authorization is null)
				return Schemes.ToArray();
			else
			{
				return new HttpAuthenticationScheme[]
				{
					new RequiredPrivileges(Schemes.ToArray(), Authorization)
				};
			}
		}

		/// <summary>
		/// Checks if the Local Web Server Node has been created.
		/// </summary>
		public static async Task CheckLocalWebServerNode()
		{
			foreach (INode Node in await MeteringTopology.Root.ChildNodes)
			{
				if (Node is LocalWebServerNode LocalWebServerNode)
				{
					localWebServerNode = LocalWebServerNode;
					break;
				}
			}

			if (localWebServerNode is null)
			{
				localWebServerNode = new LocalWebServerNode()
				{
					NodeId = await (await Translator.GetDefaultLanguageAsync()).GetStringAsync(typeof(LocalWebServerNode), 1, "Local Web Server")
				};

				await MeteringTopology.Root.AddAsync(localWebServerNode);
			}
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			if (!(webServer is null) && !(api is null))
			{
				webServer.Unregister(api);
				webServer = null;
				api = null;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Updates the certificate used in mTLS negotiation.
		/// </summary>
		/// <param name="Certificate">Updated Certificate</param>
		public void UpdateCertificate(X509Certificate Certificate)
		{
			certificate = Certificate;
		}

		/// <summary>
		/// Current web server certificate.
		/// </summary>
		internal static X509Certificate Certificate => certificate;

		/// <summary>
		/// Web Server Root folder.
		/// </summary>
		internal static string RootFolder => rootFolder;

		/// <summary>
		/// Web Server instance.
		/// </summary>
		internal static HttpServer WebServer => webServer;

		/// <summary>
		/// Scheduler instance, if available.
		/// </summary>
		internal static Scheduler Scheduler => scheduler;

		/// <summary>
		/// Local web server node.
		/// </summary>
		internal static LocalWebServerNode LocalWebServerNode => localWebServerNode;
	}
}
