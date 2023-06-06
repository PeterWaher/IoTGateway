using Waher.Content.Semantic;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Semantic element based on script.
	/// </summary>
	public class SemanticScriptElement : ISemanticElement
	{
		private readonly ScriptNode node;

		/// <summary>
		/// Semantic element based on script.
		/// </summary>
		/// <param name="Node">Script definition of semantic element.</param>
		public SemanticScriptElement(ScriptNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Property used by processor, to tag information to an element.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public bool IsLiteral => false;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is SemanticScriptElement Typed &&
				this.node.SubExpression.Equals(Typed.node.SubExpression);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.node.SubExpression.GetHashCode();
		}

		/// <summary>
		/// Compares element to another.
		/// </summary>
		/// <param name="obj">Second element.</param>
		/// <returns>Comparison result.</returns>
		public int CompareTo(object obj)
		{
			if (obj is SemanticScriptElement Typed)
				return this.node.SubExpression.CompareTo(Typed.node.SubExpression);
			else
				return -1;
		}
	}
}
