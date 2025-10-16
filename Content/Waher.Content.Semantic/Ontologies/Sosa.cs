using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// SOSA/SSN (Sensor, Observation, Sample)
	/// </summary>
	public class Sosa : IOntology
	{
		/// <summary>
		/// SOSA/SSN (Sensor, Observation, Sample)
		/// </summary>
		public Sosa()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "sosa";

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
		/// http://www.w3.org/ns/sosa/
		/// </summary>
		public const string Namespace = "http://www.w3.org/ns/sosa/";
	}
}
