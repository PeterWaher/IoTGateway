using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Text;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Things;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Graph source on the Internet.
	/// </summary>
	public class InternetContentSource : IGraphSource
	{
		/// <summary>
		/// Graph source on the Internet.
		/// </summary>
		public InternetContentSource()
		{
		}

		/// <summary>
		/// How well a source with a given URI can be loaded by the class.
		/// </summary>
		/// <param name="Source">Source URI</param>
		/// <returns>How well the class supports loading the graph.</returns>
		public Grade Supports(Uri Source)
		{
			if (!Source.IsAbsoluteUri ||
				!InternetContent.CanGet(Source, out Grade _, out _))
			{
				return Grade.NotAtAll;
			}
			else
				return Grade.Barely;
		}

		/// <summary>
		/// Loads the graph
		/// </summary>
		/// <param name="Source">Source URI</param>
		/// <param name="Node">Node performing the loading.</param>
		/// <param name="NullIfNotFound">If null should be returned, if graph is not found.</param>
		/// <param name="Caller">Information about entity making the request.</param>
		/// <returns>Graph, if found, null if not found, and null can be returned.</returns>
		public async Task<ISemanticCube> LoadGraph(Uri Source, ScriptNode Node, bool NullIfNotFound,
			RequestOrigin Caller)
		{
			object Result = null;

			try
			{
				// TODO: Include credentials in request, if available.

				Result = await InternetContent.GetAsync(Source,
					new KeyValuePair<string, string>("Accept", "text/turtle, application/x-turtle, application/rdf+xml;q=0.9, application/ld+json;q=0.8, text/xml;q=0.2, " + PlainTextCodec.DefaultContentType + ";q=0.1"));

				if (Result is ISemanticCube Cube)
					return Cube;

				if (Result is ISemanticModel Model)
					return await InMemorySemanticCube.Create(Model);

				if (Result is XmlDocument Xml)
					return new RdfDocument(Xml);

				if (Result is string s)
				{
					if (XML.IsValidXml(s))
					{
						Xml = new XmlDocument()
						{
							PreserveWhitespace = true
						};

						Xml.LoadXml(s);

						return new RdfDocument(Xml);
					}
					else
						return new TurtleDocument(s);
				}

			}
			catch (Exception ex)
			{
				if (NullIfNotFound)
					return null;
				else
					ExceptionDispatchInfo.Capture(ex).Throw();
			}

			if (NullIfNotFound)
				return null;
			else
			{
				StringBuilder sb = new StringBuilder();

				sb.Append("Graph not a semantic cube or semantic model: ");
				sb.Append(Source.ToString());
				sb.Append(" Type of content returned: ");
				sb.Append(Result?.GetType().FullName);

				throw new ScriptRuntimeException(sb.ToString(), Node);
			}
		}
	}
}
