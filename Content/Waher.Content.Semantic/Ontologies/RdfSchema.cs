﻿using System;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// RDF Schema Ontology
	/// </summary>
	public static class RdfSchema
	{
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
