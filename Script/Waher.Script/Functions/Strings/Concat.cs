using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
	/// </summary>
	public class Concat : FunctionMultiVariate, IIterativeEvaluation
	{
		/// <summary>
		/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="Vector">Vector of elements to concatenate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Concat(ScriptNode Vector, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector }, new ArgumentType[] { ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="Vector">Vector of elements to concatenate.</param>
		/// <param name="Delimiter">Optional delimiter.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Concat(ScriptNode Vector, ScriptNode Delimiter, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Delimiter }, new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Concat);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Vector", "Delimiter" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0] is IVector Vector))
				return Arguments[0];

			string Delimiter = Arguments.Length > 1 ? Arguments[1].AssociatedObjectValue?.ToString() : null;

			StringBuilder Result = new StringBuilder();
			bool First = true;

			foreach (IElement Item in Vector.VectorElements)
			{
				if (!(Item.AssociatedObjectValue is string s))
					s = Item.AssociatedObjectValue?.ToString() ?? string.Empty;

				if (First)
					First = false;
				else if (!(Delimiter is null))
					Result.Append(Delimiter);

				Result.Append(s);
			}

			return new StringValue(Result.ToString());
		}

		#region IIterativeEvalaution

		/// <summary>
		/// If the node can be evaluated iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => this.Arguments.Length == 1 ||
			(this.Arguments.Length == 2 && this.Arguments[1] is ConstantElement);

		/// <summary>
		/// Creates an iterative evaluator for the node.
		/// </summary>
		/// <returns>Iterative evaluator reference.</returns>
		public IIterativeEvaluator CreateEvaluator()
		{
			if (this.Arguments.Length == 2 && this.Arguments[1] is ConstantElement C)
				return new ConcatEvaluator(C.Constant.AssociatedObjectValue?.ToString());
			else
				return new ConcatEvaluator(null);
		}

		#endregion
	}
}
