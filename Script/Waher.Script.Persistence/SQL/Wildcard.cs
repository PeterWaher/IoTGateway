using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Represents the Wildcard symbol *
	/// </summary>
	public class Wildcard : ScriptLeafNode, IIterativeEvaluation
	{
		/// <summary>
		/// Represents the Wildcard symbol *
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Wildcard(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (Variables is ObjectProperties Properties)
				return Expression.Encapsulate(Properties.Object);
			else
				return ObjectValue.Null;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			return Task.FromResult(this.Evaluate(Variables));
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is Wildcard;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return "*".GetHashCode();
		}

		#region IIterativeEvaluation

		/// <summary>
		/// If the node can be evaluated iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => true;

		/// <summary>
		/// Creates an iterative evaluator for the node.
		/// </summary>
		/// <returns>Iterative evaluator reference.</returns>
		public IIterativeEvaluator CreateEvaluator()
		{
			return new WildcardEvaluator(this);
		}

		#endregion
	}
}
