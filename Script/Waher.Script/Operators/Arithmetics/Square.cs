using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Square operator.
	/// </summary>
	public class Square : UnaryOperator, IDifferentiable
	{
		/// <summary>
		/// Square operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Square(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Operand = this.op.Evaluate(Variables);

			return this.Evaluate(Operand);
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public virtual IElement Evaluate(IElement Operand)
		{
			if (Operand is DoubleNumber DOp)
			{
				double d = DOp.Value;
				return new DoubleNumber(d * d);
			}

			if (Operand is IRingElement E)
				return Multiply.EvaluateMultiplication(E, E, this);

			if (Operand.IsScalar)
				throw new ScriptRuntimeException("Scalar operands must be double values or ring elements.", this);

			LinkedList<IElement> Result = new LinkedList<IElement>();

			foreach (IElement Child in Operand.ChildElements)
				Result.AddLast(this.Evaluate(Child));

			return Operand.Encapsulate(Result, this);
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			int Start = this.Start;
			int Len = this.Length;
			Expression Expression = this.Expression;

			return this.DifferentiationChainRule(VariableName, Variables, this.op,
				new Multiply(
					new ConstantElement(DoubleNumber.TwoElement, Start, Len, Expression),
					this.op,
					Start, Len, Expression));
		}

	}
}
