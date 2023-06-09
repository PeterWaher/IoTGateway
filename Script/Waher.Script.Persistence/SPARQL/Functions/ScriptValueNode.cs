using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SPARQL.Functions
{
	/// <summary>
	/// Makes sure a semantic literal is reduced to its element value.
	/// </summary>
	public class ScriptValueNode : UnaryOperator
	{
		/// <summary>
		/// Makes sure a semantic literal is reduced to its element value.
		/// </summary>
		/// <param name="Operand">Operand</param>
		public ScriptValueNode(ScriptNode Operand)
			: base(Operand, Operand.Start, Operand.Length, Operand.Expression)
		{
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => this.op.IsAsynchronous;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(IElement Operand, Variables Variables)
		{
			if (Operand is ObjectValue v && v.AssociatedObjectValue is ISemanticElement E)
				return Expression.Encapsulate(E.ElementValue);
			else
				return Operand;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(IElement Operand, Variables Variables)
		{
			if (Operand is ObjectValue v && v.AssociatedObjectValue is ISemanticElement E)
				return Task.FromResult(Expression.Encapsulate(E.ElementValue));
			else
				return Task.FromResult(Operand);
		}
	}
}
