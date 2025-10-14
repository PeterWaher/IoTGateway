using System;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// Dublin Core Ontology
	/// </summary>
	public static class DublinCore
	{
		/// <summary>
		/// Dublin Core Terms namespace
		/// </summary>
		public static class Terms
		{
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

		/// <summary>
		/// Dublin Core Metadata Initiative (DCMI) namespace
		/// </summary>
		public static class MetadataInitiative
		{
			/// Dublin Core Metadata Initiative (DCMI) type namespace
			public static class Type
			{
				/// <summary>
				/// http://purl.org/dc/dcmitype/
				/// </summary>
				public const string Namespace = "http://purl.org/dc/dcmitype/";

				/// <summary>
				/// http://purl.org/dc/dcmitype/Dataset
				/// </summary>
				public static readonly Uri Dataset = new Uri(Namespace + "Dataset");
			}
		}
	}
}
