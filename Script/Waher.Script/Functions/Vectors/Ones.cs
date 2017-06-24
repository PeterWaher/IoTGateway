using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Creates a vector containing only ones.
	/// </summary>
	public class Ones : FunctionOneScalarVariable
	{
		/// <summary>
		/// Creates a vector containing only ones.
		/// </summary>
		/// <param name="Dimension">Vector dimension.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Ones(ScriptNode Dimension, int Start, int Length, Expression Expression)
			: base(Dimension, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "Ones";
			}
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			double n = Expression.ToDouble(Argument.AssociatedObjectValue);
			int N = (int)n;

			if (N != n || N < 0)
				throw new ScriptRuntimeException("Dimension must be non-negative.", this);

			double[] E = new double[N];
			int i;

			for (i = 0; i < N; i++)
				E[i] = 1;

			return new DoubleVector(E);
		}
	}
}
