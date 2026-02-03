using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.IO;
using Waher.Runtime.Threading;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the remote endpoint has made too many connections.
	/// </summary>
	public class ConnectionsExceeded : RateLimitComparison
	{
		/// <summary>
		/// Checks if the remote endpoint has made too many connections.
		/// </summary>
		public ConnectionsExceeded()
			: base()
		{
		}

		/// <summary>
		/// Checks if the remote endpoint has made too many connections.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public ConnectionsExceeded(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(ConnectionsExceeded);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new ConnectionsExceeded(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			string Endpoint = State.Request.RemoteEndPoint;
			string Key = Endpoint.RemovePortNumber() + "|Connections";
			long Count;

			using (Semaphore Semaphore = await Semaphores.BeginWrite(Key))
			{
				if (State.TryGetCachedObject(Key, out Dictionary<string, bool> Connections))
					Connections[Endpoint] = true;
				else
				{
					Duration Duration = await this.EvaluateDurationAsync(State);

					Connections = new Dictionary<string, bool>()
					{
						{ Endpoint, true }
					};

					State.AddToCache(Key, Connections, DateTime.UtcNow + Duration);
				}

				Count = Connections.Count;
			}

			return await this.Review(State, Count);
		}
	}
}
