using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Represents a named node.
	/// </summary>
	public class NamedNode : BinaryOperator
	{
		/// <summary>
		/// Represents a named node.
		/// </summary>
		/// <param name="Left">Value node</param>
		/// <param name="Right">Name node</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NamedNode(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for nodes whose <see cref="ScriptNode.IsAsynchronous"/> is false.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.left.Evaluate(Variables);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for nodes whose <see cref="ScriptNode.IsAsynchronous"/> is true.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			return this.left.EvaluateAsync(Variables);
		}
	}
}
