using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
	/// </summary>
	public class Concat : FunctionMultiVariate, IIterativeEvaluator
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
		/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="Arguments">Arguments</param>
		/// <param name="ArgumentTypes">Argument Types</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		private Concat(ScriptNode[] Arguments, ArgumentType[] ArgumentTypes, 
			int Start, int Length, Expression Expression)
			: base(Arguments, ArgumentTypes, Start, Length, Expression)
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

		#region IIterativeEvalautor

		private StringBuilder sb = null;
		private string delimiter = null;

		/// <summary>
		/// If the evaluator can perform the computation iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => this.Arguments.Length == 1 ||
			(this.Arguments.Length == 2 && this.Arguments[1] is ConstantElement);

		/// <summary>
		/// Creates a new instance of the iterative evaluator.
		/// </summary>
		/// <returns>Reference to new instance.</returns>
		public IIterativeEvaluator CreateNewEvaluator()
		{
			Concat Result = new Concat(this.Arguments, this.ArgumentTypes, this.Start, this.Length, this.Expression);

			if (this.Arguments.Length == 2 && this.Arguments[1] is ConstantElement C)
				Result.delimiter = C.Constant.AssociatedObjectValue?.ToString();

			return Result;
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.sb = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			if (this.sb is null)
				this.sb = new StringBuilder();
			else if (!(this.delimiter is null))
				this.sb.Append(this.delimiter);

			this.sb.Append(Element.AssociatedObjectValue?.ToString());
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			return new StringValue(this.sb?.ToString() ?? string.Empty);
		}

		#endregion

	}
}
