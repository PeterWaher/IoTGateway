using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Vectors;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Matrices
{
	/// <summary>
	/// Determinant(x)
	/// </summary>
	public class Determinant : FunctionOneVariable
	{
		/// <summary>
		/// Determinant(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Determinant(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Determinant);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "det" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			if (Argument is IMatrix M)
			{
				int N = M.Columns;
				if (N != M.Rows)
					throw new ScriptRuntimeException("Expected square matrix argument.", this);

				IMatrix M2 = M.Reduce(false, true, out int Rank, out ICommutativeRingWithIdentityElement Factor);
				if (M2 is null || Rank < N || Factor is null)
					return Factor?.Zero ?? new DoubleNumber(0);
				else
					return Factor;
			}
			else
				throw new ScriptRuntimeException("Expected matrix argument.", this);
		}
	}
}
