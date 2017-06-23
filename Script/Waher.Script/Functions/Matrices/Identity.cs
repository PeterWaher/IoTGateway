using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Functions.Matries
{
	/// <summary>
	/// Creates an identity matrix.
	/// </summary>
	public class Identity : FunctionOneScalarVariable
	{
		/// <summary>
		/// Creates an identity matrix.
		/// </summary>
		/// <param name="Dimension">Dimension of matrix.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Identity(ScriptNode Dimension, int Start, int Length, Expression Expression)
			: base(Dimension, Start, Length, Expression)
		{
		}

		public override string FunctionName
		{
			get
			{
				return "Identity";
			}
		}

		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			double n = Expression.ToDouble(Argument.AssociatedObjectValue);
			int N = (int)n;

			if (N != n || N < 0)
				throw new ScriptRuntimeException("Dimension must be non-negative.", this);

			double[,] E = new double[N, N];
			int x, y;

			for (y = 0; y < N; y++)
			{
				for (x = 0; x < N; x++)
					E[y, x] = x == y ? 1 : 0;
			}

			return new DoubleMatrix(E);
		}
	}
}
