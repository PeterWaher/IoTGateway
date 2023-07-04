using System;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Contains a reference to a graph in the graph store.
	/// </summary>
	[CollectionName("GraphStore")]
	[TypeName(TypeNameSerialization.None)]
	[Index("GraphUri")]
	public class GraphReference
	{
		/// <summary>
		/// Contains a reference to a graph in the graph store.
		/// </summary>
		public GraphReference()
		{ 
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Graph URI
		/// </summary>
		public CaseInsensitiveString GraphUri { get; set; }

		/// <summary>
		/// Graph Digest, used to create file names.
		/// </summary>
		public CaseInsensitiveString GraphDigest { get; set; }

		/// <summary>
		/// Folder where graph(s) are stored.
		/// </summary>
		public string Folder { get; set; }

		/// <summary>
		/// When graph was created in graph store.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// When graph was updated in graph store.
		/// </summary>
		public DateTime Updated { get; set; }

		/// <summary>
		/// Number of files used to define graph.
		/// </summary>
		public int NrFiles { get; set; }
	}
}
