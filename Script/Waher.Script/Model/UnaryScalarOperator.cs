using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for unary scalar operators.
	/// </summary>
	public abstract class UnaryScalarOperator : UnaryOperator
	{
		/// <summary>
		/// Base class for unary scalar operators.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public UnaryScalarOperator(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
		}

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Operand">Operand.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override IElement Evaluate(IElement Operand, Variables Variables)
		{
			if (Operand.IsScalar)
				return this.EvaluateScalar(Operand, Variables);
			else
			{
				ChunkedList<IElement> Result = new ChunkedList<IElement>();

				foreach (IElement Child in Operand.ChildElements)
					Result.Add(this.Evaluate(Child, Variables));

				return Operand.Encapsulate(Result, this);
			}
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override async Task<IElement> EvaluateAsync(IElement Operand, Variables Variables)
		{
			if (Operand.IsScalar)
				return await this.EvaluateScalarAsync(Operand, Variables);
			else
			{
				ChunkedList<IElement> Result = new ChunkedList<IElement>();

				foreach (IElement Child in Operand.ChildElements)
					Result.Add(await this.EvaluateAsync(Child, Variables));

				return Operand.Encapsulate(Result, this);
			}
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public abstract IElement EvaluateScalar(IElement Operand, Variables Variables);

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public virtual Task<IElement> EvaluateScalarAsync(IElement Operand, Variables Variables)
		{
			return Task.FromResult<IElement>(this.EvaluateScalar(Operand, Variables));
		}
	}
}
