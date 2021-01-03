using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Pivots a result matrix so columns become rows, and vice versa. It is similar to the matrix transpose operator, except it takes
	/// column headers into account also.
	/// </summary>
	public class Pivot : FunctionOneMatrixVariable
	{
		/// <summary>
		/// Pivots a result matrix so columns become rows, and vice versa. It is similar to the matrix transpose operator, except it takes
		/// column headers into account also.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Pivot(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "Pivot";
			}
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Object" };
			}
		}

		/// <summary>
		/// Evaluates the function on a matrix argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateMatrix(IMatrix Argument, Variables Variables)
		{
			if (Argument is ObjectMatrix M && M.HasColumnNames)
			{
				int Rows = M.Columns - 1;
				int Columns = M.Rows + 1;
				string[] Names = new string[Columns];
				string[] Names0 = M.ColumnNames;
				IElement[,] Elements = new IElement[Columns, Rows];
				IElement[,] Elements0 = M.Values;
				int x, y;

				Names[0] = Names0[0];
				for (x = 1; x < Columns; x++)
					Names[x] = Elements[0, x - 1].AssociatedObjectValue?.ToString();

				for (y = 0; y < Rows; y++)
					Elements[0, y] = new StringValue(Names0[y + 1]);

				for (x = 1; x < Columns; x++)
				{
					for (y = 0; y < Rows; y++)
						Elements[x, y] = Elements0[y + 1, x - 1];
				}

				return new ObjectMatrix(Elements);
			}
			else
				return Argument.Transpose();
		}

	}
}
