﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.IoTGateway;
using Waher.IoTGateway.Setup;
using Waher.IoTGateway.WebResources;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Authentication;
using Waher.Networking.XMPP.Concentrator;
using Waher.Runtime.Inventory;
using Waher.Security.JWT;
using Waher.Security.Users;

namespace Waher.WebService.Sparql
{
	/// <summary>
	/// Pluggable module registering the SPARQL endpoint to the web server.
	/// </summary>
	[Singleton]
	public class SparqlServiceModule : IModule
	{
		internal const string QueryPrivileges = "Admin.Graphs.Query";
		internal const string GetPrivileges = "Admin.Graphs.Get";
		internal const string AddPrivileges = "Admin.Graphs.Add";
		internal const string UpdatePrivileges = "Admin.Graphs.Update";
		internal const string DeletePrivileges = "Admin.Graphs.Delete";

		private SparqlEndpoint sparqlEndpoint;
		private GraphStore graphStore;

		/// <summary>
		/// Pluggable module registering the SPARQL endpoint to the web server.
		/// </summary>
		public SparqlServiceModule()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			List<HttpAuthenticationScheme> Schemes = new List<HttpAuthenticationScheme>();
			bool RequireEncryption;
			int MinSecurityStrength;

			if (DomainConfiguration.Instance.UseEncryption)
			{
				RequireEncryption = true;
				MinSecurityStrength = 128;
			}
			else
			{
				RequireEncryption = false;
				MinSecurityStrength = 0;
			}

			if (Types.TryGetModuleParameter("JWT", out object Obj) &&
				Obj is JwtFactory JwtFactory &&
				!JwtFactory.Disposed)
			{
				Schemes.Add(new JwtAuthentication(RequireEncryption, MinSecurityStrength, Gateway.Domain, null, JwtFactory));   // Any JWT token generated by the server will suffice. Does not have to point to a registered user.
			}

			if (!(Gateway.HttpServer is null) && Gateway.HttpServer.ClientCertificates != ClientCertificates.NotUsed)
				Schemes.Add(new MutualTlsAuthentication(Users.Source));

			Schemes.Add(new BasicAuthentication(RequireEncryption, MinSecurityStrength, Gateway.Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(RequireEncryption, MinSecurityStrength, DigestAlgorithm.MD5, Gateway.Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(RequireEncryption, MinSecurityStrength, DigestAlgorithm.SHA256, Gateway.Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(RequireEncryption, MinSecurityStrength, DigestAlgorithm.SHA3_256, Gateway.Domain, Users.Source));

			if (!(Gateway.HttpServer is null))
				Schemes.Add(new RequiredUserPrivileges(Gateway.HttpServer, QueryPrivileges));

			this.sparqlEndpoint = new SparqlEndpoint("/sparql", Schemes.ToArray());
			Gateway.HttpServer?.Register(this.sparqlEndpoint);

			this.graphStore = new GraphStore("/rdf-graph-store", Schemes.ToArray());
			Gateway.HttpServer?.Register(this.graphStore);

			if (!(Gateway.ConcentratorServer is null))
			{
				Gateway.ConcentratorServer.SourceRegistered += this.ConcentratorServer_SourceRegistered;
				Gateway.ConcentratorServer.SourceUnregistered += this.ConcentratorServer_SourceUnregistered;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			if (!(Gateway.HttpServer is null))
			{
				Gateway.HttpServer.Unregister(this.sparqlEndpoint);
				this.sparqlEndpoint = null;

				Gateway.HttpServer.Unregister(this.graphStore);
				this.graphStore = null;
			}

			if (!(Gateway.ConcentratorServer is null))
			{
				Gateway.ConcentratorServer.SourceRegistered -= this.ConcentratorServer_SourceRegistered;
				Gateway.ConcentratorServer.SourceUnregistered -= this.ConcentratorServer_SourceUnregistered;
			}

			return Task.CompletedTask;
		}

		private void ConcentratorServer_SourceRegistered(object sender, DataSourceEventArgs e)
		{
			GraphStore.InvalidateDefaultGraph();
		}

		private void ConcentratorServer_SourceUnregistered(object sender, DataSourceEventArgs e)
		{
			GraphStore.InvalidateDefaultGraph();
		}
	}
}
