using System;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Column Vector operator.
	/// </summary>
	public class ColumnVector : NullCheckBinaryOperator
	{
		/// <summary>
		/// Column Vector operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="X">X-coordinate operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ColumnVector(ScriptNode Left, ScriptNode X, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Left, X, NullCheck, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Left = this.left.Evaluate(Variables);
			if (this.nullCheck && Left.AssociatedObjectValue is null)
				return Left;

			IElement Right = this.right.Evaluate(Variables);

			return EvaluateIndex(Left, Right, this.nullCheck, this);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement Left = await this.left.EvaluateAsync(Variables);
			if (this.nullCheck && Left.AssociatedObjectValue is null)
				return Left;

			IElement Right = await this.right.EvaluateAsync(Variables);

			return EvaluateIndex(Left, Right, this.nullCheck, this);
		}
		/// <summary>
		/// Evaluates the column index operator.
		/// </summary>
		/// <param name="Matrix">Matrix</param>
		/// <param name="Index">Index</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateIndex(IElement Matrix, IElement Index, bool NullCheck, ScriptNode Node)
		{
			if (Matrix is IMatrix M)
				return EvaluateIndex(M, Index, Node);
			else if (Matrix.IsScalar)
			{
				if (NullCheck && Matrix.AssociatedObjectValue is null)
					return Matrix;

				throw new ScriptRuntimeException("The column index operator operates on matrices.", Node);
			}
			else
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (IElement E in Matrix.ChildElements)
					Elements.Add(EvaluateIndex(E, Index, NullCheck, Node));

				return Matrix.Encapsulate(Elements, Node);
			}
		}

		/// <summary>
		/// Evaluates the column index operator.
		/// </summary>
		/// <param name="Matrix">Matrix</param>
		/// <param name="Index">Index</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateIndex(IMatrix Matrix, IElement Index, ScriptNode Node)
		{
			if (Index.AssociatedObjectValue is double d)
			{
				if (d < 0 || d > int.MaxValue || d != Math.Truncate(d))
					throw new ArgumentNonNegativeIntegerScriptException("Column Index", Node);

				return Matrix.GetColumn((int)d);
			}

			if (Index.IsScalar)
				throw new ArgumentNonNegativeIntegerScriptException("Column Index", Node);
			else
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (IElement E in Index.ChildElements)
					Elements.Add(EvaluateIndex(Matrix, E, Node));

				return Index.Encapsulate(Elements, Node);
			}
		}

	}
}
