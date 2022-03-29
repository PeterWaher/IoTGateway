using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Units;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Creates a measurement.
	/// </summary>
	public class CreateMeasurement : BinaryScalarOperator
	{
		/// <summary>
		/// Creates a measurement.
		/// </summary>
		/// <param name="Operand1">Operand 1.</param>
		/// <param name="Operand2">Operand 2.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateMeasurement(ScriptNode Operand1, ScriptNode Operand2, int Start, int Length, Expression Expression)
			: base(Operand1, Operand2, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand1">Operand 1.</param>
		/// <param name="Operand2">Operand 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Operand1, IElement Operand2, Variables Variables)
		{
			double Magnitude;
			Unit Unit;
			double Error;

			if (Operand1 is PhysicalQuantity Q1)
			{
				Magnitude = Q1.Magnitude;
				Unit = Q1.Unit;
			}
			else if (Operand1 is DoubleNumber D1)
			{
				Magnitude = D1.Value;
				Unit = Unit.Empty;
			}
			else
				throw new ScriptRuntimeException("Expected Physical Quantity our numeric value.", this.LeftOperand);

			if (Operand2 is PhysicalQuantity Q2)
			{
				if (Unit == Unit.Empty)
				{
					Error = Q2.Magnitude;
					Unit = Q2.Unit;
				}
				else if (!Unit.TryConvert(Q2.Magnitude, Q2.Unit, Unit, out Error))
					throw new ScriptRuntimeException("Incompatible error unit.", this);
			}
			else if (Operand2 is DoubleNumber D2)
				Error = D2.Value;
			else
				throw new ScriptRuntimeException("Expected Physical Quantity our numeric value.", this.RightOperand);

			return new Measurement(Magnitude, Unit, Error);
		}
	}
}
