using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// Dublin Core Elements 1.1
	/// </summary>
	public class DublinCoreElements1_1 : IOntology
	{
		/// <summary>
		/// Dublin Core Elements 1.1
		/// </summary>
		public DublinCoreElements1_1()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "dce11";

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
		/// http://purl.org/dc/elements/1.1/
		/// </summary>
		public const string Namespace = "http://purl.org/dc/elements/1.1/";
	}
}
