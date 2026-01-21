using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Or(v) iterative evaluator
	/// </summary>
	public class OrEvaluator : UnaryIterativeEvaluator
	{
		private ulong integerResult = 0;
		private bool integerSigned = false;
		private bool booleanResult = false;
		private bool isBoolean = false;
		private bool isInteger = false;
		private bool first = true;

		/// <summary>
		/// Or(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public OrEvaluator(Or Node)
			: base(Node.Argument)
		{
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override void RestartEvaluator()
		{
			this.integerResult = 0;
			this.booleanResult = false;
			this.isInteger = false;
			this.isBoolean = false;
			this.first = true;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
			if (this.first)
			{
				this.first = false;

				if (Element is BooleanValue B)
				{
					this.isBoolean = true;
					this.booleanResult = B.Value;
				}
				else if (Element is DoubleNumber D)
				{
					this.isInteger = true;
					this.integerResult = Operators.Binary.And.ToUInt64(D.Value,
						out this.integerSigned, this.Node);
				}
				else
					throw new ScriptRuntimeException("Operands must be integer or Boolean values.", this.Node);
			}
			else if (this.isBoolean)
			{
				if (Element is BooleanValue B)
					this.booleanResult |= B.Value;
				else
					throw new ScriptRuntimeException("Operands do not match.", this.Node);
			}
			else if (this.isInteger)
			{
				if (Element is DoubleNumber D)
				{
					ulong Value = Operators.Binary.And.ToUInt64(D.Value, out bool Signed, this.Node);

					this.integerResult |= Value;
					this.integerSigned |= Signed;
				}
				else
					throw new ScriptRuntimeException("Operands do not match.", this.Node);
			}
			else
				throw new ScriptRuntimeException("Operands must be integer or Boolean values.", this.Node);
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
			if (this.first)
				return ObjectValue.Null;
			else if (this.isBoolean)
				return this.booleanResult ? BooleanValue.True : BooleanValue.False;
			else if (this.isInteger)
			{
				if (this.integerSigned)
					return new DoubleNumber((long)this.integerResult);
				else
					return new DoubleNumber(this.integerResult);
			}
			else
				throw new ScriptRuntimeException("Operands must be integer or Boolean values.", this.Node);
		}
	}
}
