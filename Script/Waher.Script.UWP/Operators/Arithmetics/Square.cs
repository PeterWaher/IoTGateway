using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Square operator.
	/// </summary>
	public class Square : UnaryOperator
	{
		/// <summary>
		/// Square operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
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
			DoubleNumber DOp = Operand as DoubleNumber;
			if (DOp != null)
			{
				double d = DOp.Value;
				return new DoubleNumber(d * d);
			}

			IRingElement E = Operand as IRingElement;
			if (E != null)
				return Multiply.EvaluateMultiplication(E, E, this);

			if (Operand.IsScalar)
				throw new ScriptRuntimeException("Scalar operands must be double values or ring elements.", this);

			LinkedList<IElement> Result = new LinkedList<IElement>();

			foreach (IElement Child in Operand.ChildElements)
				Result.AddLast(this.Evaluate(Child));

			return Operand.Encapsulate(Result, this);
		}

	}
}
