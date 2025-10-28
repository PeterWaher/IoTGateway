namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Represents a blank node
	/// </summary>
	public class BlankNode : SemanticElement
	{
		/// <summary>
		/// Represents a blank node
		/// </summary>
		public BlankNode()
			: this(null)
		{
		}

		/// <summary>
		/// Represents a blank node
		/// </summary>
		/// <param name="NodeId">Blank-node Node ID in document.</param>
		public BlankNode(string NodeId)
		{
			this.NodeId = NodeId;
		}

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public override bool IsLiteral => false;

		/// <summary>
		/// Blank node Node ID.
		/// </summary>
		public string NodeId { get; set; }

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => this;

		/// <inheritdoc/>
		public override string ToString()
		{
			return "_:" + this.NodeId;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is BlankNode Typed &&
				Typed.NodeId == this.NodeId;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.NodeId.GetHashCode();
		}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns
		/// an integer that indicates whether the current instance precedes, follows, or
		/// occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The
		/// return value has these meanings: Value Meaning Less than zero This instance precedes
		/// obj in the sort order. Zero This instance occurs in the same position in the
		/// sort order as obj. Greater than zero This instance follows obj in the sort order.</returns>
		/// <exception cref="System.ArgumentException">obj is not the same type as this instance.</exception>
		public override int CompareTo(object obj)
		{
			if (obj is BlankNode Typed)
				return this.NodeId.CompareTo(Typed.NodeId);
			else
				return base.CompareTo(obj);
		}
	}
}
