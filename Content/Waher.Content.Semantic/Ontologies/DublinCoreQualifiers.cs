using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// Dublin Core Qualifiers Ontology
	/// </summary>
	public class DublinCoreQualifiers : IOntology
	{
		/// <summary>
		/// Dublin Core Qualifiers Ontology
		/// </summary>
		public DublinCoreQualifiers()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "dcq";

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
		/// http://purl.org/metadata/dublin_core_qualifiers#
		/// </summary>
		public const string Namespace = "http://purl.org/metadata/dublin_core_qualifiers#";
	}
}
