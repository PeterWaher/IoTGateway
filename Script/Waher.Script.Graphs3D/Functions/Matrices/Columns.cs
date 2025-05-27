using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Graphs3D.Functions.Matrices
{
	/// <summary>
	/// Creates a matrix whose columns have elements of the same value, each defined by 
	/// the corresponding element in the input vector.
	/// </summary>
	public class Columns : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a matrix whose columns have elements of the same value, each defined by 
		/// the corresponding element in the input vector.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Columns(ScriptNode Vector, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a matrix whose columns have elements of the same value, each defined by 
		/// the corresponding element in the input vector.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="SecondDimension">Second dimension</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Columns(ScriptNode Vector, ScriptNode SecondDimension, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, SecondDimension },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Columns);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "M" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments.Length == 1 && Arguments[0] is IMatrix M)
				return new DoubleNumber(M.Columns);

			if (!(Arguments[0] is IVector V))
				throw new ScriptRuntimeException("Expected vector in first argument.", this);

			int SecondDimension;

			if (Arguments.Length == 2)
			{
				SecondDimension = (int)Expression.ToDouble(Arguments[1].AssociatedObjectValue);
				if (SecondDimension <= 0)
					throw new ScriptRuntimeException("Invalid secondary dimension.", this);
			}
			else
				SecondDimension = V.Dimension;

			if (V is DoubleVector DoubleVector)
			{
				double[] Values = DoubleVector.Values;
				int c = Values.Length;
				int i, j;
				double[,] Elements = new double[c, SecondDimension];
				double Value;

				for (i = 0; i < c; i++)
				{
					Value = Values[i];
					for (j = 0; j < SecondDimension; j++)
						Elements[i, j] = Value;
				}

				return new DoubleMatrix(Elements);
			}
			else if (V is ComplexVector ComplexVector)
			{
				Complex[] Values = ComplexVector.Values;
				int c = Values.Length;
				int i, j;
				Complex[,] Elements = new Complex[c, SecondDimension];
				Complex Value;

				for (i = 0; i < c; i++)
				{
					Value = Values[i];
					for (j = 0; j < SecondDimension; j++)
						Elements[i, j] = Value;
				}

				return new ComplexMatrix(Elements);
			}
			else if (V is BooleanVector BooleanVector)
			{
				bool[] Values = BooleanVector.Values;
				int c = Values.Length;
				int i, j;
				bool[,] Elements = new bool[c, SecondDimension];
				bool Value;

				for (i = 0; i < c; i++)
				{
					Value = Values[i];
					for (j = 0; j < SecondDimension; j++)
						Elements[i, j] = Value;
				}

				return new BooleanMatrix(Elements);
			}
			else 
			{
				int c = V.Dimension;
				int i, j;
				IElement[,] Elements = new IElement[c, SecondDimension];

				i = 0;
				foreach (IElement Value in V.VectorElements)
				{
					for (j = 0; j < SecondDimension; j++)
						Elements[i, j] = Value;

					i++;
				}

				return new ObjectMatrix(Elements);
			}
		}
	}
}
