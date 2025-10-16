using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// vCard Ontology
	/// </summary>
	public class vCard : IOntology
	{
		/// <summary>
		/// vCard Ontology
		/// </summary>
		public vCard()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "vcard";

		/// <summary>
		/// If the ontology should be shown by default in query interfaces.
		/// </summary>
		public bool ShowByDefault => false;

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
		/// http://www.w3.org/2006/vcard/ns#
		/// </summary>
		public const string Namespace = "http://www.w3.org/2006/vcard/ns#";
	}
}
