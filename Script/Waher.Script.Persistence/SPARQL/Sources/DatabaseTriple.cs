using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Persistence.Attributes;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Contains a reference to a graph in the graph store.
	/// </summary>
	[CollectionName("GraphStoreTriples")]
	[TypeName(TypeNameSerialization.None)]
	[Index("GraphKey", "S", "P", "O")]
	[Index("GraphKey", "S", "O", "P")]
	[Index("GraphKey", "P", "S", "O")]
	[Index("GraphKey", "O", "S", "P")]
	[Index("GraphKey", "P", "O", "S")]
	[Index("GraphKey", "O", "P", "S")]
	public class DatabaseTriple : ISemanticTriple
	{
		/// <summary>
		/// Contains a reference to a graph in the graph store.
		/// </summary>
		public DatabaseTriple()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Graph key
		/// </summary>
		public long GraphKey { get; set; }

		/// <summary>
		/// Subject element
		/// </summary>
		public SemanticElement S { get; set; }

		/// <summary>
		/// Predicate element
		/// </summary>
		public SemanticElement P { get; set; }

		/// <summary>
		/// Object element
		/// </summary>
		public SemanticElement O { get; set; }

		/// <summary>
		/// Subject element
		/// </summary>
		public ISemanticElement Subject => this.S;

		/// <summary>
		/// Predicate element
		/// </summary>
		public ISemanticElement Predicate => this.P;

		/// <summary>
		/// Object element
		/// </summary>
		public ISemanticElement Object => this.O;

		/// <summary>
		/// Access to elements: 0=Subject, 1=Predicate, 2=Object.
		/// </summary>
		/// <param name="Index">0=Subject, 1=Predicate, 2=Object</param>
		/// <returns>Corresponding semantic element.</returns>
		public ISemanticElement this[int Index]
		{
			get
			{
				switch (Index)
				{
					case 0: return this.S;
					case 1: return this.P;
					case 2: return this.O;
					default: return null;
				}
			}
		}
	}
}
