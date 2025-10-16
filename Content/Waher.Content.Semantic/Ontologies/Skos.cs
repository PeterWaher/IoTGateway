using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// SKOS Simple Knowledge Organization System Ontology
	/// https://www.w3.org/TR/skos-reference/
	/// </summary>
	public class Skos : IOntology
	{
		/// <summary>
		/// SKOS Simple Knowledge Organization System Ontology
		/// </summary>
		public Skos()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "skos";

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
		/// http://www.w3.org/2004/02/skos/core#
		/// </summary>
		public const string Namespace = "http://www.w3.org/2004/02/skos/core#";

		/// <summary>
		/// http://www.w3.org/2000/01/rdf-schema#label
		/// </summary>
		public static readonly Uri Label = new Uri(Namespace + "label");

		/// <summary>
		///  Section 3. The skos:Concept Class
		/// </summary>
		public static readonly Uri Concept = new Uri(Namespace + "Concept");
		
		/// <summary>
		///  Section 4. Concept Schemes
		/// </summary>
		public static readonly Uri ConceptScheme = new Uri(Namespace + "ConceptScheme");
		
		/// <summary>
		/// Section 4. Concept Schemes
		/// </summary>
		public static readonly Uri inScheme = new Uri(Namespace + "inScheme");
		
		/// <summary>
		/// Section 4. Concept Schemes
		/// </summary>
		public static readonly Uri hasTopConcept = new Uri(Namespace + "hasTopConcept");
		
		/// <summary>
		/// Section 4. Concept Schemes
		/// </summary>
		public static readonly Uri topConceptOf = new Uri(Namespace + "topConceptOf");
		
		/// <summary>
		///  Section 5. Lexical Labels
		/// </summary>
		public static readonly Uri altLabel = new Uri(Namespace + "altLabel");
		
		/// <summary>
		/// Section 5. Lexical Labels
		/// </summary>
		public static readonly Uri hiddenLabel = new Uri(Namespace + "hiddenLabel");
		
		/// <summary>
		/// Section 5. Lexical Labels
		/// </summary>
		public static readonly Uri prefLabel = new Uri(Namespace + "prefLabel");
		
		/// <summary>
		/// Section 6. Notations
		/// </summary>
		public static readonly Uri notation = new Uri(Namespace + "notation");
		
		/// <summary>
		/// Section 7. Documentation Properties
		/// </summary>
		public static readonly Uri changeNote = new Uri(Namespace + "changeNote");
		
		/// <summary>
		/// Section 7. Documentation Properties
		/// </summary>
		public static readonly Uri definition = new Uri(Namespace + "definition");
		
		/// <summary>
		/// Section 7. Documentation Properties
		/// </summary>
		public static readonly Uri editorialNote = new Uri(Namespace + "editorialNote");
		
		/// <summary>
		/// Section 7. Documentation Properties
		/// </summary>
		public static readonly Uri example = new Uri(Namespace + "example");
		
		/// <summary>
		/// Section 7. Documentation Properties
		/// </summary>
		public static readonly Uri historyNote = new Uri(Namespace + "historyNote");
		
		/// <summary>
		/// Section 7. Documentation Properties
		/// </summary>
		public static readonly Uri note = new Uri(Namespace + "note");
		
		/// <summary>
		/// Section 7. Documentation Properties
		/// </summary>
		public static readonly Uri scopeNote = new Uri(Namespace + "scopeNote");
		
		/// <summary>
		/// Section 8. Semantic Relations
		/// </summary>
		public static readonly Uri broader = new Uri(Namespace + "broader");
		
		/// <summary>
		/// Section 8. Semantic Relations
		/// </summary>
		public static readonly Uri broaderTransitive = new Uri(Namespace + "broaderTransitive");
		
		/// <summary>
		/// Section 8. Semantic Relations
		/// </summary>
		public static readonly Uri narrower = new Uri(Namespace + "narrower");
		
		/// <summary>
		/// Section 8. Semantic Relations
		/// </summary>
		public static readonly Uri narrowerTransitive = new Uri(Namespace + "narrowerTransitive");
		
		/// <summary>
		/// Section 8. Semantic Relations
		/// </summary>
		public static readonly Uri related = new Uri(Namespace + "related");
		
		/// <summary>
		/// Section 8. Semantic Relations
		/// </summary>
		public static readonly Uri semanticRelation = new Uri(Namespace + "semanticRelation");
		
		/// <summary>
		/// Section 9. Concept Collections
		/// </summary>
		public static readonly Uri Collection = new Uri(Namespace + "Collection");
		
		/// <summary>
		/// Section 9. Concept Collections
		/// </summary>
		public static readonly Uri OrderedCollection = new Uri(Namespace + "OrderedCollection");
		
		/// <summary>
		/// Section 9. Concept Collections
		/// </summary>
		public static readonly Uri member = new Uri(Namespace + "member");
		
		/// <summary>
		/// Section 9. Concept Collections
		/// </summary>
		public static readonly Uri memberList = new Uri(Namespace + "memberList");
		
		/// <summary>
		/// Section 10. Mapping Properties
		/// </summary>
		public static readonly Uri broadMatch = new Uri(Namespace + "broadMatch");
		
		/// <summary>
		/// Section 10. Mapping Properties
		/// </summary>
		public static readonly Uri closeMatch = new Uri(Namespace + "closeMatch");
		
		/// <summary>
		/// Section 10. Mapping Properties
		/// </summary>
		public static readonly Uri exactMatch = new Uri(Namespace + "exactMatch");
		
		/// <summary>
		/// Section 10. Mapping Properties
		/// </summary>
		public static readonly Uri mappingRelation = new Uri(Namespace + "mappingRelation");
		
		/// <summary>
		/// Section 10. Mapping Properties
		/// </summary>
		public static readonly Uri narrowMatch = new Uri(Namespace + "narrowMatch");
		
		/// <summary>
		/// Section 10. Mapping Properties
		/// </summary>
		public static readonly Uri relatedMatch = new Uri(Namespace + "relatedMatch");
	}
}
