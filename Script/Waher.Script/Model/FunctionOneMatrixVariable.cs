using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for funcions of one matrix variable.
	/// </summary>
	public abstract class FunctionOneMatrixVariable : FunctionOneVariable
	{
		/// <summary>
		/// Base class for funcions of one matrix variable.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FunctionOneMatrixVariable(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			if (Argument is IMatrix Matrix)
			{
				if (Matrix is DoubleMatrix DoubleMatrix)
					return this.EvaluateMatrix(DoubleMatrix, Variables);

				if (Matrix is ComplexMatrix ComplexMatrix)
					return this.EvaluateMatrix(ComplexMatrix, Variables);

				if (Matrix is BooleanMatrix BooleanMatrix)
					return this.EvaluateMatrix(BooleanMatrix, Variables);

				return this.EvaluateMatrix(Matrix, Variables);
			}
			else
				return this.EvaluateMatrix((IMatrix)MatrixDefinition.Encapsulate(new IElement[] { Argument }, 1, 1, this), Variables);
		}

		/// <summary>
		/// Evaluates the function on a matrix argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public abstract IElement EvaluateMatrix(IMatrix Argument, Variables Variables);

		/// <summary>
		/// Evaluates the function on a matrix argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public virtual IElement EvaluateMatrix(DoubleMatrix Argument, Variables Variables)
		{
			return this.EvaluateMatrix((IMatrix)Argument, Variables);
		}

		/// <summary>
		/// Evaluates the function on a matrix argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public virtual IElement EvaluateMatrix(ComplexMatrix Argument, Variables Variables)
		{
			return this.EvaluateMatrix((IMatrix)Argument, Variables);
		}

		/// <summary>
		/// Evaluates the function on a matrix argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public virtual IElement EvaluateMatrix(BooleanMatrix Argument, Variables Variables)
		{
			return this.EvaluateMatrix((IMatrix)Argument, Variables);
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "M" }; }
		}

	}
}
