using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// FOAF (Friend of a Friend)
	/// </summary>
	public class Foaf : IOntology
	{
		/// <summary>
		/// FOAF (Friend of a Friend)
		/// </summary>
		public Foaf()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "foaf";

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
		/// http://xmlns.com/foaf/0.1/
		/// </summary>
		public const string Namespace = "http://xmlns.com/foaf/0.1/";
	}
}
