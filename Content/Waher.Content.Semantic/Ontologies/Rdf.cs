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
		public static readonly Uri type = new Uri(Namespace + "type");

		/// <summary>
		/// URI representing rdf:first
		/// </summary>
		public static readonly Uri first = new Uri(Namespace + "first");

		/// <summary>
		/// URI representing rdf:rest
		/// </summary>
		public static readonly Uri rest = new Uri(Namespace + "rest");

		/// <summary>
		/// URI representing rdf:nil
		/// </summary>
		public static readonly Uri nil = new Uri(Namespace + "nil");

		/// <summary>
		/// URI representing rdf:subject
		/// </summary>
		public static readonly Uri subject = new Uri(Namespace + "subject");

		/// <summary>
		/// URI representing rdf:predicate
		/// </summary>
		public static readonly Uri predicate = new Uri(Namespace + "predicate");

		/// <summary>
		/// URI representing rdf:object
		/// </summary>
		public static readonly Uri @object = new Uri(Namespace + "object");

		/// <summary>
		/// URI representing rdf:Statement
		/// </summary>
		public static readonly Uri Statement = new Uri(Namespace + "Statement");

		/// <summary>
		/// URI representing rdf:Seq, an ordered container.
		/// </summary>
		public static readonly Uri Seq = new Uri(Namespace + "Seq");

		/// <summary>
		/// URI representing rdf:Bag, an unordered container.
		/// </summary>
		public static readonly Uri Bag = new Uri(Namespace + "Bag");

		/// <summary>
		/// URI representing rdf:Alt, an unordered set of alternatives, where the first
		/// one is the default option.
		/// </summary>
		public static readonly Uri Alt = new Uri(Namespace + "Alt");

		/// <summary>
		/// URI representing rdf:List
		/// </summary>
		public static readonly Uri List = new Uri(Namespace + "List");

		/// <summary>
		/// URI representing rdf:li
		/// </summary>
		public static readonly Uri li = new Uri(Namespace + "li");

		/// <summary>
		/// URI representing an item in a list.
		/// </summary>
		/// <param name="Index">Index of item.</param>
		/// <returns>URI representing rdf:_Index</returns>
		public static Uri ListItem(int Index) => new Uri(listItem + Index.ToString());

		private const string listItem = Namespace + "_";
	}
}
