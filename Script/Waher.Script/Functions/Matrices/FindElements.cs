using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Matrices
{
	/// <summary>
	/// Finds elements in matrices or vectors.
	/// </summary>
	public class FindElements : FunctionMultiVariate
	{
		/// <summary>
		/// Finds elements in matrices or vectors.
		/// </summary>
		/// <param name="SearchFor">What to search for.</param>
		/// <param name="In">Object to search in.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public FindElements(ScriptNode SearchFor, ScriptNode In, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { SearchFor, In }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(FindElements);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "SearchFor", "In" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			IElement SearchFor = Arguments[0];
			IElement In = Arguments[1];

			if (In is IMatrix M)
			{
				List<IElement> Coordinates = new List<IElement>();
				int Rows = M.Rows;
				int Columns = M.Columns;
				int x, y;

				for (y = 0; y < Rows; y++)
				{
					for (x = 0; x < Columns; x++)
					{
						if (SearchFor.Equals(M.GetElement(x, y)))
						{
							Coordinates.Add(new DoubleNumber(x));
							Coordinates.Add(new DoubleNumber(y));
						}
					}
				}

				return new DoubleMatrix(Coordinates.Count / 2, 2, Coordinates);
			}
			else if (In is IVector V)
			{
				List<double> Indices = new List<double>();
				int i = 0;

				foreach (IElement Element in V.VectorElements)
				{
					if (SearchFor.Equals(Element))
						Indices.Add(i);

					i++;
				}

				return new DoubleVector(Indices.ToArray());
			}
			else if (In is ISet S)
			{
				List<double> Indices = new List<double>();
				int i = 0;

				foreach (IElement Element in S.ChildElements)
				{
					if (SearchFor.Equals(Element))
						Indices.Add(i);

					i++;
				}

				return new DoubleVector(Indices.ToArray());
			}
			else
				throw new ScriptRuntimeException("Expected second argument to be matrix, vector or set.", this);
		}
	}
}
