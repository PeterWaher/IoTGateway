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
				IElement[,] Elements = new IElement[Rows, Columns];
				IElement[,] Elements0 = M.Values;
				int Column, Row;

				Names[0] = Names0[0];
				for (Column = 1; Column < Columns; Column++)
					Names[Column] = Elements0[Column - 1, 0].AssociatedObjectValue?.ToString();

				for (Row = 0; Row < Rows; Row++)
					Elements[Row, 0] = new StringValue(Names0[Row + 1]);

				for (Column = 1; Column < Columns; Column++)
				{
					for (Row = 0; Row < Rows; Row++)
						Elements[Row, Column] = Elements0[Column - 1, Row + 1];
				}

				return new ObjectMatrix(Elements)
				{
					ColumnNames = Names
				};
			}
			else
				return Argument.Transpose();
		}

	}
}
