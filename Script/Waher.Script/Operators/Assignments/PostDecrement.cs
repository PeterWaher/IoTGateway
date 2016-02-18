using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Post-Decrement operator.
	/// </summary>
	public class PostDecrement : ScriptNode
	{
		private string variableName;
		
		/// <summary>
		/// Post-Decrement operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public PostDecrement(string VariableName, int Start, int Length)
			: base(Start, Length)
		{
			this.variableName = VariableName;
		}

		/// <summary>
		/// Name of variable
		/// </summary>
		public string VariableName
		{
			get { return this.variableName; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			Variable v;

			if (!Variables.TryGetVariable(this.variableName, out v))
				throw new ScriptRuntimeException("Variable not found: " + this.variableName, this);

			IElement Value = v.ValueElement;
			IElement Value2;
			DoubleNumber n = Value as DoubleNumber;

			if (n != null)
				Value2 = new DoubleNumber(n.Value - 1);
			else
				Value2 = PreDecrement.Decrement(Value, this);

			v.ValueElement = Value2;

			return Value;
		}

	}
}
