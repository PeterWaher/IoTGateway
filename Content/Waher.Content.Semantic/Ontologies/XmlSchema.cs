using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// XML Schema Ontology
	/// </summary>
	public class XmlSchema
	{
		/// <summary>
		/// XML Schema Ontology
		/// </summary>
		public XmlSchema()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// If the interface understands objects such as <paramref name="Uri"/>.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Uri Uri)
		{
			return Uri.AbsolutePath.StartsWith(Namespace) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#
		/// </summary>
		public const string Namespace = "http://www.w3.org/2001/XMLSchema#";
	}
}
