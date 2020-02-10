using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Units;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Sets a physical unit
	/// </summary>
	public class SetUnit : UnaryScalarOperator
	{
		private readonly Unit unit;

		/// <summary>
		/// Sets a physical unit
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Unit">Unit to set.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SetUnit(ScriptNode Operand, Unit Unit, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
			this.unit = Unit;
		}

		/// <summary>
		/// Unit to set.
		/// </summary>
		public Unit Unit
		{
			get { return this.unit; }
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Operand, Variables Variables)
		{
			if (Operand is DoubleNumber D)
				return new PhysicalQuantity(D.Value, this.unit);

			if (Operand is PhysicalQuantity Q)
			{
				if (Unit.TryConvert(Q.Magnitude, Q.Unit, this.unit, out double Magnitude))
					return new PhysicalQuantity(Magnitude, this.unit);
				else
					throw new ScriptRuntimeException("Unable to convert from " + Q.Unit.ToString() + " to " + this.unit.ToString() + ".", this);
			}

			throw new ScriptRuntimeException("Unable to set physical unit.", this);
		}
	}
}
