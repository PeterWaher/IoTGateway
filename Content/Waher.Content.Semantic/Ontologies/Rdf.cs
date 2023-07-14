using System;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// RDF Schema Ontology
	/// </summary>
	public static class Rdf
	{
		/// <summary>
		/// http://www.w3.org/1999/02/22-rdf-syntax-ns#
		/// </summary>
		public const string Namespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

		/// <summary>
		/// URI representing rdf:type
		/// </summary>
		public static readonly Uri Type = new Uri(Namespace + "type");

		/// <summary>
		/// URI representing rdf:first
		/// </summary>
		public static readonly Uri First = new Uri(Namespace + "first");

		/// <summary>
		/// URI representing rdf:rest
		/// </summary>
		public static readonly Uri Rest = new Uri(Namespace + "rest");

		/// <summary>
		/// URI representing rdf:nil
		/// </summary>
		public static readonly Uri Nil = new Uri(Namespace + "nil");

		/// <summary>
		/// URI representing rdf:subject
		/// </summary>
		public static readonly Uri Subject = new Uri(Namespace + "subject");

		/// <summary>
		/// URI representing rdf:predicate
		/// </summary>
		public static readonly Uri Predicate = new Uri(Namespace + "predicate");

		/// <summary>
		/// URI representing rdf:object
		/// </summary>
		public static readonly Uri Object = new Uri(Namespace + "object");

		/// <summary>
		/// URI representing rdf:Statement
		/// </summary>
		public static readonly Uri Statement = new Uri(Namespace + "Statement");

		/// <summary>
		/// URI representing rdf:Bag
		/// </summary>
		public static readonly Uri Bag = new Uri(Namespace + "Bag");

		/// <summary>
		/// URI representing rdf:li
		/// </summary>
		public static readonly Uri Li = new Uri(Namespace + "li");

		/// <summary>
		/// URI representing an item in a list.
		/// </summary>
		/// <param name="Index">Index of item.</param>
		/// <returns>URI representing rdf:_Index</returns>
		public static Uri ListItem(int Index) => new Uri(listItem + Index.ToString());

		private const string listItem = Namespace + "_";
	}
}
