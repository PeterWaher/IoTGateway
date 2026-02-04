using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Things;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Graph source on the local machine.
	/// </summary>
	public class LocalContentSource : IGraphSource
	{
		/// <summary>
		/// Graph source on the local machine.
		/// </summary>
		public LocalContentSource()
		{
		}

		/// <summary>
		/// How well a source with a given URI can be loaded by the class.
		/// </summary>
		/// <param name="Source">Source URI</param>
		/// <returns>How well the class supports loading the graph.</returns>
		public Grade Supports(Uri Source)
		{
			if (!Source.IsAbsoluteUri || !IsLocal(Source))
				return Grade.NotAtAll;
			else
				return Grade.Barely;
		}

		/// <summary>
		/// Checks if an URI corresponds to a local file name resource.
		/// </summary>
		/// <param name="Source">Source</param>
		/// <returns>If the source is a local file name resource.</returns>
		public static bool IsLocal(Uri Source)
		{
			if (!InternetContent.IsLocalDomain(Source.Host, true))
				return false;

			if (!string.IsNullOrEmpty(Source.Query))
				return false;

			if (!Types.TryGetModuleParameter("HTTP", out IResourceMap ResourceMap))
				return false;

			return 
				ResourceMap.TryGetFileName(Source.AbsolutePath, true, out string _) ||
				ResourceMap.TryGetFileName("/" + Source.Host + Source.AbsolutePath, true, out string _);
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
			string XmlString = null;

			try
			{
				// TODO: Include caller credentials in request, if available.

				if (!Types.TryGetModuleParameter("HTTP", out IResourceMap ResourceMap))
					throw new Exception("No appropriate resource map registered.");

				if (!ResourceMap.TryGetFileName(Source.AbsolutePath, true, out string FileName))
					throw new Exception("Local resource file not found.");

				string ContentType = InternetContent.GetContentType(Path.GetExtension(FileName));
				byte[] Data = await Files.ReadAllBytesAsync(FileName);

				ContentResponse Response = await InternetContent.DecodeAsync(ContentType, Data, Source);
				Response.AssertOk();

				Result = Response.Decoded;

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
						Xml = XML.ParseXml(XmlString = s, true);
						return new RdfDocument(Xml);
					}
					else
						return new TurtleDocument(s);
				}

			}
			catch (XmlException ex)
			{
				if (NullIfNotFound)
					return null;
				else
					throw XML.AnnotateException(ex, XmlString);
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
