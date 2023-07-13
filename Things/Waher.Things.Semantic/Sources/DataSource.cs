using System;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Script.Model;
using Waher.Script.Persistence.SPARQL;

namespace Waher.Things.Semantic.Sources
{
	/// <summary>
	/// Makes harmonized data sources and nodes available as semantic information.
	/// </summary>
	public class DataSource : IGraphSource
	{
		/// <summary>
		/// Makes harmonized data sources and nodes available as semantic information.
		/// </summary>
		public DataSource()
		{
		}

		/// <summary>
		/// How well the source handles a given Graph URI.
		/// </summary>
		/// <param name="GraphUri">Graph URI</param>
		/// <returns>How well the URI is supported.</returns>
		public Grade Supports(Uri GraphUri)
		{
			if (!IsServerDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				return Grade.NotAtAll;
			}

			string s = GraphUri.AbsolutePath;
			string[] Parts = s.Split('/');
			int c = Parts.Length;

			if (c <= 1 || !string.IsNullOrEmpty(Parts[0]))
				return Grade.NotAtAll;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c <= 2 || !Gateway.ConcentratorServer.TryGetDataSource(Parts[1], out _))
				return Grade.NotAtAll;

			if (c > 4)
				return Grade.NotAtAll;

			return Grade.Excellent;
		}

		/// <summary>
		/// Checks if a domain is the server domain, or optionally, an alternative domain.
		/// </summary>
		/// <param name="Domain">Domain to check.</param>
		/// <param name="IncludeAlternativeDomains">If alternative domains are to be checked as well.</param>
		/// <returns>If the domain to check is the server domain, or optionally, an alternative domain.</returns>
		public static bool IsServerDomain(CaseInsensitiveString Domain, bool IncludeAlternativeDomains)
		{
			if (Domain == Gateway.Domain || (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain) && Domain == "localhost"))
				return true;

			if (IncludeAlternativeDomains)
			{
				foreach (CaseInsensitiveString s in Gateway.AlternativeDomains)
				{
					if (s == Domain)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Loads the graph
		/// </summary>
		/// <param name="GraphUri">Graph URI</param>
		/// <param name="Node">Node performing the loading.</param>
		/// <param name="NullIfNotFound">If null should be returned, if graph is not found.</param>
		/// <returns>Graph, if found, null if not found, and null can be returned.</returns>
		public async Task<ISemanticCube> LoadGraph(Uri GraphUri, ScriptNode Node, bool NullIfNotFound)
		{
			if (!IsServerDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				return null;
			}

			string s = GraphUri.AbsolutePath;
			string[] Parts = s.Split('/');
			int c = Parts.Length;

			if (c <= 1 || !string.IsNullOrEmpty(Parts[0]))
				return null;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c <= 2 || !Gateway.ConcentratorServer.TryGetDataSource(Parts[1], out _))
				return null;

			switch (c)
			{
				case 2: // DOMAIN/Source
					break;

				case 3: // /DOMAIN/Source/NodeID
					break;

				case 4: // /DOMAIN/Source/Partition/NodeID
					break;

				default:
					return null;
			}

			return null;
		}

	}
}
