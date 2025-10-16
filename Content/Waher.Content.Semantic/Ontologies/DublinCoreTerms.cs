using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// Dublin Core Terms Ontology
	/// </summary>
	public class DublinCoreTerms : IOntology
	{
		/// <summary>
		/// Dublin Core Terms Ontology
		/// </summary>
		public DublinCoreTerms()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "dcterms";

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
		/// http://purl.org/dc/terms/
		/// </summary>
		public const string Namespace = "http://purl.org/dc/terms/";

		/// <summary>
		/// http://purl.org/dc/terms/type
		/// </summary>
		public static readonly Uri type = new Uri(Namespace + "type");

		/// <summary>
		/// http://purl.org/dc/terms/created
		/// </summary>
		public static readonly Uri created = new Uri(Namespace + "created");

		/// <summary>
		/// http://purl.org/dc/terms/updated
		/// </summary>
		public static readonly Uri updated = new Uri(Namespace + "updated");

		/// <summary>
		/// http://purl.org/dc/terms/creator
		/// </summary>
		public static readonly Uri creator = new Uri(Namespace + "creator");

		/// <summary>
		/// http://purl.org/dc/terms/contributor
		/// </summary>
		public static readonly Uri contributor = new Uri(Namespace + "contributor");
	}
}
