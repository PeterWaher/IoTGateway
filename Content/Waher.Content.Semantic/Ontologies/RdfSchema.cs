using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// RDF Schema Ontology
	/// </summary>
	public class RdfSchema : IOntology
	{
		/// <summary>
		/// RDF Schema Ontology
		/// </summary>
		public RdfSchema()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "rdfs";

		/// <summary>
		/// If the ontology should be shown by default in query interfaces.
		/// </summary>
		public bool ShowByDefault => true;

		/// <summary>
		/// If the interface understands objects such as <paramref name="Uri"/>.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(string Uri)
		{
			return Uri.StartsWith(Namespace) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// http://www.w3.org/2000/01/rdf-schema#
		/// </summary>
		public const string Namespace = "http://www.w3.org/2000/01/rdf-schema#";

		/// <summary>
		/// http://www.w3.org/2000/01/rdf-schema#label
		/// </summary>
		public static readonly Uri label = new Uri(Namespace + "label");

		/// <summary>
		/// http://www.w3.org/2000/01/rdf-schema#comment
		/// </summary>
		public static readonly Uri comment = new Uri(Namespace + "comment");
	}
}
