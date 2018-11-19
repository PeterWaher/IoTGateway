using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Count(v)
	/// </summary>
	public class Count : FunctionOneVectorVariable
	{
		/// <summary>
		/// Count(v)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Count(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "count"; }
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
		{
			return new DoubleNumber(Argument.Dimension);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(IVector Argument, Variables Variables)
		{
			return new DoubleNumber(Argument.Dimension);
		}
	}
}