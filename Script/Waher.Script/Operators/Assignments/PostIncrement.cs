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
	/// Post-Increment operator.
	/// </summary>
	public class PostIncrement : ScriptLeafNode
	{
		private readonly string variableName;
		
		/// <summary>
		/// Post-Increment operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PostIncrement(string VariableName, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
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
			if (!Variables.TryGetVariable(this.variableName, out Variable v))
				throw new ScriptRuntimeException("Variable not found: " + this.variableName, this);

			IElement Value = v.ValueElement;
			IElement Value2;

			if (Value is DoubleNumber n)
				Value2 = new DoubleNumber(n.Value + 1);
			else
				Value2 = PreIncrement.Increment(Value, this);

			Variables[this.variableName] = Value2;

            return Value;
		}

	}
}
