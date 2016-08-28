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
		private Unit unit;

		/// <summary>
		/// Sets a physical unit
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Unit">Unit to set.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
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
			DoubleNumber D = Operand as DoubleNumber;
			if (D != null)
				return new PhysicalQuantity(D.Value, this.unit);

			PhysicalQuantity Q = Operand as PhysicalQuantity;
			if (Q != null)
			{
				double Magnitude;

				if (Unit.TryConvert(Q.Magnitude, Q.Unit, this.unit, out Magnitude))
					return new PhysicalQuantity(Magnitude, this.unit);
				else
					throw new ScriptRuntimeException("Unable to convert from " + Q.Unit.ToString() + " to " + this.unit.ToString() + ".", this);
			}

			throw new ScriptRuntimeException("Unable to set physical unit.", this);
		}
	}
}
