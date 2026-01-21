using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Assignments.Post
{
	/// <summary>
	/// Post-Decrement operator.
	/// </summary>
	public class PostDecrement : ScriptLeafNodeVariableReference
	{
		/// <summary>
		/// Post-Decrement operator.
		/// </summary>
		/// <param name="VariableName">Variable name..</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PostDecrement(string VariableName, int Start, int Length, Expression Expression)
			: base(VariableName, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (!Variables.TryGetVariable(this.variableName, out Variable v))
				throw new VariableNotFoundScriptException(this.variableName, this);

			IElement Value = v.ValueElement;
			IElement Value2;

			if (Value.AssociatedObjectValue is double d)
				Value2 = new DoubleNumber(d - 1);
			else
				Value2 = Pre.PreDecrement.Decrement(Value, this);

            Variables[this.variableName] = Value2;

            return Value;
		}

	}
}
