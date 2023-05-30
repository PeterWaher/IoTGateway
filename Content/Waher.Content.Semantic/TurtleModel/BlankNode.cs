namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a blank node
	/// </summary>
	public class BlankNode : ISemanticElement
	{
		/// <summary>
		/// Represents a blank node
		/// </summary>
		/// <param name="Index">Blank-node index in document.</param>
		public BlankNode(int Index)
		{
			this.Index = Index;
		}

		/// <summary>
		/// Blank node index.
		/// </summary>
		public int Index { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return "_:n" + this.Index.ToString();
		}
	}
}
